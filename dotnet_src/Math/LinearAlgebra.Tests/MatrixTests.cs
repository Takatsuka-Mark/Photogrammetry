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
            [0, 0] = 1f,
            [0, 1] = 2f,
            [1, 0] = 3f,
            [1, 1] = 4f
        };

        var resultMatrix = matrix.Transpose();

        Assert.Equal(1f, resultMatrix[0, 0]);
        Assert.Equal(2f, resultMatrix[1, 0]);
        Assert.Equal(3f, resultMatrix[0, 1]);
        Assert.Equal(4f, resultMatrix[1, 1]);
    }
}