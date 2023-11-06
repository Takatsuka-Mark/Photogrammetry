from cv2 import imread, imwrite
import numpy as np

image = imread('./data/feature_matching_test/15pt_star.png')
new_image = np.zeros_like(image)
height, width, _ = image.shape

offset = 150


for row in range(height):
    for col in range(width - offset):
        new_image[row, col+offset] = image[row, col]

imwrite('./data/feature_matching_test/15pt_star_shifted_150.png', new_image)
