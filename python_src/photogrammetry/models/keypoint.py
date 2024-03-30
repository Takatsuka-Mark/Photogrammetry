import numpy as np
from photogrammetry.storage.image_db import ImageDB


class KeyPoint:
    def __init__(self, image_id: int, coord: np.ndarray, gaussian_pairs, image_db: ImageDB) -> None:
        # TODO remove "image" once the image store is created.
        # TODO determine how to pass guassian_pairs. Maybe in a constant parameter store.
        self._image_id = image_id
        self._coord = coord
        self._descriptor = None
        self._gaussian_pairs = gaussian_pairs
        self._image_db = image_db
    
    @classmethod
    def from_reference(cls, keypoint):
        # TODO this needs implementing.
        return 

    @property
    def coord(self):
        return self._coord
    
    @property
    def descriptor(self):
        # TODO rename to brief descriptor?
        if self._descriptor is None:
            self._descriptor = self._brief_descriptor()
            pass
        return self._descriptor

    def _brief_descriptor(self) -> int:
        des = 0
        u, v = self._coord
        bw_image = self._image_db.get_bw_image(self._image_id)
        img_height, img_width = self._image_db.dim
        for idx, pair in enumerate(self._gaussian_pairs):
            if (
                (0 > pair[0][0] + u or pair[0][0] + u >= img_height) or
                (0 > pair[1][0] + u or pair[1][0] + u >= img_height) or
                (0 > pair[0][1] + v or pair[0][1] + v >= img_width) or
                (0 > pair[1][1] + v or pair[1][1] + v >= img_width)
            ):
                # TODO determine how to handle pairs outside of the img.
                continue
            p1 = bw_image[pair[0][0] + u, pair[0][1] + v]
            p2 = bw_image[pair[1][0] + u, pair[1][1] + v]
            if p1 < p2:
                des += (2**idx)
        return des
    
def generate_gaussian_pairs(stdev, num_pairs=256):
    # TODO there are better algorithms for pair selection. See https://www.cs.ubc.ca/~lowe/525/papers/calonder_eccv10.pdf
    pairs = np.zeros((num_pairs, 2, 2), dtype=np.int64)    # TODO does this really need to be 64?
    for idx in range(num_pairs):
        pairs[idx] = [np.rint(np.random.normal([0, 0], stdev)).astype(np.int64), np.rint(np.random.normal([0, 0], stdev)).astype(np.int64)]
    return pairs
