from argparse import ArgumentParser
from cv2 import circle, imwrite, imread
from photogrammetry.storage.image_db import ImageDB
from photogrammetry.image_processing.keypoint_detection import FASTKeypointDetector
from photogrammetry.clustering.hierarchical import HierarchicalClustering, ChunkedHierarchicalClustering, ChunkedHierarchicalClusteringMultithreaded
from time import time
from photogrammetry.storage.keypoint_cache import KeypointCache, KeypointCacheInfo

def setup_and_parse_args():
    parser = ArgumentParser(
        prog='cluster_features',
        description='tests detecting and clustering features'
    )
    parser.add_argument('input_file')
    parser.add_argument('--detection-threshold', default=50, type=int, required=False)
    parser.add_argument('--max-merge-dist', default=25, type=int, required=False)
    return parser.parse_args()

def draw_keypoints(img, keypoints, color):
    for keypoint in keypoints:
        circle(img, keypoint.coord[::-1], 5, color, -1)

def run_fast_detection(image_db: ImageDB, image_id: int, detection_threshold: int):
    fast_detector = FASTKeypointDetector(detection_threshold, image_db)
    keypoints = fast_detector.detect_points(image_id)
    return keypoints

def cluster_fast_detection(keypoints, max_merge_dist: int):
    hc = HierarchicalClustering(keypoints, max_merge_distance=max_merge_dist)
    clustered_keypoints = hc.run_clustering()
    return clustered_keypoints

def chunked_cluster_fast_detection(keypoints, image_dim, max_merge_dist: int):
    chunked_hc = ChunkedHierarchicalClusteringMultithreaded(
        image_dim, keypoints, max_merge_dist=max_merge_dist
    )
    chunked_keypoints = chunked_hc.run_clustering()
    return chunked_keypoints

def main():
    cache = KeypointCache()
    args = setup_and_parse_args()
    input_filename = args.input_file
    image = imread(input_filename)
    raw_keypoint_cache_info = KeypointCacheInfo(
        img_hash=args.input_file,   # TODO this is a hack...    Instead, the image DB shoudl return a UUID or index. Then, we can use that as the hash of the image.
        is_clustered=False,
        fast_detection_threshold=args.detection_threshold
    )
    height, width, _ = image.shape
    image_db = ImageDB(height, width)
    image_id = image_db.add_image(image)
    
    keypoints = cache.get_keypoints_if_exist(raw_keypoint_cache_info)
    if keypoints is None:
        keypoints = run_fast_detection(image_db, image_id, args.detection_threshold)
        cache.store_keypoints_if_not_exist(keypoints, raw_keypoint_cache_info)
    print(f"Found {len(keypoints)} keypoints")

    start=time()
    # Taking ~11 seconds for 2175 points.
    # Unplugged, ~12.2s, 2175 -> 279 keypoints.
    # clustered_keypoints = cluster_fast_detection(keypoints, args.max_merge_dist)

    # Unplugged, 4x4, ~3.71s, 2175 -> 280
    clustered_keypoints = chunked_cluster_fast_detection(keypoints, (height, width), args.max_merge_dist)
    print(f"Clustered to {len(clustered_keypoints)} keypoints in {time() - start}")
    draw_keypoints(image, keypoints, (0, 0, 255))
    draw_keypoints(image, clustered_keypoints, (0, 255, 0))
    print("Drawing base keypoints in RED and clustered in GREEN")
    imwrite(f"{input_filename[:-4]}_clustered_keypoints.jpg", image)

if __name__ == '__main__':
    main()
