using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Duccsoft.Terry;

public class BotTerryDriver : TerryDriver
{
	[Property] public List<Vector3> CurrentPath { get; set; }
	[Property] public float TargetReachedDistance { get; set; } = 1f;

	protected override void OnUpdate()
	{
		var pathDirection = GetPathDirection();
		Controller.WishVelocity = Vector3.Forward;
		Controller.EyeAngles = new Angles( 0, Rotation.LookAt(pathDirection).Yaw(), 0 );
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
}
