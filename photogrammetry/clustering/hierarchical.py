from photogrammetry.models.keypoint import KeyPoint
import numpy as np
from dataclasses import dataclass
from typing import Optional
import multiprocessing
from abc import ABC, abstractmethod


@dataclass
class _HierarchicalCluster:
    num_items: int
    center: np.ndarray
    keypoints: list[KeyPoint]


class HierarchicalClustering:
    def __init__(self, keypoints: list[KeyPoint], max_merge_distance: int = 25) -> None:
        # TODO the max merge distance should be scaled via a percentage of the image size..
        self.max_merge_distance = max_merge_distance
        # Maps ID, to cluster object
        self.cluster_map: dict[int, _HierarchicalCluster] = {}
        self.num_clusters_acc = 0
        self.active_clusters = set()    # Cluster ids
        self.num_keypoints = len(keypoints)
        self.z = np.zeros((max(self.num_keypoints * 2 - 1, 0), 4), dtype=np.int32)    # TODO is 32 sufficient?
        self.sorted_cluster_distances = []
        self._initialize_clusters(keypoints)

    def _initialize_clusters(self, keypoints: list[KeyPoint]) -> None:
        for keypoint in keypoints:
            self._add_new_cluster(_HierarchicalCluster(1, keypoint.coord, [keypoint]), delay_sort=True)
        self.sorted_cluster_distances.sort(key=lambda x: x[0])

    def _cluster_distance(self, cluster_id_1: int, cluster_id_2: int) -> int:
        # TODO implement different types of distances
        # This is the city block distance.
        return sum(np.abs(
            self.cluster_map[cluster_id_1].center -
            self.cluster_map[cluster_id_2].center
        ))

    def _compute_new_center(self, cluster_id_1: int, cluster_id_2: int) -> np.ndarray:
        cluster1 = self.cluster_map[cluster_id_1]
        cluster2 = self.cluster_map[cluster_id_2]

        return np.divide(((cluster1.center * cluster1.num_items) + (cluster2.center * cluster2.num_items)), cluster1.num_items + cluster2.num_items)

    def _merge_clusters(self, cluster_id_1: int, cluster_id_2: int, distance):
        self._remove_clusters([cluster_id_1, cluster_id_2])
        num_observations = self.cluster_map[cluster_id_1].num_items + self.cluster_map[cluster_id_2].num_items
        combined_keypoints = self.cluster_map[cluster_id_1].keypoints + self.cluster_map[cluster_id_2].keypoints
        cluster_id = self._add_new_cluster(_HierarchicalCluster(
            num_observations,
            self._compute_new_center(cluster_id_1, cluster_id_2),
            combined_keypoints
        ))
        self.z[cluster_id] = [cluster_id_1, cluster_id_2, distance, num_observations]
        return cluster_id

    def _add_new_cluster(self, cluster: _HierarchicalCluster, delay_sort=False) -> int:
        cluster_id = self.num_clusters_acc
        self.num_clusters_acc += 1
        self.cluster_map[cluster_id] = cluster

        # TODO is there some way that we can only store a few minimum distances?
        # It does not make sense to store just one. But, how many are required?
        # min_dist = math.inf
        # c1 = -1
        # c2 = -1
        for cluster1 in self.active_clusters:
            dist = self._cluster_distance(cluster1, cluster_id)
            if dist > self.max_merge_distance:
                continue
            self.sorted_cluster_distances.append((dist, cluster1, cluster_id))
        # TODO this is horrible that we're sorting every time (especially when we're only adding a single element at once.)
        # Perhaps we should offload sorting or allow it to be disabled.
        if not delay_sort:
            self.sorted_cluster_distances.sort(key=lambda x: x[0])

        self.active_clusters.add(cluster_id)
        return cluster_id

    def _remove_clusters(self, cluster_ids: list[int]):
        # TODO This could technically be slower than just checking if a given cluster distance includes
        # non-existent cluster_ids when popping it. It could be better to mix the two techniques. 
        for cluster_id in cluster_ids:
            self.active_clusters.discard(cluster_id)

        # Start at end of sorted_cluster_distances and pop elements to front.
        for i in reversed(range(len(self.sorted_cluster_distances))):
            _, c1, c2 = self.sorted_cluster_distances[i]
            if c1 in cluster_ids or c2 in cluster_ids:
                self.sorted_cluster_distances.pop(i)

    # TODO rename to pop?
    def _min_dist_clusters(self) -> Optional[tuple[int, int, int]]:
        # TODO add guard for if there are no clusters remaining
        # TODO this could sort every time it returns? That would be faster than each time it adds a cluster maybe...
        if len(self.sorted_cluster_distances) == 0:
            return None
        return self.sorted_cluster_distances.pop(0)

    def run_clustering(self) -> list:
        # First, merge all clusters.
        while len(self.active_clusters) > 1:
            # While there are still clusters to be merged, 
            
            # TODO the maximum merge distance should probably be computed as a function of the cluster size.
            # TODO also, the max merge distance shouldn't be the only metric for ending clustering,
            # We need some form of detection to say, ok, we just performed a massive merge overall, cut it here.
            min_dist_tup = self._min_dist_clusters()
            # print(min_dist, min_dist_cluster1, min_dist_cluster2)
            if min_dist_tup is None:
                break
            min_dist, min_dist_cluster1, min_dist_cluster2 = min_dist_tup
            # TODO should make merge_clusters take a tuple or perhaps a dataclass
            self._merge_clusters(min_dist_cluster1, min_dist_cluster2, min_dist)

        clustered_keypoints = []
        # Since we are short circuting the loop, we can just take all active clusters and call it good.
        # print(self.active_clusters)
        for cluster_id in self.active_clusters:
            cluster = self.cluster_map[cluster_id]
            if cluster.num_items == 0:
                clustered_keypoints.append(cluster.keypoints[0])
                continue
            ref_keypoint = cluster.keypoints[0]
            clustered_keypoints.append(
                KeyPoint(   # TODO replace with KeyPoint.from_reference when done.
                    image_id=ref_keypoint._image_id,
                    coord=np.round(cluster.center).astype(np.int32),
                    gaussian_pairs=ref_keypoint._gaussian_pairs,
                    image_db=ref_keypoint._image_db
                )
            )
        return clustered_keypoints

