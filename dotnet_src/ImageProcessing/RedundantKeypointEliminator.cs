using ImageProcessing;
using ImageProcessing.Abstractions;
using MathNet.Numerics;

public class RedundantKeypointEliminator
{
    private readonly int supressionRadius;

    public RedundantKeypointEliminator(int supressionRadius)
    {
        this.supressionRadius = supressionRadius;
    }

    public List<Keypoint> EliminateRedundantKeypoints(List<Keypoint> keypoints)
    {
        // Uses NMS (Non Maximum Suppression)

        keypoints = keypoints.OrderByDescending(keypoint => keypoint.FastScore).ToList();
        var acceptableList = new List<Keypoint>();

        while(keypoints.Count > 0){
            // TODO queue time?
            var maximalKeypoint = keypoints[0];
            acceptableList.Add(maximalKeypoint);
            keypoints.RemoveAt(0);

            // TODO is there really any reason to use a list? I'm not sure.
            keypoints = keypoints.Where(keypoint => IsAcceptableDistance(keypoint, maximalKeypoint)).ToList();
        }

        return acceptableList;
    }

    public bool IsAcceptableDistance(Keypoint kp1, Keypoint kp2){
        return Utils.CoordinateDistance(kp1.Coordinate, kp2.Coordinate) > supressionRadius;
    }

    // TODO implement ANMS (Adaptive NMS)
}