public sealed class Rotator : Component
{
	[Property] public Angles RotationPerSecond { get; set; }

	protected override void OnFixedUpdate()
	{
		Transform.Rotation *= RotationPerSecond * Time.Delta;
	}
}
