using ImageProcessing.Abstractions;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;
using Images.Abstractions.Pixels;
using LinearAlgebra;

namespace ImageProcessing.Pipelines.Items;

public class DeWarpItem : BaseItem
{
	private readonly Matrix<Uv> _distortionMatrix;

	public DeWarpItem(DeWarp deWarp) : base(new EmptyOptions())
	{
		_distortionMatrix = deWarp.GetDistortionMatrix();	// TODO create initialize function?
	}
	
	public override Task<BaseMessage> ProcessAsync(BaseMessage input, CancellationToken cancellationToken)
	{
		var castMessage = AsInputType(input);
		// TODO cast input and output?
		
		// TODO temporarily remove this part of the pipeline processing(?)
		// TODO kinda want to just have a builder that can build each step of the pipeline.
		return Task.FromResult<BaseMessage>(castMessage);
		// return Task.FromResult<BaseMessage>(new RgbaImage
		// 	{ Image = DeWarp.ApplyDistortionMat(castMessage.Image, _distortionMatrix) });
	}

	public override Type GetInputType() => typeof(RgbaImage);
	public override Type GetOutputType() => typeof(RgbaImage);
	private RgbaImage AsInputType(BaseMessage input)
	{
		var value = input as RgbaImage;
		if (input.GetType() != GetInputType() || value is null)
			throw new Exception("Failed to cast input message");
		return value;
	} 
}