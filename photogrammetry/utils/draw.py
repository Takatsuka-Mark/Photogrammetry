def draw_point(image, point, height, width):
    x = point[0]
    y = point[1]
    # TODO need to clamp this inside of frame
    for u in range(x-2, x+3):
        for v in range(y-2, y+3):
            if u >= height or u < 0 or v >= width or v < 0:
                continue
            image[u, v] = [0, 255, 0]
    pass