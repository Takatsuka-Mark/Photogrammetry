from argparse import ArgumentParser
from cv2 import imread, Mat, imwrite
from math import sqrt, cos, sin
import numpy as np

def setup_and_parse_args():
    parser = ArgumentParser(
        prog='DeWarp',
        description='Removes fisheye from photos',
    )
    parser.add_argument('input_file')
    return parser.parse_args()

# def read_image(image_file: str):

def de_warp(image: Mat):
    """
    https://etd.ohiolink.edu/apexprod/rws_etd/send_file/send?accession=dayton1323312991&disposition=inline
    (x, y) undistorted image coordinates
    (xd, yd) distorted image coordinates
    r, undistorted radial distance from image center to pixel coordinate
    rd, distorted r

    u = x + x0
    v = y + y0

    Affine transformation:
    u=x-x0, v=y-y0
        Where (x0, y0) are coordinates of image center and (u, v) are known
    
    rd = r * f(r, k(bar)d)

    xd = x * f(r, k(bar)d)
    yd = y * f(r, k(bar)d)

    f(r, k(bar)d) = (1 + k1d * r + k2d * r^2) / (1 + k3d * r + k4d * r^2 + k5d * r^3)

    r = sqrt(x^2 + y^2)
    kbard = {k1d, k2d, ..., k5d} - coefficients of radial distortion

    Invert to find the radius, so we get
    Ar^3 + Br^2 + Cr + D = 0
    where:
    A=1
    B=(rd * k4d - k1d) / (rd * k5d - k2d)
    C=(rd * k3d - 1) / (rd * k5d - k2d)
    D = rd / (rd * k5d - k2d)

    This gives us the value of r (solving the cubic polynomial).
    Then, the polar coordinates (r, theta) are transformed into image coords
    x = r * cos (theta), y = r * sin(theta)
    theta = arctan (y / x)

    Finally, u = x + x0, v = y + y(0?)

    When radial distrotion is small, k1d = k1c, k2d = k2c, and k3c = k4c = k5c ~= 0.
    So, just focus on k1c and k2c

    https://www.mathworks.com/help/symbolic/developing-an-algorithm-for-undistorting-an-image.html

    """
    # Radial distrotion coefficients.
    # TODO pass as params
    # kd = [0, 0, 0, 0, 0]
    kd = [3e-4, 1e-7, 0, 0, 0]

    original_height, original_width, channel = image.shape

    # Let x direction be along width, and y direction be along height
    # (x0, y0) is image center
    x0 = original_width / 2
    y0 = original_height / 2
    
    # The x range is from [-1/2width, 1/2width]
    # Whereas the u,v is is [0, width]
    # u related to x, y related to v

    # (x,y) are real image coordiantes
    # (xd,yd) are distorted image coordinates
    # Radial distortion is along radial direction from center of image
    # r is ideal and rd is distorted radial distance from image center to any pixel coordinate

    # new_img = np.zeros((original_height, original_width, 3), np.uint8)
    new_img = np.empty_like(image)

    # TODO determine if [0, width) or [1, width]
    for u in range(0, original_width):
        for v in range(0, original_height):
            # 2.2
            x = int(u - x0)
            y = int(v - y0)

            rd = sqrt(x ** 2 + y ** 2)

            # 2.13
            a = 1
            b = (rd * kd[3] - kd[0]) / (rd * kd[4] - kd[1])
            c = (rd * kd[2] - 1) / (rd * kd[4] - kd[1])
            d = rd / (rd * kd[4] - kd[1])

            # Solving 2.12
            roots = np.sort(np.roots([a, b, c, d]))

            reals = np.sort(roots.real[abs(roots.imag) < 1e-5])

            r = reals[0]
            if len(reals) == 3:
                r = reals[1]
            


            # 2.15
            theta = np.arctan2(y, x)
            # 2.14
            x = r * cos(theta)
            y = r * sin(theta)

            # TODO determine if cast to int is OK
            new_u = int(x + x0)
            new_v = int(y + y0)
            # print(new_u, new_v)
            # if new_u > 1080
            new_img[v, u] = image[new_v, new_u]
    return new_img

def main():
    args = setup_and_parse_args()
    image = imread(args.input_file)
    outfile_path = './data/dewarp_test/dewarped.jpg'    # TODO make this `{infile}_dewarped.jpg`
    de_warped = de_warp(image)
    imwrite('./data/dewarp_test/outfile.jpg', de_warped)


if __name__ == '__main__':
    main()