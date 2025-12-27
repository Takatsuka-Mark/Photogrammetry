using ImageProcessing.Pipelines.Items;
using ImageProcessing.Pipelines.Messages;
using ImageProcessing.Pipelines.Options;

namespace ImageProcessing.Pipelines;

public class DeWarpSequentialPipeline : BaseSequentialPipeline<RgbaImage, RgbaImage>
{
	public DeWarpSequentialPipeline()
	{
	}
	
	public override void ValidatePipeline()
	{
		throw new NotImplementedException();
	}

	public override Task RunPipelineAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
	
	public class DeWarpSequentialPipelineBuilder : Builder<DeWarpSequentialPipeline>
	{
		private readonly DeWarpItem _deWarpItem;

		public DeWarpSequentialPipelineBuilder(DeWarpItem deWarpItem): base(new SequentialPipelineOptions())
		{
			_deWarpItem = deWarpItem;
		}

		public DeWarpSequentialPipeline BuildPipeline()
		{
			return AddItem(_deWarpItem).Build();
		}
	}
}