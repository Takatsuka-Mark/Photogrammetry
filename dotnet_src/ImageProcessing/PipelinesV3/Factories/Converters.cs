using System.Threading.Tasks.Dataflow;
using ImageProcessing.PipelinesV3.DTOs;
using Images.Abstractions.Pixels;
using LinearAlgebra;
using PhotogrammetryStore;

namespace ImageProcessing.PipelinesV3.Factories;

public static class Converters
{
	public static TransformBlock<MetadataStoreRecord, MetadataStoreRecord> GetGrayscaleConverterTransformBlock(
		MetadataStore metadataStore)
	{
		// TODO not the biggest fan of having the metadata store passed here.
		return new TransformBlock<MetadataStoreRecord, MetadataStoreRecord>(record =>
		{
			// TODO variant selection. See DeWarp for full comment.
			var matrix = metadataStore.FetchDeWarpedRgba64(record.RecordGuid);
			var result = (Matrix<Grayscale>)matrix.Convert((_, rgba64) => Grayscale.FromRgba64(rgba64));
			metadataStore.StoreDeWarpedGrayscale(record.RecordGuid, result);
			return record;
		});
	}
}
