public sealed class CameraAngle : Component
{
	[Property] public string Name { get; set; }
	[Property] public bool IsMain { get; set; }

	[ActionGraphNode( "CameraAngle.Reset")]
	[Title( "Reset Camera Angle"), Group("Camera")]
	public static void ResetCameraAngle()
	{
		var scene = Game.ActiveScene;
		if ( !scene.IsValid() || !scene.Camera.IsValid() )
			return;

		var camera = scene.GetAllComponents<CameraAngle>()
			.FirstOrDefault( c => c.IsMain );

		camera ??= scene.GetAllComponents<CameraAngle>()
				.FirstOrDefault();

		if ( camera is null )
			return;

		scene.Camera.Transform.World = camera.Transform.World;
	}
}
