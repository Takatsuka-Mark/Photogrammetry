from argparse import ArgumentParser
from cv2 import imread, imwrite
from photogrammetry.image_processing.warping import generate_distortion_mat, apply_distortion_mat
from os import path
from datetime import datetime
import json
from socket import gethostname


def setup_and_parse_args():
    parser = ArgumentParser(
        prog='DeWarp',
        description='Removes fisheye from photos',
    )
    parser.add_argument('input_file')
    parser.add_argument('stats_comments')
    return parser.parse_args()

# def read_image(image_file: str):

def save_stats(stats_file_path, stats):
    stats['timestamp'] = datetime.now().isoformat()
    stats['hostname'] = gethostname()   # Use to differentiate between PCs
    total_stats = []
    if path.exists(stats_file_path):
        total_stats = json.load(open(stats_file_path, 'r'))
    total_stats.append(stats)

    json.dump(total_stats, open(stats_file_path, 'w'))
    print("Run stats saved in:", stats_file_path)

def main():
    base_dir = './data/dewarp_test/'
    args = setup_and_parse_args()
    image = imread(args.input_file)
    outfile_path = path.join(base_dir, 'dewarped.jpg')    # TODO make this `{infile}_dewarped.jpg`
    stats_file_path = path.join(base_dir, 'stats.json')

    run_stats = {}
    run_stats['comments'] = args.stats_comments

    start_time = datetime.now()
    # TODO pass by args
    distortion_coefficients = [3e-4, 1e-7, 0, 0, 0]
    
    img_height, img_width, channels = image.shape
    run_stats['input_img'] = {
        'height': img_height,
        'width': img_width,
        'channels': channels,
        'filename': args.input_file
    }
    distortion_mat = generate_distortion_mat((img_height, img_width), distortion_coefficients)
    run_stats['generate_distortion_mat_micros'] = (datetime.now() - start_time).microseconds
    start_time = datetime.now()
    de_warped = apply_distortion_mat(image, distortion_mat)
    run_stats['apply_distortion_mat_micros'] = (datetime.now() - start_time).microseconds
    imwrite(outfile_path, de_warped)
    save_stats(stats_file_path, run_stats)

if __name__ == '__main__':
    main()