namespace LinearAlgebra.Tests;

public class MatrixTests
{
    [Fact]
    public void TestCreate()
    {
        var matrix = new Matrix<float>(new MatrixDimensions { Height = 2, Width = 2 });
        matrix.Get(0, 0);
    }

    [Fact]
    public void TestCreateWithFill()
    {
        var matrix = new Matrix<float>(new MatrixDimensions { Height = 2, Width = 2 }, emptyFill: 0.67f);

        foreach (var value in matrix.IterateAll())
        {
            Assert.Equal(0.67f, value);
        }
    }

    [Fact]
    public void TestFill()
    {
        var matrix = new Matrix<float>(new MatrixDimensions { Height = 2, Width = 2 }, emptyFill: 0f);

        foreach (var value in matrix.IterateAll())
        {
            Assert.NotEqual(0.67f, value);
        }

        matrix.Fill(0.67f);

        foreach (var value in matrix.IterateAll())
        {
            Assert.Equal(0.67f, value);
        }
    }

    [Fact]
    public void TestTranspose()
    {
        var matrix = new Matrix<float>(new MatrixDimensions { Height = 2, Width = 2 }, emptyFill: 0f)
        {
            [(ushort)0, (ushort)0] = 1f,
            [(ushort)0, (ushort)1] = 2f,
            [(ushort)1, (ushort)0] = 3f,
            [(ushort)1, (ushort)1] = 4f
        };

        var resultMatrix = matrix.Transpose();

        Assert.Equal(1f, resultMatrix[0, 0]);
        Assert.Equal(2f, resultMatrix[1, 0]);
        Assert.Equal(3f, resultMatrix[0, 1]);
        Assert.Equal(4f, resultMatrix[1, 1]);
    }
    
    [Fact]
    public void TestTransposeRectangular()
    {
        var matrix = new Matrix<float>(new MatrixDimensions { Height = 2, Width = 1 }, emptyFill: 0f)
        {
            [(ushort)0, (ushort)0] = 1f,
            [(ushort)0, (ushort)1] = 2f
        };

        var resultMatrix = matrix.Transpose();

        Assert.Equal(1f, resultMatrix[0, 0]);
        Assert.Equal(2f, resultMatrix[1, 0]);
    }

    [Fact]
    public void TestConvert()
    {
        var matrix = new Matrix<float>(new MatrixDimensions { Height = 2, Width = 1 }, emptyFill: 0f)
        {
            [(ushort)0, (ushort)0] = 1f,
            [(ushort)0, (ushort)1] = 2f
        };

        var convertedMatrix = (Matrix<int>)matrix.Convert((_, f) => (int)f);
        
        Assert.Equal(matrix.Dimensions, convertedMatrix.Dimensions);
        Assert.Equal(1, convertedMatrix[0, 0]);
        Assert.Equal(2, convertedMatrix[0, 1]);
    }
}