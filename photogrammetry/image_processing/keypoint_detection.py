from cv2 import Mat, cvtColor, COLOR_BGR2GRAY
import numpy as np
import time
import multiprocessing

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
MINI_BRESENHAM_CIRCLE_3 = BRESENHAM_CIRCLE_3[[0, 4, 8, 12]]
# Going from [[x, y], [x2, y2], ...] to [[x, x2, ...], [y, y2, ...]]
BRESENHAM_CIRCLE_3_TP = BRESENHAM_CIRCLE_3.transpose((1, 0))
MINI_BRESENHAM_CIRCLE_3_TP = MINI_BRESENHAM_CIRCLE_3.transpose((1, 0))

class FASTKeypointDetector:
    def __init__(self, threshold, img_height, img_width) -> None:
        self.threshold = threshold
        self.img_height = img_height
        self.img_width = img_width

        # Placeholder values. Could use properties to set with proper placeholder values.
        self._bw_img = np.empty(0)
        self._bounds = np.empty(0)

        self._time_acc = 0

    def _config_caches(self, image: Mat):
        self._bw_img = cvtColor(image, COLOR_BGR2GRAY).astype(np.int16)
        # Convert into array like [[[lower, upper], [lower, upper], ...],[],...]
        # Packing bounds like this seems to have reduced from 5.7 seconds to 5.45
        # Caching bounds in general also saves reduces from 6 to 2.7 seconds
        self._bounds = np.stack([self._bw_img - self.threshold, self._bw_img + self.threshold], axis=2)

    def _in_threshold(self, ip_bounds, test_intensity):
        # TODO this could just take in x, y. Not lower, upper bound
        return (test_intensity > ip_bounds[0]) and (test_intensity < ip_bounds[1])

    def _fetch_bresenham_circle(self, u, v):
        # Apparently unpacking an array is expensive.
        # So, changing this to `ring_xs, ring_ys = BRES_.. + np.array(...)` is about 7% slower
        # ring_points = BRESENHAM_CIRCLE_3_TP + np.array([[x], [y]])
        # return self._bounds[ring_points[0], ring_points[1]]
        return self._bw_img[BRESENHAM_CIRCLE_3_TP[0] + u, BRESENHAM_CIRCLE_3_TP[1] + v]

    def _is_keypoint_quick(self, ip_bounds, u, v) -> bool:
        # TODO this should likely be combined with _is_keypoint. But, only required once the 4 caluclated quick points aren't thrown out
        quick_num_inside_thresh = 0
        for idx in range(4):
            # I would like to split the bound fetching into a separate method, but it's too slow.

            # TODO HUGE POTENTIAL SAVINGS



            # TODO perhaps we could speed this line up by pre-computing all MINI BRES x and y values. Here, we will repeat the same operations on x for every y where x is the same and visa versa. This is super inefficient.
            # Get the lower and upper bounds at the `idx` value in the mini bresenham circle, offset by x or y.
            if not self._in_threshold(ip_bounds, self._bw_img[MINI_BRESENHAM_CIRCLE_3_TP[0, idx] + u, MINI_BRESENHAM_CIRCLE_3_TP[1, idx] + v]):
                continue
            
            if quick_num_inside_thresh > 0:
                # There has already been one failure and we just found another. Reject the point as it can't be a corner
                return False
            quick_num_inside_thresh += 1
        return True

    def _is_keypoint(self, ip_bounds, bres_intensities) -> bool:
        is_beginning_consec = True
        num_beginning_consec = 0
        num_consec = 0
        num_fail = 0
        for idx in range(len(bres_intensities)):
            if self._in_threshold(ip_bounds, bres_intensities[idx]):
                # We've broken the streak
                is_beginning_consec = False
                num_consec = 0
                num_fail += 1
                if num_fail > 4:
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

    def _process_row(self, u):
        """
        TODO EFFEICIENCY IMPROVEMENT <<<
        TODO, once fetching the mini bresenham is fast enough - Instead, iterate fetching mini bres' 4x
        Then, reconstruct the map from those results. If each ring passes the test, continue to next.
        If all pass, then we have to check for consecutives. But, we'll already have the proper info.
        """
        keypoints = []
        for v in range(3, self.img_width-3):
            # intensity at p
            ip_bounds = self._bounds[u, v]
            # Ring fetch taking ~4.2 seconds.
            # bounds = self._fetch_bresenham_circle(x, y)
            now = time.time()
            is_potential = self._is_keypoint_quick(ip_bounds, u, v)    # ~2.84
            self._time_acc += time.time() - now
            if is_potential:
                bres_circle = self._fetch_bresenham_circle(u, v)
                if self._is_keypoint(ip_bounds, bres_circle):
                    keypoints.append([u, v])

            # Is keypoint taking ~1.18 seconds
        return keypoints

    def _process_rows(self, xs):
        keypoints = []
        for x in xs:
            keypoints.extend(self._process_row(x))
        return keypoints

    def detect_points(self, image: Mat):
        # TODO we are excluding the 3 pixel border because it requires extra thought. Determine if this is OK
        # contents like [(height, width), (height2, width2)]
        keypoints = []
        now = time.time()

        self._config_caches(image)

        # Multiprocessing method. 1920x1080 ~ 1.81 seconds, 33886 keypoints. ~1.5 after refactor? ~0.67 after first Bres re-work + chunk size modification.
        # 15pt star ~ 0.152 seconds, 128 keypoints
        # pool = multiprocessing.Pool()
        # num_rows_per_process = 50
        # outputs = pool.map(self._process_row, range(3, self.img_height-3), chunksize=num_rows_per_process)
        # for output in outputs:
        #     keypoints.extend(output)
        
        # Regular method. 1920x1080 ~ 15.9 seconds, 33886 keypoints. ~6 after threshold refactor. ~2.54 after first Bresenham re-work
        # 15pt star ~ 1.158 seconds, 128 keypoints
        for u in range(3, self.img_height-3):
            keypoints.extend(self._process_row(u))
        print(time.time() - now)
        print("time_acc clocked", self._time_acc, "seconds")
        for keypoint in keypoints:
            print(keypoint, self._direction(keypoint[0], keypoint[1]))
        return keypoints

    def _moment(self, u, v, p, q):
        # Let u, v be the height and width.
        # https://iopscience.iop.org/article/10.1088/1742-6596/1693/1/012068/pdf
        acc = 0
        for x, y in BRESENHAM_CIRCLE_3:
            acc += (x ** p) * (y ** q) * self._bw_img[x + u, y + v]     # TODO determine if x, y or y, x. Not sure if it matters
        return acc
    
    def _direction(self, u, v):
        return np.arctan2(self._moment(u, v, 0, 1), self._moment(u, v, 1, 0))

    def _brief_descriptor(self, u, v):
        des = 0
        pass