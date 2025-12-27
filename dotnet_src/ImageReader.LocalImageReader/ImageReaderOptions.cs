using System.ComponentModel.DataAnnotations;
using Framework;
using Microsoft.Extensions.Configuration;

namespace ImageReader.LocalImageReader;

public sealed class ImageReaderOptions
{
    public const string Section = "ImageReader";

    [Required]
    public required string RootDirectory { get; init; }
    [Required]
    public required string RootOutputDirectory { get; init; }
}