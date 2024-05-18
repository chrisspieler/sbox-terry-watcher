public sealed class SkyController : Component
{
	[Property] public SkyBox2D Clouds { get; set; }
	protected override void OnStart()
	{
		if ( !Clouds.IsValid() )
			return;

		var yaw = Game.Random.Float( 0, 360 );
		Clouds.Transform.Rotation = new Angles( 0, yaw, 0 );
	}
}
