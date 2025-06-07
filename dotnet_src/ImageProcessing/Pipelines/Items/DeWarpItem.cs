using ImageProcessing.Abstractions;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines.Items;

public class DeWarpItem : BaseItem<RgbaImage, RgbaImage>
{
	private readonly DistortionMatrix _distortionMatrix;

	public DeWarpItem(DeWarp deWarp) : base(new EmptyOptions())
	{
		_distortionMatrix = deWarp.GetDistortionMatrix();	// TODO create initialize function?
	}
	
	public override Task<RgbaImage> ProcessAsync(RgbaImage input, CancellationToken cancellationToken)
	{
		return Task.FromResult(new RgbaImage { Image = DeWarp.ApplyDistortionMat(input.Image, _distortionMatrix) });
	}
}