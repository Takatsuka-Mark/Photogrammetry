from cv2 import Mat, cvtColor, COLOR_BGR2GRAY, imwrite
import numpy as np
import time

"""
https://homepages.inf.ed.ac.uk/rbf/CVonline/LOCAL_COPIES/AV1011/AV1FeaturefromAcceleratedSegmentTest.pdf
"""

# TODO dynamically create?
# Points 1 to 16, in format (height_offset, width_offset) relative to P 
BRESENHAM_CIRCLE_3 = np.array([
    [-3, 0],
    [-3, 1],
    [-2, 2],
    [-1, 3],
    [0, 3],
    [1, 3],
    [2, 2],
    [3, 1],
    [3, 0],
    [3, -1],
    [2, -2],
    [1, -3],
    [0, -3],
    [-1, -3],
    [-2, -2],
    [-3, -1],
])
# Going from [[x, y], [x2, y2], ...] to [[x, x2, ...], [y, y2, ...]]
BRESENHAM_CIRCLE_3_TP = BRESENHAM_CIRCLE_3.transpose((1, 0))


def fast_detection(image: Mat, threshold: int = 10):
    img_height, img_width, _ = image.shape

    # TODO determine if BGR is correct color mapping.
    bw_img = cvtColor(image, COLOR_BGR2GRAY)

    # contents like [(height, width), (height2, width2)]
    keypoints = []

    time_acc = 0

    # TODO we are clamping the bounds here, but this will throw keypoints on edges out. Fix this (needs fixing when fetching bresenham circle)
    for x in range(3, img_height-3):
        for y in range(3, img_width-3):
            # x and y are coords of p

            # x = 10
            # y = 10

            # Known key point top
            # x = 35
            # y = 202

            # Known bottom key point
            # x = 340
            # y = 213

            # intensity at p
            ip = bw_img[x, y]
            # Ring fetch taking ~0.285 seconds.
            intensity_ring = fetch_bresenham_circle(bw_img, x, y)
            
            now = time.time()
            # Is keypoint taking ~0.925 seconds
            if is_keypoint(ip, intensity_ring, threshold):
                keypoints.append([x, y])
            time_acc += (time.time() - now)
    print(time_acc)
    return keypoints

def in_threshold(principal_intensity, test_intensity, threshold: int):
    return (test_intensity > (principal_intensity - threshold)) and (test_intensity < (principal_intensity + threshold))

def in_threshold_percentage(principal_intensity, test_intensity, threshold: float):
    thresh = principal_intensity * threshold
    return (test_intensity > (principal_intensity - thresh)) and (test_intensity < (principal_intensity + thresh))

# Note, all of these point groupings could be calculated when initialized. Then, when running on an image we just have to fetch points with bw_img[pts[0], pts[1]]
def fetch_bresenham_circle(bw_img: Mat, x, y):
    ring_points = BRESENHAM_CIRCLE_3_TP + np.array([[x], [y]])
    return bw_img[ring_points[0], ring_points[1]]

# TODO vectorizing like this does work...
# in_threshold_ring_func = np.vectorize(lambda intensity, ip, threshold: in_threshold(ip, intensity, threshold))

def is_keypoint(principal_intensity: int, intensity_ring: np.ndarray, threshold: int) -> bool:
    # Quick test to see if it's possible.

    quick_test_outside_thresh = 0
    for circle_idx in [0, 4, 8, 12]:    # TODO make dynamic if making circle dynamic
        if in_threshold(principal_intensity, intensity_ring[circle_idx], threshold):
            continue
        quick_test_outside_thresh += 1

    if quick_test_outside_thresh < 3:
        # point rejected, not a corner
        return False

    is_beginning_consec = True
    num_beginning_consec = 0
    num_consec = 0
    for intensity in intensity_ring:
        if in_threshold(principal_intensity, intensity, threshold):
            # We've broken the streak
            is_beginning_consec = False
            num_consec = 0
        else:
            num_consec += 1
            if is_beginning_consec:
                num_beginning_consec += 1
            if num_consec >= 12:
                return True
    # We end with the number of consecutive outside the threshold at the end of the ring.
    # So, adding on the beginning completes that "run" if it exists.
    num_consec += num_beginning_consec
    return num_consec >= 12
