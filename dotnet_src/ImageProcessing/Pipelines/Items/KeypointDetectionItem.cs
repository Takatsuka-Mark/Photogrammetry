using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;
using Images.Abstractions.Pixels;

namespace ImageProcessing.Pipelines.Items;

public class KeypointDetectionItem : BaseItem
{
	private readonly KeypointDetection _keypointDetection;

	public KeypointDetectionItem(KeypointDetection keypointDetection) : base(new EmptyOptions())
	{
		_keypointDetection = keypointDetection;
	}


	public override Task<BaseMessage> ProcessAsync(BaseMessage input, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public override Type GetInputType()
	{
		throw new NotImplementedException();
	}

	public override Type GetOutputType()
	{
		throw new NotImplementedException();
	}
	
	private GrayscaleImage AsInputType(BaseMessage input)
	{
		// TODO is there a way I can repeat doing this?
		var value = input as GrayscaleImage;
		if (input.GetType() != GetInputType() || value is null)
			throw new Exception("Failed to cast input message");
		return value;
	} 
}