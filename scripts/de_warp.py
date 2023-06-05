from argparse import ArgumentParser
from cv2 import imread, Mat, imwrite
from math import sqrt, cos, sin
import numpy as np
# from pathlib import Path

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

def cubic_polynomial(a, b, c, d):
    """
    Solving:
    a * x^3 + b * x^2 + c * x + d
    """
    # TODO calculate this....
    # Replacing with np.roots
    pass


def de_warp_3(image: Mat):
    """
    https://imagemagick.org/Usage/lens/correcting_lens_distortions.pdf

    Real world coords, X, Y
    Image coords, x,y

    x_hat, y_hat denote image coordinates of the idea perspective mapping.

    We need relationship between physical coordinates of pixel, x,y
        and coordinates x_hat, y_hat of ideal perspective


    Now, let x_hat, y_hat to refer to principal point and x,y to refer to physical centre of image.
    Offset of principal point being x_hat_p, y_hat_p.

    Split lens distortion into radial and tangential part
    
    r_hat^2 = x_hat^2 + y_hat^2
    r^2 = (x - x_hat_p) ^ 2 + (y - y_hat_p) ^ 2
    r_hat = f(r) * r
    r_hat * alpha = g(r, alpha) * r * alpha

    barrel distortion, f(r) < 1
    pincussion distortion f(r) > 1

    # This is tunable with `an` and different functions are possible
    f(r) = 1 + a1 * r + a2 * r ^ 2 + a3 * r ^ 3

    incorporating tangential distortion
    (x_hat, y_hat) = (f) * (x, y) + (2 * a4 * x * y + a5 * (r^2 + 2 * x^2), a4 * (r^2 + 2y^2) + 2 * a5 * x * y)


    """
    pass

def f(r, a):
    return 1 + a[0] * r + a[1] * (r ** 2) + a[2] * (r ** 3)


def de_warp_4(imager: Mat):
    # https://iopscience.iop.org/article/10.1088/1742-6596/1453/1/012136/pdf
    """
    Only vers radial distortion. tangential distortion "can be ignored for most computer vision applications"
    """
    pass

def de_warp_5(image: Mat):
    # https://www.ncbi.nlm.nih.gov/pmc/articles/PMC8624224/
    """
    let (x0, y0) be the position where light is imaged on the camera sensor when there is no distortion
    let (x, y) be the actual position after the distortion occurs.

    x0 = x(1 + k1 * r^2 + k2 r^4 + k3 * r^6)
    y0 = y(1 + k1 * r^2 + k2 r^4 + k3 * r^6)

    
    """
    # Radial distrotion coefficients.
    # TODO pass as params
    kd = [0, 0, 0, 0, 0]

    original_height, original_width, channel = image.shape


# https://www.mathworks.com/help/symbolic/developing-an-algorithm-for-undistorting-an-image.html
# https://www.mathworks.com/help/vision/ug/visual-simultaneous-localization-and-mapping-slam-overview.html

def de_warp_2(image: Mat):
    """
    https://www.researchgate.net/publication/269271727_A_fisheye_distortion_correction_algorithm_optimized_for_hardware_implementations

    uv plane is selected ROI
    xy plane represents the image sensor
    p is arbitrary point on uv plane
    
    p(uv) = (pu, pv)
    p(xyz) = (px, py, pz)

    Transforming unit vectors in the uv plane, then using the linear relationship
    p(xyz) = p'(xyz) + (p(xyz) - p'(xyz))
           = p'(xyz) + pu u^ (xyz) + pv v^ (xyz)
    u^ and v^ are unit vectors in the uv plane.
    p' is a vector pointing at the center of the ROI with length equal to mR
    
    Unit vectors
    Can be calculated through sequence of 2D vector rotations

    Let u^ (uv) = (1, 0) be one unit vector.
    (k)angle_alpha denote a rotation of a 2D vector k by angle alpha
    It's representation in xyz frame is 
    a = (u^ (uv))angle_psi
    b = (a1, 0)angle_negative_beta
    c = (b1, a2)angle_gamma
    u^ (xyz) = (c1, c2, b2)
    where a,b,c are vectors resulting from intermediate computations.
    
    Repeat procedure to get v^

    Next step is rectangular to polar coordinates

    gammap = arctan(py / px)
    betap = arctan(pxy / pz)

    pxy = sqrt((px)^2 + (py)^2)
    betap is sensor incidence angle
    If orthogonal fisheye lens was used, (px,py) givese exact location of pixel in the image acquired by fisheye.

    In general case, lens function needs to be included.
    So, map with lens function
    (pfx, pfy) given by

    rp = R * Flens(Betap)
    pfx = rp * cos(gammap)
    pfy = rp * sin(gammap)
    """
    pass


def main():
    args = setup_and_parse_args()
    image = imread(args.input_file)
    outfile_path = './data/dewarp_test/dewarped.jpg'    # TODO make this `{infile}_dewarped.jpg`
    de_warped = de_warp(image)
    imwrite('./data/dewarp_test/outfile.jpg', de_warped)


if __name__ == '__main__':
    main()