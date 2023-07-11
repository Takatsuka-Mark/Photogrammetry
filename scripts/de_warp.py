from argparse import ArgumentParser
from cv2 import imread, imwrite
from photogrammetry.image_processing.warping import get_distortion_mat, apply_distortion_mat
from os import path
import time
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
    parser.add_argument(
        '--refresh-cache',
        action="store_true",
        default=False
    )
    return parser.parse_args()

# def read_image(image_file: str):

def save_stats(stats_file_path, stats):
    stats['timestamp'] = datetime.now().isoformat()
    stats['hostname'] = gethostname()   # Use to differentiate between PCs
    total_stats = []
    if path.exists(stats_file_path):
        with open(stats_file_path, 'r') as fp:
            total_stats = json.load(fp)
    total_stats.append(stats)

    with open(stats_file_path, 'w') as fp:
        json.dump(total_stats, fp)
    print("Run stats saved in:", stats_file_path)

def main():
    base_dir = './data/dewarp_test/'
    args = setup_and_parse_args()
    image = imread(args.input_file)
    outfile_path = path.join(base_dir, 'dewarped.jpg')    # TODO make this `{infile}_dewarped.jpg`
    stats_file_path = path.join(base_dir, 'stats.json')

    run_stats = {}
    run_stats['comments'] = args.stats_comments

    start_time = time.time()
    # TODO pass by args
    distortion_coefficients = [3e-4, 1e-7, 0, 0, 0]
    
    img_height, img_width, channels = image.shape
    run_stats['input_img'] = {
        'height': img_height,
        'width': img_width,
        'channels': channels,
        'filename': args.input_file
    }
    distortion_mat = get_distortion_mat((img_height, img_width), distortion_coefficients, refresh_cache=args.refresh_cache)
    run_stats['generate_distortion_mat_seconds'] = (time.time() - start_time)
    run_stats['generate_includes_caching'] = True
    start_time = time.time()
    de_warped = apply_distortion_mat(image, distortion_mat)
    run_stats['apply_distortion_mat_seconds'] = (time.time() - start_time)
    imwrite(outfile_path, de_warped)
    save_stats(stats_file_path, run_stats)

if __name__ == '__main__':
    main()