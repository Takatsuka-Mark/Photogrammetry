import numpy as np
from cv2 import FONT_HERSHEY_SIMPLEX, circle, imread, imwrite, line, putText

from photogrammetry.image_processing.keypoint_detection import \
    FASTKeypointDetector
from photogrammetry.image_processing.keypoint_matching import match_keypoints
from photogrammetry.clustering.hierarchical import HierarchicalClustering, ChunkedHierarchicalClusteringMultithreaded
from photogrammetry.storage.image_db import ImageDB
from photogrammetry.storage.keypoint_cache import KeypointCache, KeypointCacheInfo
from photogrammetry.utils.draw import draw_point
from argparse import ArgumentParser


def setup_and_parse_args():
    parser = ArgumentParser(
        prog='detect_features',
        description='Detects keypoints'
    )
    parser.add_argument('input_file_1')
    parser.add_argument('input_file_2')
    parser.add_argument('--detection-threshold', default=50, type=int, required=False)
    parser.add_argument('--max-merge-dist', default=25, type=int, required=False)
    parser.add_argument('--match-threshold', default=75, type=int, required=False)
    return parser.parse_args()

def draw_keypoints(img, keypoints, color):
    for keypoint in keypoints:
        circle(img, keypoint.coord[::-1], 5, color, -1)

def run_fast_detection(fast_detector: FASTKeypointDetector, image_id: int):
    keypoints = fast_detector.detect_points(image_id)
    return keypoints

def chunked_cluster_fast_detection(keypoints, image_dim, max_merge_dist: int):
    chunked_hc = ChunkedHierarchicalClusteringMultithreaded(
        image_dim, keypoints, max_merge_dist=max_merge_dist
    )
    chunked_keypoints = chunked_hc.run_clustering()
    return chunked_keypoints

def cluster_keypoints(image, input_filename, detection_threshold, image_db, cache, image_dim, max_merge_dist, fast_detector):
    keypoint_cache_info = KeypointCacheInfo(
        img_hash=input_filename,
        is_clustered=False,
        fast_detection_threshold=detection_threshold
    )
    image_id = image_db.add_image(image)

    keypoints = cache.get_keypoints_if_exist(keypoint_cache_info)
    if not keypoints:
        keypoints = run_fast_detection(fast_detector, image_id)
        cache.store_keypoints_if_not_exist(keypoints, keypoint_cache_info)
    # TODO not finding independent dicts for some reason... Maybe cache needs a reload.
    # clustered_keypoint_cache_info = KeypointCacheInfo(
    #     img_hash=input_filename,
    #     is_clustered=True,
    #     fast_detection_threshold=detection_threshold
    # )

    # clustered_keypoints = cache.get_keypoints_if_exist(clustered_keypoint_cache_info)
    # if not clustered_keypoints:
    # return keypoints
    clustered_keypoints = chunked_cluster_fast_detection(keypoints, image_dim, max_merge_dist)
        # cache.store_keypoints_if_not_exist(keypoints, clustered_keypoint_cache_info)
    return clustered_keypoints
    # return keypoints



def get_keypoint_2_coord(keypoint, img_width):
    return [i + j for i, j in zip(keypoint.coord, [0, img_width])]

def draw_keypoint(img, keypoint, is_second:bool, img_width):
    circle(img, get_keypoint_2_coord(keypoint, img_width)[::-1] if is_second else keypoint.coord[::-1], 20, (0, 0, 255) if is_second else (0, 255, 0), -1)

def draw_keypoint_line(img, keypoint1, keypoint2, color, img_width):
    line(img, keypoint1.coord[::-1], get_keypoint_2_coord(keypoint2, img_width)[::-1], color, 10)

def label_keypoint(img, keypoint, keypoint_idx, is_second: bool, img_width):
    putText(img, str(keypoint_idx), get_keypoint_2_coord(keypoint, img_width)[::-1] if is_second else keypoint.coord[::-1], FONT_HERSHEY_SIMPLEX, 4, color=(0, 255, 255), thickness=10)


def main():
    cache = KeypointCache()
    args = setup_and_parse_args()
    input_filename_1 = args.input_file_1
    input_filename_2 = args.input_file_2

    image_1 = imread(input_filename_1)
    image_2 = imread(input_filename_2)

    # TODO should verify that image1 and 2 are the same shape
    height, width, _ = image_1.shape
    img_dim = (height, width)

    image_db = ImageDB(height, width)

    fast_detector = FASTKeypointDetector(args.detection_threshold, image_db)
    img_1_keypoints = cluster_keypoints(
        image_1, input_filename_1,
        args.detection_threshold, image_db, cache, img_dim, args.max_merge_dist,
        fast_detector
    )
    img_2_keypoints = cluster_keypoints(
        image_2, input_filename_2,
        args.detection_threshold, image_db, cache, img_dim, args.max_merge_dist,
        fast_detector
    )

    # print(img_2_keypoints)

    key1_to_key2_dist = match_keypoints(img_1_keypoints, img_2_keypoints, -1)

    image_combined = np.hstack((image_1, image_2))

    # for key_idx in range(len(img_1_keypoints)):
    #     draw_keypoint(image_combined, img_1_keypoints[key_idx], False, width)
    # for key_idx in range(len(img_2_keypoints)):
    #     draw_keypoint(image_combined, img_2_keypoints[key_idx], True, width)

    for key1_idx in range(len(img_1_keypoints)):
        keypoint1 = img_1_keypoints[key1_idx]
        
        # print(key1_to_key2_dist[key1_idx])
        # return
        key2_idx, dist = key1_to_key2_dist[key1_idx, 0]
        if dist > args.match_threshold:
            continue
        # print(dist)
        keypoint2 = img_2_keypoints[key2_idx]
        
        draw_keypoint(image_combined, keypoint1, False, width)
        draw_keypoint(image_combined, keypoint2, True, width)
        draw_keypoint_line(image_combined, keypoint1, keypoint2, (255, 0, 0), width)

    imwrite('./data/feature_matching_test/matched_features_combined.jpg', image_combined)

if __name__ == '__main__':
    main()
