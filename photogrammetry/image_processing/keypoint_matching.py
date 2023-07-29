# TODO move out of image_processing

from photogrammetry.image_processing.keypoint_detection import KeyPoint
import math

def match_keypoints(keypoints1: list[KeyPoint], keypoints2: list[KeyPoint], hamming_threshold: int):
    key1_to_key2 = {}
    for idx1 in range(len(keypoints1)):
        des1 = keypoints1[idx1].descriptor
        key1_to_key2[idx1] = {}
        for idx2 in range(len(keypoints2)):
            dist = hamming_distance(des1, keypoints2[idx2])
            key1_to_key2[idx1][idx2] = dist

    key1_to_min_dist = {}
    # TODO very crude implementation. need some sort of fallback
    for idx1, keys2 in key1_to_key2.items():
        min_dist = math.inf
        min_dist_idx = -1
        for idx2, dist in keys2.items():
            if dist < min_dist:
                min_dist = dist
                min_dist_idx = idx2
        key1_to_min_dist[idx1] = {min_dist_idx: min_dist}

    return key1_to_min_dist
    



def hamming_distance(int1, int2) -> int:
    # NOTE in python 3.10, the builtin int.bitcount can be used.
    return bin(int1 ^ int2).count("1")