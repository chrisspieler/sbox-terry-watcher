using System;

public sealed class TerrySpawner : Component
{
	[Property] public Action<TerryController> OnTerrySpawned { get; set; }

	private void Spawn()
	{
		
	}

	protected override void DrawGizmos()
	{
		var mins = new Vector3( -16f, -16f, 0f );
		var maxs = new Vector3( 16f, 16f, 72f );
		var bounds = new BBox( mins, maxs );
		Gizmo.Hitbox.BBox( bounds );
		Gizmo.Draw.Color = Gizmo.IsHovered || Gizmo.IsSelected
			? Color.White
			: Color.Gray;
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.Color = Color.Green.WithAlpha( 0.75f );
		Gizmo.Draw.Model( "models/citizen/citizen.vmdl" );
	}
}
