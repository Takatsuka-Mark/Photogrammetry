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


def fast_detection(image: Mat, threshold: int = 10):
    img_height, img_width, _ = image.shape

    # TODO determine if BGR is correct color mapping.
    bw_img = cvtColor(image, COLOR_BGR2GRAY)

    # contents like [(height, width), (height2, width2)]
    keypoints = []

    time_acc = 0

    for x in range(0, img_height):
        for y in range(0, img_width):
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
            now = time.time()
            # Ring fetch taking ~1.6 seconds.
            intensity_ring = fetch_bresenham_circle(bw_img, x, y, img_height, img_width, ip)
            time_acc += (time.time() - now)
            
            # Is keypoint taking ~0.925 seconds
            if is_keypoint(ip, intensity_ring, threshold):
                keypoints.append([x, y])
    print(time_acc)
    return keypoints

def in_threshold(principal_intensity, test_intensity, threshold: int):
    return (test_intensity > (principal_intensity - threshold)) and (test_intensity < (principal_intensity + threshold))

def in_threshold_percentage(principal_intensity, test_intensity, threshold: float):
    thresh = principal_intensity * threshold
    return (test_intensity > (principal_intensity - thresh)) and (test_intensity < (principal_intensity + thresh))

def fetch_bresenham_circle(bw_img: Mat, x, y, img_height, img_width, default_value):
    intensity_ring = []
    for u_off, v_off in BRESENHAM_CIRCLE_3:
        u = x + u_off
        v = y + v_off

        # out of bounds checks. Set to ip, (inside threshold so discounted)
        if u < 0 or u >= img_height or v < 0 or v >= img_width:
            intensity_ring.append(default_value)
        else:
            intensity_ring.append(bw_img[u, v])
    return intensity_ring
    

def is_keypoint(point_intensity: int, intensity_ring: list[int], threshold: int) -> bool:
    # Quick test to see if it's possible.
    quick_test_outside_thresh = 0
    for circle_idx in [0, 4, 8, 12]:    # TODO make dynamic if making circle dynamic
        if in_threshold(point_intensity, intensity_ring[circle_idx], threshold):
            continue
        quick_test_outside_thresh += 1

    if quick_test_outside_thresh < 3:
        # point rejected, not a corner
        return False

    is_beginning_consec = True
    num_beginning_consec = 0
    num_consec = 0
    for intensity in intensity_ring:
        if in_threshold(point_intensity, intensity, threshold):
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
