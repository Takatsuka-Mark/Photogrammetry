from argparse import ArgumentParser
from cv2 import imread, Mat, imwrite
from photogrammetry.image_processing.keypoint_detection import fast_detection, in_threshold

def setup_and_parse_args():
    parser = ArgumentParser(
        prog='detect_features',
        description='Detects keypoitns'
    )
    parser.add_argument('input_file')
    return parser.parse_args()

def run_fast_detection(image: Mat):
    keypoints = fast_detection(image)

    height, width, _ = image.shape

    for x, y in keypoints:
        # TODO need to clamp this inside of frame
        for u in range(x-2, x+3):
            for v in range(y-2, y+3):
                if u >= height or u < 0 or v >= width or v < 0:
                    continue
                image[u, v] = [0, 255, 0]
    # TODO pass as arg
    imwrite('./data/feature_detection_test/fast_detected.jpg', image)

def main():
    base_dir = './data/feature_detection_test'
    args = setup_and_parse_args()
    image = imread(args.input_file)

    run_fast_detection(image)

if __name__ == '__main__':
    main()
