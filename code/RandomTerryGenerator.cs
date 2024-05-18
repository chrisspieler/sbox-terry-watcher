using Duccsoft.Terry;

public sealed class RandomTerryGenerator : Component
{
	[Property] public SkinnedModelRenderer Renderer { get; set; }
	[Property] public BodyGeneratorConfig BodyConfig { get; set; }
	[Property] public OutfitGeneratorConfig OutfitConfig { get; set; }
	[Property] public bool RandomizeOnStart { get; set; } = true;

	protected override void OnStart()
	{
		if ( RandomizeOnStart )
		{
			Randomize();
		}
	}

	[Button("Randomize")]
	public void Randomize()
	{
		if ( !Renderer.IsValid() || !Game.IsPlaying )
			return;

		var data = TerryData.Generate( BodyConfig, OutfitConfig );
		data.Container.Apply( Renderer );
	}
}
