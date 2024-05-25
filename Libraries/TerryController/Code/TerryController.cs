using Sandbox;

namespace Duccsoft.Terry;

[Group( "Terry" )]
[Title( "Terry Controller" )]
public sealed partial class TerryController : Component
{
	[Property] public CharacterController CharacterController { get; set; }
	[Property] public float KillPlaneLevel { get; set; } = -300f;
	[Property] public float CrouchMoveSpeed { get; set; } = 64.0f;
	[Property] public float WalkMoveSpeed { get; set; } = 190.0f;
	[Property] public float RunMoveSpeed { get; set; } = 190.0f;
	[Property] public float SprintMoveSpeed { get; set; } = 320.0f;

	[Property] public bool CustomEyeAngle { get; set; } = false;
	[Property, ShowIf( nameof( CustomEyeAngle ), true )]
	public Angles InitialEyeAngle { get; set; }

	[Property] public TerryAnimationHelper AnimationHelper { get; set; }

	[Property] public bool ShouldCrouch { get; set; }
	[Property] public bool ShouldWalk { get; set; }
	[Property] public bool ShouldRun { get; set; }
	[Property] public bool ShouldJump { get; set; }

	[Sync] public bool Crouching { get; set; }
	[Sync] public Angles EyeAngles 
	{
		get => _eyeAngles; 
		set
		{
			var angles = value;
			angles.pitch = angles.pitch.Clamp( -90, 90 );
			angles.roll = 0f;
			_eyeAngles = angles;
		}
	}
	private Angles _eyeAngles;
	[Sync] public Vector3 WishVelocity { get; set; }

	public float EyeHeight = 64;

	protected override void OnStart()
	{
		EyeAngles = CustomEyeAngle
			? InitialEyeAngle
			: Transform.Rotation;
	}

	protected override void OnUpdate()
	{
		if ( Transform.Position.z < KillPlaneLevel )
		{
			GameObject.Destroy();
			return;
		}
		UpdateAnimation();

	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
			return;

		Transform.Rotation = new Angles( 0, EyeAngles.yaw, 0 );
		UpdateCrouch();
		UpdateMovement();
	}

	float CurrentMoveSpeed
	{
		get
		{
			if ( Crouching ) return CrouchMoveSpeed;
			if ( ShouldRun ) return SprintMoveSpeed;
			if ( ShouldWalk ) return WalkMoveSpeed;

			return RunMoveSpeed;
		}
	}

	public RealTimeSince LastGrounded { get; private set; }
	public RealTimeSince LastUngrounded { get; private set; }
	public RealTimeSince LastJump { get; private set; }

	float GetFriction()
	{
		if ( CharacterController.IsOnGround ) return 6.0f;

		// air friction
		return 0.2f;
	}

	private void UpdateMovement()
	{
		if ( CharacterController is null )
			return;

		var cc = CharacterController;

		Vector3 halfGravity = Scene.PhysicsWorld.Gravity * Time.Delta * 0.5f;

		if ( LastGrounded < 0.2f && LastJump > 0.3f && ShouldJump )
		{
			ShouldJump = false;
			LastJump = 0;
			cc.Punch( Vector3.Up * 300 );
		}

		if ( !WishVelocity.IsNearlyZero() )
		{
			WishVelocity = new Angles( 0, EyeAngles.yaw, 0 ).ToRotation() * WishVelocity;
			WishVelocity = WishVelocity.WithZ( 0 );
			WishVelocity = WishVelocity.ClampLength( 1 );
			WishVelocity *= CurrentMoveSpeed;

			if ( !cc.IsOnGround )
			{
				WishVelocity = WishVelocity.ClampLength( 50 );
			}
		}


		cc.ApplyFriction( GetFriction() );

		if ( cc.IsOnGround )
		{
			cc.Accelerate( WishVelocity );
			cc.Velocity = CharacterController.Velocity.WithZ( 0 );
		}
		else
		{
			cc.Velocity += halfGravity;
			cc.Accelerate( WishVelocity );

		}

		//
		// Don't walk through other players, let them push you out of the way
		//
		var pushVelocity = TerryPusher.GetPushVector( Transform.Position + Vector3.Up * 40.0f, Scene, GameObject );
		if ( !pushVelocity.IsNearlyZero() )
		{
			var travelDot = cc.Velocity.Dot( pushVelocity.Normal );
			if ( travelDot < 0 )
			{
				cc.Velocity -= pushVelocity.Normal * travelDot * 0.6f;
			}

			cc.Velocity += pushVelocity * 128.0f;
		}

		cc.Move();

		if ( !cc.IsOnGround )
		{
			cc.Velocity += halfGravity;
		}
		else
		{
			cc.Velocity = cc.Velocity.WithZ( 0 );
		}

		if ( cc.IsOnGround )
		{
			LastGrounded = 0;
		}
		else
		{
			LastUngrounded = 0;
		}
	}
	float DuckHeight = (64 - 36);

	bool CanUncrouch()
	{
		if ( !Crouching ) return true;
		if ( LastUngrounded < 0.2f ) return false;

		var tr = CharacterController.TraceDirection( Vector3.Up * DuckHeight );
		return !tr.Hit; // hit nothing - we can!
	}

	public void UpdateCrouch()
	{
		if ( ShouldCrouch == Crouching )
			return;

		// crouch
		if ( ShouldCrouch )
		{
			CharacterController.Height = 36;
			Crouching = ShouldCrouch;

			// if we're not on the ground, slide up our bbox so when we crouch
			// the bottom shrinks, instead of the top, which will mean we can reach
			// places by crouch jumping that we couldn't.
			if ( !CharacterController.IsOnGround )
			{
				CharacterController.MoveTo( Transform.Position += Vector3.Up * DuckHeight, false );
				Transform.ClearInterpolation();
				EyeHeight -= DuckHeight;
			}

			return;
		}

		// uncrouch
		if ( !ShouldCrouch )
		{
			if ( !CanUncrouch() ) return;

			CharacterController.Height = 64;
			Crouching = ShouldCrouch;
			return;
		}
	}

	private void UpdateAnimation()
	{
		if ( AnimationHelper is null ) return;

		var wv = WishVelocity.Length;

		AnimationHelper.WithWishVelocity( WishVelocity );
		AnimationHelper.WithVelocity( CharacterController.Velocity );
		AnimationHelper.IsGrounded = CharacterController.IsOnGround;
		AnimationHelper.DuckLevel = Crouching ? 1.0f : 0.0f;

		AnimationHelper.MoveStyle = TerryAnimationHelper.MoveStyles.Walk;

		var lookDir = EyeAngles.ToRotation().Forward * 1024;
		AnimationHelper.WithLook( lookDir, 1, 0.5f, 0.25f );
	}

	public void SetBodyVisibility( bool visible )
	{
		if ( AnimationHelper is null )
			return;

		var renderMode = visible
			? ModelRenderer.ShadowRenderType.On
			: ModelRenderer.ShadowRenderType.ShadowsOnly;

		AnimationHelper.Target.RenderType = renderMode;

		foreach ( var clothing in AnimationHelper.Target.Components.GetAll<ModelRenderer>( FindMode.InChildren ) )
		{
			if ( !clothing.Tags.Has( "clothing" ) )
				continue;

			clothing.RenderType = renderMode;
		}
	}

}
