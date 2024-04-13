from argparse import ArgumentParser
from cv2 import imread, Mat, imwrite, circle
from photogrammetry.image_processing.keypoint_detection import FASTKeypointDetector
from photogrammetry.storage.image_db import ImageDB
import time

def setup_and_parse_args():
    parser = ArgumentParser(
        prog='detect_features',
        description='Detects keypoints'
    )
    parser.add_argument('input_file')
    return parser.parse_args()

def draw_keypoint(img, keypoint):
    circle(img, keypoint.coord[::-1], 5, (0, 255, 0), -1)

def run_fast_detection(image_db: ImageDB, image_id: int):
    # keypoints = fast_detection(image)
    fast_detector = FASTKeypointDetector(50, image_db)
    keypoints = fast_detector.detect_points(image_id)
    return keypoints

def draw_keypoints(keypoints, image_db, image_id, filename):
    image = image_db.get_image(image_id)

    for keypoint in keypoints:
        draw_keypoint(image, keypoint)
    # TODO pass as arg
    imwrite(f"{filename[:-4]}_fast_detected.jpg", image)
    return keypoints

def main():
    args = setup_and_parse_args()
    input_filename = args.input_file
    image = imread(input_filename)
    height, width, _ = image.shape
    image_db = ImageDB(height, width)
    image_id = image_db.add_image(image)
    start = time.time()
    keypoints = run_fast_detection(image_db, image_id)
    print(len(keypoints), "keypoints found")
    print(f"Fast detection took {time.time() - start}")
    draw_keypoints(keypoints, image_db, image_id, input_filename)
    

if __name__ == '__main__':
    main()
