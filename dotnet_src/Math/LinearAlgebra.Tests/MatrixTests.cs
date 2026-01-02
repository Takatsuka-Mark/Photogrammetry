namespace LinearAlgebra.Tests;

public class MatrixTests
{
    [Fact]
    public void TestCreateMatrix()
    {
        var matrix = new Matrix<float>(new MatrixDimensions{Rows = 2, Cols = 2});
        matrix.Get(0, 0);
    }

    [Fact]
    public void TestCreateMatrixWithFill()
    {
        var matrix = new Matrix<float>(new MatrixDimensions{Rows = 2, Cols = 2}, emptyFill: 0.67f);

        foreach (var value in matrix.IterateAll())
        {
            Assert.Equal(0.67f, value);   
        }
    }
    
    [Fact]
    public void TestMatrixFill()
    {
        var matrix = new Matrix<float>(new MatrixDimensions{Rows = 2, Cols = 2}, emptyFill: 0f);
        
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
}