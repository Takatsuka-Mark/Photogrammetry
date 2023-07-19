from cv2 import Mat, cvtColor, COLOR_BGR2GRAY, imwrite
import numpy as np
import time
import multiprocessing
from functools import partial

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

class FASTKeypointDetector:
    def __init__(self, threshold, img_height, img_width) -> None:
        self.threshold = threshold
        self.img_height = img_height
        self.img_width = img_width

        # Placeholder values. Could use properties to set with proper placeholder values.
        self._bw_img = np.empty(0)
        self._lower_bounds = np.empty(0)
        self._upper_bounds = np.empty(0)

    def _config_caches(self, image: Mat):
        self._bw_img = cvtColor(image, COLOR_BGR2GRAY).astype(np.int16)
        self._lower_bounds = self._bw_img - self.threshold
        self._upper_bounds = self._bw_img + self.threshold

    def _in_threshold(self, principal_intensity: int, lower_bound: int, upper_bound: int):
        # TODO this could just take in x, y. Not lower, upper bound
        return (principal_intensity > lower_bound) and (principal_intensity < upper_bound)

    def _fetch_bresenham_circle(self, x, y):
        ring_points = BRESENHAM_CIRCLE_3_TP + np.array([[x], [y]])
        return self._lower_bounds[ring_points[0], ring_points[1]], self._upper_bounds[ring_points[0], ring_points[1]]

    def _is_keypoint(self, principal_intensity: int, ring_lower_bounds, ring_upper_bounds) -> bool:
        # Quick test to see if it's possible.

        quick_test_outside_thresh = 0
        for circle_idx in [0, 4, 8, 12]:    # TODO make dynamic if making circle dynamic
            if self._in_threshold(principal_intensity, ring_lower_bounds[circle_idx], ring_upper_bounds[circle_idx]):
                continue
            quick_test_outside_thresh += 1

        if quick_test_outside_thresh < 3:
            # point rejected, not a corner
            return False

        is_beginning_consec = True
        num_beginning_consec = 0
        num_consec = 0
        num_fail = 0
        for idx in range(len(ring_lower_bounds)):
            if self._in_threshold(principal_intensity, ring_lower_bounds[idx], ring_upper_bounds[idx]):
                # We've broken the streak
                is_beginning_consec = False
                num_consec = 0
                num_fail += 1
                if num_fail > 4:    # TODO This actualy didn't impact performance???
                    return False
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

    def _process_row(self, x):
        keypoints = []
        for y in range(3, self.img_width-3):
            # intensity at p
            ip = self._bw_img[x, y]
            # Ring fetch taking ~0.285 seconds.
            bres_lower_bounds, bres_upper_bounds = self._fetch_bresenham_circle(x, y)

            # Is keypoint taking ~0.925 seconds
            if self._is_keypoint(ip, bres_lower_bounds, bres_upper_bounds):
                keypoints.append([x, y])
        return keypoints

    def detect_points(self, image: Mat):
        # TODO we are excluding the 3 pixel border because it requires extra thought. Determine if this is OK
        # contents like [(height, width), (height2, width2)]
        keypoints = []
        now = time.time()

        self._config_caches(image)

        # Multiprocessing method. 1920x1080 ~ 1.81 seconds, 33886 keypoints. ~1.5 after refactor?
        # 15pt star ~ 0.152 seconds, 128 keypoints
        # pool = multiprocessing.Pool()
        # outputs = pool.map(self._process_row, range(3, self.img_height-3))
        # for output in outputs:
        #     keypoints.extend(output)
        
        # Regular method. 1920x1080 ~ 15.9 seconds, 33886 keypoints. ~6 after threshold refactor.
        # 15pt star ~ 1.158 seconds, 128 keypoints
        for x in range(3, self.img_height-3):
            keypoints.extend(self._process_row(x))
        print(time.time() - now)
        return keypoints
