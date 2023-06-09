from cv2 import Mat
from math import sqrt, cos, sin
import numpy as np
from typing import Tuple


def generate_distortion_mat(image_dim: Tuple[int, int], distortion_coefficients: list):
    """
    Params:
    image_dim: (int_img_height, int_img_width)
    distortion_coefficients: [int]*5
    """
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
    height = image_dim[0]
    width = image_dim[1]


    # Let x direction be along width, and y direction be along height
    # (x0, y0) is image center
    y0 = width / 2
    x0 = height / 2
    
    # The x range is from [-1/2width, 1/2width]
    # Whereas the u,v is is [0, width]
    # u related to x, y related to v

    # (x,y) are real image coordiantes
    # (xd,yd) are distorted image coordinates
    # Radial distortion is along radial direction from center of image
    # r is ideal and rd is distorted radial distance from image center to any pixel coordinate

    # Maps new image original image[x, y] coordinates to new image coords.
    # So, the on the final image, at pixel (u, v), we can look at this map
    # of (u, v) and get the original pixel location from it, (x, y) = mat[u, v]
    distortion_mat = np.empty((height, width, 2), np.uint16)

    for u in range(0, height):
        for v in range(0, width):
            # 2.2
            x = int(u - x0)
            y = int(v - y0)

            rd = sqrt(x ** 2 + y ** 2)

            # 2.13
            a = 1
            b = (rd * distortion_coefficients[3] - distortion_coefficients[0]) / (rd * distortion_coefficients[4] - distortion_coefficients[1])
            c = (rd * distortion_coefficients[2] - 1) / (rd * distortion_coefficients[4] - distortion_coefficients[1])
            d = rd / (rd * distortion_coefficients[4] - distortion_coefficients[1])

            # Solving 2.12
            # TODO determine which roots to pick
            roots = np.sort(np.roots([a, b, c, d]))

            reals = np.sort(roots.real[abs(roots.imag) < 1e-5])

            r = reals[0]
            if len(reals) == 3:
                r = reals[1]

            # 2.15
            theta = np.arctan2(x, y)
            # 2.14
            y = r * cos(theta)
            x = r * sin(theta)

            # TODO determine if cast to int is OK
            new_v = int(y + y0)
            new_u = int(x + x0)
            # print(new_u, new_v)
            # if new_u > 1080
            distortion_mat[u, v] = (new_u, new_v)
            # new_img[v, u] = image[new_v, new_u]

    return distortion_mat


def apply_distortion_mat(image: Mat, distortion_mat: Mat):
    
    # Radial distrotion coefficients.
    height, width, _ = image.shape
    new_image = np.empty_like(image)
    for u in range(0, height):
        for v in range(0, width):
            old_u, old_v = distortion_mat[u, v]
            new_image[u, v] = image[old_u, old_v]
    return new_image
        
