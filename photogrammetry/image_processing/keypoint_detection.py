from cv2 import Mat, cvtColor, COLOR_BGR2GRAY, imwrite
import numpy as np

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
            
            circle_intensity = []
            for u_off, v_off in BRESENHAM_CIRCLE_3:
                u = x + u_off
                v = y + v_off

                # out of bounds checks. Set to ip, (inside threshold so discounted)
                if u < 0 or u >= img_height or v < 0 or v >= img_width:
                    circle_intensity.append(ip)
                else:
                    circle_intensity.append(bw_img[u, v])
            
            if is_keypoint(ip, circle_intensity, threshold):
                keypoints.append([x, y])
    return keypoints

def in_threshold(principal_intensity, test_intensity, threshold: int):
    return (test_intensity > (principal_intensity - threshold)) and (test_intensity < (principal_intensity + threshold))

def in_threshold_percentage(principal_intensity, test_intensity, threshold: float):
    thresh = principal_intensity * threshold
    return (test_intensity > (principal_intensity - thresh)) and (test_intensity < (principal_intensity + thresh))

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

    # TODO this is a super naive way to solve this. Just used for testing.
    is_on_run = False
    tests = 0
    consecutive_outside_thresh = 0
    total_in_thresh = 0
    idx = -1
    while tests < 32:
        idx += 1
        if idx == 16:
            idx = 0
        if tests >= 16 and not is_on_run:
            # We've seen everything, aren't on a run so we must be done.
            break
        tests += 1
        intensity = intensity_ring[idx]
        if in_threshold(point_intensity, intensity, threshold):
            consecutive_outside_thresh = 0
            is_on_run = False
            total_in_thresh += 1
            continue
        else:
            is_on_run = True
            consecutive_outside_thresh += 1
        if consecutive_outside_thresh >= 12:
            return True
    if consecutive_outside_thresh >= 12:
        return True
    return False
