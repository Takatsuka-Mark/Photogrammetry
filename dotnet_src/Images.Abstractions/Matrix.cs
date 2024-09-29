using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;

namespace Images.Abstractions;

public class Matrix<TData>
{
    // TODO maybe eventually allow for creation of N dimensional matrix?
    public MatrixDimensions Dimensions { get; }

    private readonly MatrixStorage<TData> _storage;

    public Matrix(MatrixDimensions dimensions)
    {
        Dimensions = dimensions;
        _storage = new MatrixStorage<TData>(dimensions);
    }

    internal Matrix(MatrixStorage<TData> storage)
    {
        Dimensions = storage.Dimensions;
        _storage = storage;
    }

    // TODO maybe some of this should go into a Matricies Project
    
    public static Matrix<TData> FromColMajorArray(TData[,] colMajorArray)
    {
        return new Matrix<TData>(MatrixStorage<TData>.FromColMajorArray(colMajorArray));
    }
    
    public static Matrix<TData> FromRowMajorArray(TData[,] rowMajorArray)
    {
        return new Matrix<TData>(MatrixStorage<TData>.FromRowMajorArray(rowMajorArray));
    }

    public TData this[int x, int y]
    {
        get => _storage[x, y];
        set => _storage[x, y] = value;
    }

    public bool CoordsAreValid(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Dimensions.Width && y < Dimensions.Height;
    }
    
    public void ValidateCoords(int x, int y)
    {
        if (!CoordsAreValid(x, y))
            throw new ArgumentOutOfRangeException($"Received X:{x}, Y:{y} for matrix of Width:{Dimensions.Width}, Height:{Dimensions.Height}");
    }

    public Matrix<TData> Transpose()
    {
        // TODO should probably build a way to do this without checks.
        var transposedMatrix = new Matrix<TData>(new MatrixDimensions(Dimensions.Height, Dimensions.Width));

        for (var y = 0; y < Dimensions.Height; y++)
        {
            for (var x = 0; x < Dimensions.Width; x++)
            {
                transposedMatrix[y, x] = this[x, y];
            }
        }

        return transposedMatrix;
    }

    public TData[] Row(int y)
    {
        return _storage.Row(y);
    }

    public TData[] GetColumn(int x)
    {
        return _storage.Column(x);
    }

    public void Draw()
    {
        Console.WriteLine("[");
        for (int y = 0; y < Dimensions.Height; y++)
        {
            var items = new List<TData>();
            for (int x = 0; x < Dimensions.Width; x++)
            {
                items.Add(this[x, y]);
            }

            Console.WriteLine($"[{String.Join(", ", items)}],");
        }
        Console.WriteLine("]");
    }


    public delegate TOutData PixelConverter<out TOutData>(TData dataIn);
    public Matrix<TNewData> Convert<TNewData>(PixelConverter<TNewData> pixelConverter)
    {
        // TODO should create a generic map function that does this double for loop generation.
        var outputMatrix = new Matrix<TNewData>(Dimensions);

        for (var x = 0; x < Dimensions.Width; x++)
        {
            for (var y = 0; y < Dimensions.Height; y++)
            {
                outputMatrix[x, y] = pixelConverter(_storage[x, y]);
            }
        }
        
        return outputMatrix;
    }

    public void DrawSquare(int x, int y, int radius, TData dataToDraw)
    {
        ValidateCoords(x, y);

        for (var u = x - radius; u < x + radius; u += 1)
        {
            for (var v = y - radius; v < y + radius; v += 1)
            {
                _storage.SetOrDoNothing(u, v, dataToDraw);
            }
        }
    }

    public void DrawLine(Coordinate p1, Coordinate p2, TData dataToDraw)
    {
        // Using bresehnam's line drawer
        ValidateCoords(p1.X, p1.Y);
        ValidateCoords(p2.X, p2.Y);

        var dx = Math.Abs(p2.X - p1.X);
        var dy = Math.Abs(p2.Y - p1.Y);
        var sx = p1.X < p2.X ? 1 : -1;
        var sy = p1.Y < p2.Y ? 1 : -1;
        var err = dx - dy;

        var x = p1.X;
        var y = p1.Y;

        // TODO Kinda scary to have a while(true)
        while (true){
            _storage.SetOrDoNothing(x, y, dataToDraw);

            if (x == p2.X && y == p2.Y)
                break;

            var err2 = 2 * err;

            if (err2 >= -dy){
                err -= dy;
                x += sx;
            }
            if (err2 <= dx){
                err += dx;
                y += sy;
            }
        }
    }
}