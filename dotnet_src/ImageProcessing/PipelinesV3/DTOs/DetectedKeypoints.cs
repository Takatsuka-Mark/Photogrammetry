using ImageProcessing.Abstractions;

namespace ImageProcessing.PipelinesV3.DTOs;

public record DetectedKeypoints(GrayscaleImagePair ImagePair, List<Keypoint> Keypoints);