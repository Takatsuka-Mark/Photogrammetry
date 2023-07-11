from math import sqrt, cos, sin
from cv2 import Mat, remap, INTER_LINEAR
import numpy as np
from typing import Tuple
from photogrammetry.utils.files import create_dir_if_not_exists
from os import path
import time

def get_distortion_mat(image_dim: Tuple[int, int], distortion_coefficients: list, cache = True, refresh_cache = False):
    if not cache:
        return generate_distortion_mat(image_dim, distortion_coefficients)

    filename = distortion_mat_filename(image_dim, distortion_coefficients)
    cache_dir = './data/distortion_mats'
    create_dir_if_not_exists(cache_dir)
    
    file_path = path.join(cache_dir, filename)
    if not refresh_cache and path.exists(file_path) and path.isfile(file_path):
        return np.load(file_path)
    
    start = time.time()
    dist_mat = generate_distortion_mat(image_dim, distortion_coefficients)
    print(f"Took {time.time() - start} seconds to generate the distortion mat")
    np.save(file_path, dist_mat)
    return dist_mat

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
    
    # Maps rd^2 (expanded, (x^2 + y^2)) to radius, r
    rd2_cache = {}

    for u in range(0, height):
        for v in range(0, width):
            # 2.2
            x = int(u - x0)
            y = int(v - y0)

            rd2 = x ** 2 + y ** 2
            if rd2 not in rd2_cache:
                
                rd = sqrt(rd2)

                # 2.13
                a = 1
                b = (rd * distortion_coefficients[3] - distortion_coefficients[0]) / (rd * distortion_coefficients[4] - distortion_coefficients[1])
                c = (rd * distortion_coefficients[2] - 1) / (rd * distortion_coefficients[4] - distortion_coefficients[1])
                d = rd / (rd * distortion_coefficients[4] - distortion_coefficients[1])

                # Solving 2.12
                roots = np.sort(np.roots([a, b, c, d]))

                reals = np.sort(roots.real[abs(roots.imag) < 1e-5])

                # TODO determine which roots to pick
                if len(reals) == 3:
                    r = reals[1]
                else:
                    r = reals[0]

                rd2_cache[rd2] = r

            r = rd2_cache[rd2]
            # 2.15
            theta = np.arctan2(x, y)
            # 2.14
            y = r * cos(theta)
            x = r * sin(theta)

            # TODO determine if cast to int is OK
            new_v = int(y + y0)
            new_u = int(x + x0)
            distortion_mat[u, v] = (new_u, new_v)

    # NOTE that vectorize didn't offer much performance improvement sadly
    return distortion_mat

def apply_distortion_mat(image: Mat, distortion_mat: Mat):
    # Takes ~ 0.005 seconds for 1920x1080 with ints on M1.
    # TODO Could save read this from cache instead.
    transposed_dist_mat = distortion_mat.transpose((2, 0, 1))
    map_y = transposed_dist_mat[0].astype(np.float32)
    map_x = transposed_dist_mat[1].astype(np.float32)

    # Takes ~ 0.0035 seconds for 1920x1080 with ints on M1
    # TODO Need to experiment with interpolation
    start = time.time()
    new_img = remap(image, map_x, map_y, INTER_LINEAR)
    print(f'Took {time.time() - start} to remap')
    return new_img

    
def apply_distortion_mat_naive(image: Mat, distortion_mat: Mat):
    """
    My home built code to perform the same task as `cv2.remap` with no interpolation.
    The logic is very boilerplate so I feel OK replacing it with the cv2 function and not building my
    own faster version. Complex or interesting algorithms should be implemented by hand.

    Takes ~1.5 seconds for a 1920x1080 image on M1
    """
    height, width, _ = image.shape
    new_image = np.empty_like(image)
    for u in range(0, height):
        for v in range(0, width):
            new_image[u, v] = image[tuple(distortion_mat[u, v])]
    return new_image

def distortion_mat_filename(image_dim: Tuple[int, int], distortion_coefficients: list):
    height, width = image_dim
    return f'dim_{width}x{height}_coeff_{"_".join([str(x) for x in distortion_coefficients])}.npy'
