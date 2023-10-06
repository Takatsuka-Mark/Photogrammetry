from pathlib import Path
import json
from typing import Optional
from dataclasses import dataclass
from uuid import uuid4
import pickle


@dataclass
class KeypointCacheInfo:
    img_hash: int
    is_clustered: bool
    fast_detection_threshold: Optional[int] = None

    def as_dict(self):
        return {
            "img_hash": self.img_hash,
            "is_clustered": self.is_clustered,
            "fast_detection_threshold": self.fast_detection_threshold
        }

# TODO make a version of this that can take a general keypoint detector / clusterer
# and decorate the function S.T. any calls to "get_keypoints" will be cached.
class KeypointCache:
    def __init__(self, data_dir="./data/tmp/keypoint_cache") -> None:
        self.base_dir_path = Path(data_dir)
        self.index_path = self.base_dir_path.joinpath('index.json')
        self._create_cache_dir_if_not_exists()
        self.index_content = self._create_or_get_index()

    def get_keypoints_if_exist(self, cache_info: KeypointCacheInfo) -> Optional[list]:
        if uid := self._exists_in_cache(cache_info):
            with open(self.base_dir_path.joinpath(f"{uid}.dat"), "rb") as f:
                return pickle.load(f)
        return None

    def store_keypoints_if_not_exist(self, keypoints: list, cache_info: KeypointCacheInfo):
        if self._exists_in_cache(cache_info):
            return
        uid = str(uuid4())
        self.index_content[uid] = cache_info.as_dict()
        with open(self.base_dir_path.joinpath(f"{uid}.dat"), 'wb') as f:
            pickle.dump(keypoints, f)
        self._write_index()
        return uid

    def _exists_in_cache(self, cache_info: KeypointCacheInfo):
        for uid, in_cache_info in self.index_content.items():
            if in_cache_info == cache_info.as_dict():
                return uid
        return False

    def _create_cache_dir_if_not_exists(self):
        if self.base_dir_path.exists() and self.base_dir_path.is_dir():
            return
        self.base_dir_path.mkdir()

    def _create_or_get_index(self):
        if not self.index_path.exists():
            return {}
        with open(self.index_path, 'r') as f:
            index_json = json.load(f)
        return index_json

    def _write_index(self):
        with open(self.index_path, 'w') as f:
            json.dump(self.index_content, f)
