using Sandbox;
using System.Linq;

public sealed class DirectTerryController : Component
{
	[ConVar( "camera_smoothing" )]
	public static float CameraSmoothing { get; set; } = 0f;

	[Property] public TerryController Controller { get; set; }
	[Property] public bool UsePrefererredFov { get; set; } = true;

	protected override void OnEnabled()
	{
		Controller ??= Components.Get<TerryController>();
	}

	protected override void OnUpdate()
	{
		Controller.WishVelocity = Input.AnalogMove;
		Controller.ShouldCrouch = Input.Down( "duck" );
		Controller.ShouldJump = Input.Down( "jump" );
		Controller.ShouldWalk = Input.Down( "walk" );
		Controller.ShouldRun = Input.Down( "run" );
	}

	protected override void OnPreRender()
	{
		UpdateCamera();
	}

	private void UpdateCamera()
	{
		var camera = Scene.GetAllComponents<CameraComponent>().Where( x => x.IsMainCamera ).FirstOrDefault();
		if ( camera is null ) return;

		var targetEyeHeight = Controller.Crouching ? 18 : 42;
		Controller.EyeHeight = Controller.EyeHeight.LerpTo( targetEyeHeight, RealTime.Delta * 10.0f );

		var eyePos = Transform.Position + new Vector3( 0, 0, Controller.EyeHeight );
		var targetCameraPos = eyePos + ((Rotation)Controller.EyeAngles).Backward * 100f;

		// smooth view z, so when going up and down stairs or ducking, it's smooth af
		if ( Controller.LastUngrounded > 0.2f )
		{
			targetCameraPos.z = camera.Transform.Position.z.LerpTo( targetCameraPos.z, RealTime.Delta * 25.0f );
		}

		camera.Transform.Position = targetCameraPos;

		Controller.EyeAngles += Input.AnalogLook;
		if ( CameraSmoothing == 0f )
		{
			camera.Transform.Rotation = Controller.EyeAngles;
		}
		else
		{
			var smoothSpeed = CameraSmoothing.Remap( 0, 100, 30, 5 );
			camera.Transform.Rotation = Rotation.Lerp( camera.Transform.Rotation, Controller.EyeAngles, Time.Delta * smoothSpeed );
		}
		if ( UsePrefererredFov )
		{
			camera.FieldOfView = Preferences.FieldOfView;
		}
	}
}
