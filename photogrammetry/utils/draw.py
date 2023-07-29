def draw_point(image, point, height, width, sqr_radius:int = 3):
    x = point[0]
    y = point[1]
    # TODO need to clamp this inside of frame
    for u in range(x-sqr_radius+1, x+sqr_radius):
        for v in range(y-sqr_radius+1, y+sqr_radius):
            if u >= height or u < 0 or v >= width or v < 0:
                continue
            image[u, v] = [0, 255, 0]
    pass