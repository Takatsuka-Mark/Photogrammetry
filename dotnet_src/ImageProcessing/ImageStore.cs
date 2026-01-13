using ImageProcessing.Abstractions;
using Images.Abstractions;
using Images.Abstractions.Pixels;

namespace ImageProcessing;

public class ImageStore
{
    private int _imageIdKey = -1;
    private readonly Dictionary<int, int> _imageVariantIdStore = new ();
    private readonly Dictionary<ImageVariantKey, Matrix<Rgba>> _rgbaMatrixStore = new();
    private readonly Dictionary<ImageVariantKey, Matrix<Grayscale>> _grayscaleMatrixStore = new();
    
    public ImageStore()
    {
        // TODO this is horrible. But I want something quick and dirty that'll get the job done.
        // TODO can I do this somehow without duplicate functions?
        // TODO could I abstract this out to just a generic store?
        // Then could have a store controller that determines which to pull from. Then
        // Pipeline stuff would just be pulled from 
    }

    public ImageVariantKey StoreImage(Matrix<Rgba> rgba)
    {
        // TODO should store the image with an ID. But we also care about the version...
        // TODO should this be made generic to also store keypoints?
        
        // Stores a completely unique image.
        var key = GetNextKey();
        _rgbaMatrixStore.Add(key, rgba);
        return key;
    }

    public ImageVariantKey StoreImage(Matrix<Grayscale> grayscale)
    {
         var key = GetNextKey();
         _grayscaleMatrixStore.Add(key, grayscale);
         return key;
    }
    
    public ImageVariantKey StoreImageVariant(int imageId, Matrix<Rgba> rgba)
    {
        // Stores a variant of a given image.
        var key = GetNextKey(imageId);
        _rgbaMatrixStore.Add(key, rgba);
        return key;
    }
    
    public ImageVariantKey StoreImageVariant(int imageId, Matrix<Grayscale> grayscale)
    {
        // Stores a variant of a given image.
        var key = GetNextKey(imageId);
        _grayscaleMatrixStore.Add(key, grayscale);
        return key;
    }

    public Matrix<Rgba> GetRgbaImage(ImageVariantKey imageVariantKey)
    {
        if (!_rgbaMatrixStore.TryGetValue(imageVariantKey, out var image))
            throw new Exception("Key does not exist for image variant");

        return image;
    }

    public Matrix<Grayscale> GetGrayscaleImage(ImageVariantKey imageVariantKey)
    {
        if (!_grayscaleMatrixStore.TryGetValue(imageVariantKey, out var image))
            throw new Exception("Key does not exist for image variant");

        return image;
    }

    private ImageVariantKey GetNextKey()
    {
        var imageId = _imageIdKey;
        _imageVariantIdStore.Add(imageId, 1);
        Interlocked.Increment(ref _imageIdKey);
        return new ImageVariantKey
        {
            ImageId = imageId,
            ImageVariantId = 0
        };
    }

    private ImageVariantKey GetNextKey(int imageId)
    {
        if (!_imageVariantIdStore.TryGetValue(imageId, out var value))
            throw new Exception("Cannot get the key for an image that doesn't already exist.");

        var ivk = new ImageVariantKey
        {
            ImageId = imageId,
            ImageVariantId = value
        };

        _imageVariantIdStore[imageId] += 1;
        return ivk;
    }
}