class BaseChunkedHierarchicalClustering(ABC):
    def __init__(
        self, img_dim: tuple[int, int], keypoints: list[KeyPoint], chunks_dim: tuple[int, int] = (4, 4),
        max_merge_dist: int = 25, edge_merge_dist: Optional[int] = None
    ) -> None:
        self.chunks_dim = chunks_dim
        self.max_merge_distance = max_merge_dist
        self.height_chunk_off = img_dim[0] // chunks_dim[0]
        self.width_chunk_off = img_dim[1] // chunks_dim[1]
        self._init_keypoint_chunks(keypoints)
    
    @abstractmethod
    def _init_keypoint_chunks(self, keypoints):
        raise NotImplementedError()

    def _keypoint_to_chunk_idx(self, keypoint):
        h_chunk = min(keypoint.coord[0] // self.height_chunk_off, self.chunks_dim[0] - 1)
        w_chunk = min(keypoint.coord[1] // self.width_chunk_off, self.chunks_dim[1] - 1)
        return h_chunk, w_chunk

    def _cluster_chunk(self, keypoints: list) -> list:
        if len(keypoints) == 0:
            return []
        hc = HierarchicalClustering(keypoints, self.max_merge_distance)
        return hc.run_clustering()

    @abstractmethod
    def run_clustering(self):
        raise NotImplementedError()


class ChunkedHierarchicalClustering(BaseChunkedHierarchicalClustering):    
    def _init_keypoint_chunks(self, keypoints):
        keypoints_by_chunk = [[[] for _ in range(self.chunks_dim[1])] for _ in range(self.chunks_dim[0])]
        for keypoint in keypoints:
            h_chunk, w_chunk = self._keypoint_to_chunk_idx(keypoint)
            keypoints_by_chunk[h_chunk][w_chunk].append(keypoint)
        self.chunked_keypoints = keypoints_by_chunk

    def run_clustering(self) -> list:
        clustered_keypoints_acc = []
        for chunk_h in range(self.chunks_dim[0]):
            for chunk_w in range(self.chunks_dim[1]):
                clustered_keypoints_acc.extend(
                    self._cluster_chunk(self.chunked_keypoints[chunk_h][chunk_w])
                )
        return clustered_keypoints_acc

class ChunkedHierarchicalClusteringMultithreaded(BaseChunkedHierarchicalClustering):
    def __init__(self, img_dim: tuple[int, int], keypoints: list[KeyPoint], chunks_dim: tuple[int, int] = (4, 4), max_merge_dist: int = 25, edge_merge_dist: Optional[int] = None, chunks_per_thread: Optional[int] = None) -> None:
        self.chunks_per_thread = chunks_per_thread or 2
        super().__init__(img_dim, keypoints, chunks_dim, max_merge_dist, edge_merge_dist)
    
    def _init_keypoint_chunks(self, keypoints):
        self.chunked_keypoints = [[] for _ in range(self.chunks_dim[0] * self.chunks_dim[1])]
        for keypoint in keypoints:
            h_chunk, w_chunk = self._keypoint_to_chunk_idx(keypoint)
            self.chunked_keypoints[h_chunk * self.chunks_dim[1] + w_chunk].append(keypoint)

    def run_clustering(self) -> list:
        clustered_keypoints_acc = []
        
        # TODO set size of pool manually?
        pool = multiprocessing.Pool()
        outputs = pool.map(self._cluster_chunk, [chunk for chunk in self.chunked_keypoints], chunksize=self.chunks_per_thread)
        for output in outputs:
            clustered_keypoints_acc.extend(output)
        return clustered_keypoints_acc
