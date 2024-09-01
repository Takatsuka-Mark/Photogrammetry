using System.Collections.Immutable;
using System.Numerics;
using ImageProcessing.Abstractions;
using Images.Abstractions;

namespace ImageProcessing;

public class KeypointMatching
{
    private readonly int _hammingThreshold;

    public KeypointMatching(int hammingThreshold)
    {
        _hammingThreshold = hammingThreshold;
    }

    public List<KeypointPair> MatchKeypoints(List<Keypoint> keypoints1, List<Keypoint> keypoints2)
    {
        // TODO introduce hamming distance.
        var k1ToK2ToDistance = new Dictionary<int, Dictionary<int, int>>();
        // var k1ToK2ToDistance = new Matrix<int>(new MatrixDimensions(keypoints1.Count, keypoints2.Count));
        
        for (var k1Idx = 0; k1Idx < keypoints1.Count; k1Idx++)
        {
            var k1 = keypoints1[k1Idx];
            var k2ToDistance = new Dictionary<int, int>();
            for (var k2Idx = 0; k2Idx < keypoints2.Count; k2Idx++)
            {
                var k2 = keypoints2[k2Idx];
                // k1ToK2ToDistance[k1Idx, k2Idx] = CountOnes(k1.BriefDescriptor ^ k2.BriefDescriptor);
                k2ToDistance.Add(k2Idx, CountOnes(k1.BriefDescriptor ^ k2.BriefDescriptor));
            }
            k1ToK2ToDistance.Add(k1Idx, k2ToDistance);
        }

        var keypointPairs = new List<KeypointPair>();
        var availableK1Idx = Enumerable.Range(0, keypoints1.Count).ToHashSet();
        var availableK2Idx = Enumerable.Range(0, keypoints2.Count).ToHashSet();
        
        // TODO This could be done so much more efficiently. this is just POC.
        while (keypointPairs.Count < keypoints1.Count)
        {
            var smallestDistance = int.MaxValue;
            var smallestDistanceK1Idx = 0;
            var smallestDistanceK2Idx = 0;
            // Find the smallest available match.
            foreach (var k1Idx in availableK1Idx)
            {
                var k2s = k1ToK2ToDistance[k1Idx];
                foreach (var k2Idx in availableK2Idx)
                {
                    if (smallestDistance <= k2s[k2Idx])
                        continue;
                    smallestDistance = k2s[k2Idx];
                    smallestDistanceK1Idx = k1Idx;
                    smallestDistanceK2Idx = k2Idx;
                }
            }
            
            keypointPairs.Add(new KeypointPair
            {
                Distance = smallestDistance,
                Keypoint1 = keypoints1[smallestDistanceK1Idx],
                Keypoint2 = keypoints2[smallestDistanceK2Idx]
            });

            availableK1Idx.Remove(smallestDistanceK1Idx);
            availableK2Idx.Remove(smallestDistanceK2Idx);
        }
        
        return keypointPairs;
    }

    private static int CountOnes(BigInteger value)
    {
        var numOnes = 0;

        while (value != 0)
        {
            value &= (value - 1);
            numOnes += 1;
        }
        
        return numOnes;
    }
}