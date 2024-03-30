from cv2 import Mat, cvtColor, COLOR_BGR2GRAY
import numpy as np

# TODO probably move to a different package.
class ImageDB:
    def __init__(self, image_height, image_width) -> None:
        self._images = {}
        self._cur_img_id = 0
        self._image_width = image_width
        self._image_height = image_height

    @property
    def dim(self):
        return self._image_height, self._image_width

    def add_image(self, image: Mat) -> int:
        """
        Given an image, returns the image ID.
        """
        # TODO ensure that this is a color image passed in?
        image_id = self._cur_img_id
        self._images[image_id] = {
            "bgr_original": image
        }
        self._cur_img_id += 1
        return image_id

    def get_image(self, image_id: int) -> Mat:
        self._raise_if_image_is_missing(image_id)
        return self._images[image_id]["bgr_original"]
    
    def get_bw_image(self, image_id: int) -> np.ndarray:
        self._raise_if_image_is_missing(image_id)
        
        if "bw_i16" not in self._images[image_id]:
            self._images[image_id]["bw_i16"] = cvtColor(self._images[image_id]["bgr_original"], COLOR_BGR2GRAY).astype(np.int16)
        return self._images[image_id]["bw_i16"]

    def _raise_if_image_is_missing(self, image_id: int):
        if image_id not in self._images:
            raise ValueError(f"The image with the ID {image_id} could not be found in the ImageDB")
