# TODO move out of image_processing

from photogrammetry.image_processing.keypoint_detection import KeyPoint
import math
import numpy as np

def match_keypoints(keypoints1: list[KeyPoint], keypoints2: list[KeyPoint], hamming_threshold: int):
    key1_to_key2_dist = np.zeros((len(keypoints1), len(keypoints2), 2), dtype=np.int64)
    for idx1 in range(len(keypoints1)):
        des1 = keypoints1[idx1].descriptor
        for idx2 in range(len(keypoints2)):
            dist = hamming_distance(des1, keypoints2[idx2].descriptor)
            key1_to_key2_dist[idx1, idx2] = [idx2, dist]

    # key1_to_min_dist = {}
    # # TODO very crude implementation. need some sort of fallback
    # for idx1 in range(len(keypoints1)):
    #     min_dist = math.inf
    #     min_dist_idx = -1
    #     for idx2 in range(len(keypoints2)):
    #         dist = key1_key2_to_dist[idx1, idx2]
    #         if dist < min_dist:
    #             min_dist = dist
    #             min_dist_idx = idx2
    #     key1_to_min_dist[idx1] = {min_dist_idx: min_dist}
    # print(key1_to_key2_dist[0])
    # key1_to_key2_dist = key1_to_key2_dist[key1_to_key2_dist[:, ]]
    for idx1 in range(len(keypoints1)):
        # Sort by the distances
        row = key1_to_key2_dist[idx1]
        key1_to_key2_dist[idx1] = row[row[:, 1].argsort()]  # sorted(key1_to_key2_dist[idx1], key=lambda x: x[1])
    # print(key1_to_key2_dist[0])
    return key1_to_key2_dist

    # return key1_to_min_dist


def hamming_distance(int1, int2) -> int:
    # NOTE in python 3.10, the builtin int.bitcount can be used.
    return bin(int1 ^ int2).count("1")

# import numpy as np
# arr = np.array(
# [[ 0, 52],
#  [ 1, 44],
#  [ 2, 40],
#  [ 3, 51],
#  [ 4, 64],
#  [ 5, 47],
#  [ 6, 65],
#  [ 7, 79],
#  [ 8, 73],
#  [ 9, 41],
#  [10, 60],
#  [11, 76],
#  [12, 49],
#  [13, 70],
#  [14, 70],
#  [15, 50],
#  [16, 70],
#  [17, 40],
#  [18, 74],
#  [19, 52],
#  [20, 49],
#  [21, 42],
#  [22, 43]]
# )
# print(arr[arr[:,1].argsort()])
# print(arr)