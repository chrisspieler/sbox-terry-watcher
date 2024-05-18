using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Duccsoft.Terry;

public class BotTerryDriver : TerryDriver
{
	[Property] public List<Vector3> CurrentPath { get; set; }
	[Property] public float TargetReachedDistance { get; set; } = 1f;
	[Property] public float LookLerpSpeed { get; set; } = 10f;

	protected override void OnUpdate()
	{
		var pathDirection = GetPathDirection();
		Controller.WishVelocity = Vector3.Forward;
		Controller.ShouldWalk = true;
		var targetYaw = Rotation.LookAt( pathDirection ).Yaw();
		var currentYaw = Controller.EyeAngles.yaw.LerpTo( targetYaw, Time.Delta * LookLerpSpeed );
		Controller.EyeAngles = new Angles( 0, currentYaw, 0 );
	}

	private Vector3 GetPathDirection()
	{
		while ( ReachedNextPosition() )
		{
			CurrentPath.RemoveAt( 0 );
		}

		if ( !CurrentPath.Any() )
			return Vector3.Zero;

		return (CurrentPath[0] - Transform.Position).Normal;
	}

	private bool ReachedNextPosition()
	{
		if ( !CurrentPath.Any() )
			return false;
		var distance = Transform.Position.Distance( CurrentPath.First() );
		return distance < TargetReachedDistance;
	}

	public void NavigateTo( Vector3 targetPosition )
	{
		CurrentPath = Scene.NavMesh.GetSimplePath( Transform.Position, targetPosition );
	}

	protected override void DrawGizmos()
	{
		Gizmo.Draw.Color = Color.Green;

		Gizmo.Draw.Arrow( Vector3.Zero, Controller.WishVelocity * 10f, 3, 1.5f );

		if ( !Gizmo.IsSelected )
			return;

		for ( int i = 0; i < CurrentPath.Count; i++ )
		{
			var pathPoint = Transform.World.PointToLocal( CurrentPath[i] );
			Gizmo.Draw.Color = Color.Green;
			Gizmo.Draw.LineSphere( pathPoint, 3f );
			Gizmo.Draw.Color = Color.Green * 0.9f;
			if ( i == 0 )
			{
				Gizmo.Draw.Line( pathPoint, Vector3.Zero );
			}
			else
			{
				var previousPoint = Transform.World.PointToLocal( CurrentPath[i - 1] );
				Gizmo.Draw.Line( pathPoint, previousPoint );
			}
		}
	}
}
