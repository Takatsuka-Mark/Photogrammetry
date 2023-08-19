from photogrammetry.image_processing.keypoint_detection import FASTKeypointDetector
from photogrammetry.image_processing.keypoint_matching import match_keypoints
from photogrammetry.storage.image_db import ImageDB
from photogrammetry.utils.draw import draw_point
from cv2 import imread, imwrite, circle, line, putText, FONT_HERSHEY_SIMPLEX
import pickle
import math
import numpy as np

image1 = imread('./data/feature_matching_test/15pt_star.png')
image2 = imread('./data/feature_matching_test/15pt_star_shifted_150.png')
# image1 = imread('./data/feature_matching_test/lego_space_1_from_left.jpg')
# image2 = imread('./data/feature_matching_test/lego_space_1_from_right.jpg')



img_height, img_width, _ = image1.shape
image_db = ImageDB(img_height, img_width)
image_1_id = image_db.add_image(image1)
image_2_id = image_db.add_image(image2)

fast_detector = FASTKeypointDetector(50, image_db)

# Saving keypoints
img1_keypoints = fast_detector.detect_points(image_1_id)
img2_keypoints = fast_detector.detect_points(image_2_id)
# with open('./data/feature_matching_test/lego_space_1_from_left_keypoints.dat', 'wb') as f:
#     pickle.dump(img1_keypoints, f)
# with open('./data/feature_matching_test/lego_space_1_from_right_keypoints.dat', 'wb') as f:
#     pickle.dump(img2_keypoints, f)

# Loading keypoints
# with open('./data/feature_matching_test/lego_space_1_from_left_keypoints.dat', 'rb') as f:
#     img1_keypoints = pickle.load(f)
# with open('./data/feature_matching_test/lego_space_1_from_right_keypoints.dat', 'rb') as f:
#     img2_keypoints = pickle.load(f)


# Drawing all keypoints on the images
# for keypoint in img1_keypoints:
#     draw_point(image1, keypoint.coord, img_height, img_width)

# imwrite('./data/feature_matching_test/lego_space_1_from_left_keypoints.jpg', image1)

# for keypoint in img2_keypoints:
#     draw_point(image2, keypoint.coord, img_height, img_width)

# imwrite('./data/feature_matching_test/lego_space_1_from_right_keypoints.jpg', image2)

key1_to_key2_dist = match_keypoints(img1_keypoints, img2_keypoints, -1)

def get_keypoint_2_coord(keypoint):
    return [i + j for i, j in zip(keypoint.coord, [0, img_width])]

def draw_keypoint(img, keypoint, is_second:bool):
    circle(img, get_keypoint_2_coord(keypoint)[::-1] if is_second else keypoint.coord[::-1], 20, (0, 0, 255) if is_second else (0, 255, 0), -1)

def draw_keypoint_line(img, keypoint1, keypoint2, color):
    line(img, keypoint1.coord[::-1], get_keypoint_2_coord(keypoint2)[::-1], color, 10)

def label_keypoint(img, keypoint, keypoint_idx, is_second: bool):
    putText(img, str(keypoint_idx), get_keypoint_2_coord(keypoint)[::-1] if is_second else keypoint.coord[::-1], FONT_HERSHEY_SIMPLEX, 4, color=(0, 255, 255), thickness=10)

image_combined = np.hstack((image1, image2))
for key1_idx in range(0, len(img1_keypoints), 3):
    keypoint1 = img1_keypoints[key1_idx]
    # print(keypoint1.descriptor)
    # Print 10 next best keypoints
    draw_keypoint(image_combined, keypoint1, is_second=False)
    key2_idx, dist = key1_to_key2_dist[key1_idx, 0]
    keypoint2 = img2_keypoints[key2_idx]
    draw_keypoint(image_combined, keypoint2, is_second=True)
    draw_keypoint_line(image_combined, keypoint1, keypoint2, (255, 0, 0))
    # label_keypoint(image_combined, keypoint1, key1_idx, is_second=False)
    # for idx in range(len(img2_keypoints)):
    #     key2_idx, dist = key1_to_key2_dist[key1_idx, idx]
    #     print(dist)
    # for idx in range(10):
    #     key2_idx, dist = key1_to_key2_dist[key1_idx, idx]
    #     keypoint2 = img2_keypoints[key2_idx]
    #     print(f"From: {key1_idx}, To: {key2_idx}, Dist: {dist}")
    #     draw_keypoint(image_combined, keypoint2, is_second=True)
    #     label_keypoint(image_combined, keypoint2, key2_idx, is_second=True)
    #     draw_keypoint_line(image_combined, keypoint1, keypoint2, (255, 0, 0))

# circle(image_combined, img1_keypoints[best_key1].coord[::-1], 20, (0, 255, 0), -1)
# circle(image_combined, [i + j for i, j in zip(img2_keypoints[best_key2].coord, [img_height, img_width])][::-1], 20, (0, 0, 255), -1)

imwrite('./data/feature_matching_test/15pt_star_combined.jpg', image_combined)
# imwrite('./data/feature_matching_test/lego_space_1_combined.jpg', image_combined)

# circle(image1, img1_keypoints[best_key1].coord[::-1], 20, (0, 255, 0), -1)
# circle(image2, img2_keypoints[best_key2].coord[::-1], 20, (0, 255, 0), -1)
# imwrite('./data/feature_matching_test/lego_space_1_from_left_matched.jpg', image1)
# imwrite('./data/feature_matching_test/lego_space_1_from_right_matched.jpg', image2)


