from photogrammetry.models.keypoint import KeyPoint
import numpy as np
from dataclasses import dataclass
import math


@dataclass
class _HierarchicalCluster:
    cluster_id: int
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
        self.z = np.zeros((self.num_keypoints * 2 - 1, 4), dtype=np.int32)    # TODO is 32 sufficient?
        # TODO add check for number of keypoints, and constant for max ram allowed to allocate.
        # this can easly blow up.
        # TODO add check for 4k max image size. Assuming it is 4k, the max distance edge to edge is ~8.3mil. So, really we only need 24ish unsigned bits. But, 32 will do I guess... Probably should be unsigned but causing problems?
        self.cluster_dist_map = np.full((self.num_keypoints * 2 - 1, self.num_keypoints * 2 - 1), fill_value=-1, dtype=np.int32)    # TODO change dtype when impleemnting other distance measures
        self._initialize_clusters(keypoints)

    def _initialize_clusters(self, keypoints: list[KeyPoint]) -> None:
        for keypoint in keypoints:
            self.cluster_map[self.num_clusters_acc] = _HierarchicalCluster(self.num_clusters_acc, 1, keypoint.coord, [keypoint])
            self.active_clusters.add(self.num_clusters_acc)
            self.num_clusters_acc += 1

    def _cluster_distance(self, cluster_id_1: int, cluster_id_2: int) -> int:
        # TODO implement different types of distances
        # This is the city block distance.
        cluster_1_center = self.cluster_map[cluster_id_1].center
        cluster_2_center = self.cluster_map[cluster_id_2].center
        return sum(np.abs(cluster_1_center - cluster_2_center))

    def _compute_new_center(self, cluster_id_1: int, cluster_id_2: int) -> np.ndarray:
        cluster1 = self.cluster_map[cluster_id_1]
        cluster2 = self.cluster_map[cluster_id_2]

        return np.divide(((cluster1.center * cluster1.num_items) + (cluster2.center * cluster2.num_items)), cluster1.num_items + cluster2.num_items)

    def _merge_clusters(self, cluster_id_1: int, cluster_id_2: int, distance):
        cluster_id = self.num_clusters_acc
        self.num_clusters_acc += 1
        self.active_clusters.discard(cluster_id_1)
        self.active_clusters.discard(cluster_id_2)   # TODO discard vs remove?
        self.active_clusters.add(cluster_id)
        num_observations = self.cluster_map[cluster_id_1].num_items + self.cluster_map[cluster_id_2].num_items
        combined_keypoints = self.cluster_map[cluster_id_1].keypoints + self.cluster_map[cluster_id_2].keypoints
        self.cluster_map[cluster_id] = _HierarchicalCluster(
            cluster_id,
            num_observations,
            self._compute_new_center(cluster_id_1, cluster_id_2),
            combined_keypoints
        )
        self.z[cluster_id] = [cluster_id_1, cluster_id_2, distance, num_observations]
        return cluster_id

    def _min_dist_clusters(self):
        # Computes the distance between all active clusters and returns minimums
        min_dist = math.inf
        min_dist_cluster1 = -1
        min_dist_cluster2 = -1
        active_cluster_list = list(self.active_clusters)
        # TODO this shouldn't look through all every time. We can form an ordered list of what has been best so far,
        # merge in any new clusters, and pop the first value.
        for c1_idx, cluster1 in enumerate(active_cluster_list[:-1]):
            for cluster2 in active_cluster_list[c1_idx + 1:]:
                dist = self.cluster_dist_map[cluster1, cluster2]
                if dist == -1:
                    dist = self._cluster_distance(cluster1, cluster2)
                    self.cluster_dist_map[cluster1, cluster2] = dist    # TODO This is clunky but I'll go with it for now.
                    self.cluster_dist_map[cluster2, cluster1] = dist
                if dist < min_dist:
                    min_dist = dist
                    min_dist_cluster1 = cluster1
                    min_dist_cluster2 = cluster2
        
        return min_dist, min_dist_cluster1, min_dist_cluster2

    def run_clustering(self):
        # First, merge all clusters.
        while len(self.active_clusters) > 1:
            # While there are still clusters to be merged, 
            
            # TODO the maximum merge distance should probably be computed as a function of the size.
            # TODO also, the max merge distance shouldn't be the only metric for ending clustering,
            # We need some form of detection to say, ok, we just performed a massive merge overall, cut it here.
            min_dist, min_dist_cluster1, min_dist_cluster2 = self._min_dist_clusters()
            if min_dist > self.max_merge_distance:
                # We have likely completed the best near merges.
                print(min_dist)
                break
            self._merge_clusters(min_dist_cluster1, min_dist_cluster2, min_dist)
        
        clustered_keypoints = []
        # Since we are short circuting the loop, we can just take all active clusters and call it good.
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



"""
Result: (n - 1) by 4 matrix Z.

At i'th iteration, clusters with indicies z[i, 0] and z[i, 1] are combined to form cluster n+1

a cluster with an index less than n corresponds to one of n original observations.

Each row looks like [cluster0_id, cluster1_id, dist_from_0_to_1, number of observations in cluster.]

"""

