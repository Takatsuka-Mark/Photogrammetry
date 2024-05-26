
namespace ImageProcessing;


// # TODO dynamically create?
// # Points 1 to 16, in format (height_offset, width_offset) relative to P
// BRESENHAM_CIRCLE_3 = np.array([
//     [-3, 0],
//     [-3, 1],
//     [-2, 2],
//     [-1, 3],
//     [0, 3],
//     [1, 3],
//     [2, 2],
//     [3, 1],
//     [3, 0],
//     [3, -1],
//     [2, -2],
//     [1, -3],
//     [0, -3],
//     [-1, -3],
//     [-2, -2],
//     [-3, -1],
// ])
// MINI_BRESENHAM_CIRCLE_3 = BRESENHAM_CIRCLE_3[[0, 4, 8, 12]]
// # Going from [[x, y], [x2, y2], ...] to [[x, x2, ...], [y, y2, ...]]
// BRESENHAM_CIRCLE_3_TP = BRESENHAM_CIRCLE_3.transpose((1, 0))
// MINI_BRESENHAM_CIRCLE_3_TP = MINI_BRESENHAM_CIRCLE_3.transpose((1, 0))


public class KeypointDetection
{
    private static int[,] _bresenhamCircle3 = {{-3, 0}, {-3, 1}, {-2, 2}, {-1, 3}, {0, 3}, {1, 3}, {2, 2}, {3, 1}, {3, 0}, {3, -1}, {2, -2}, {1, -3}, {0, -3}, {-1, -3}, {-2, -2}, {-3, 1}};
    // private static int[,] _miniBresenhamCircle3;
    
    
}
