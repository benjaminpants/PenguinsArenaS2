using Sandbox;
using Sandbox.Utility;
using System;
using System.ComponentModel;
using System.Xml.Linq;

namespace PenguinsArena;

public partial class Pawn : AnimatedEntity
{
	[Net, Predicted]
	public Weapon ActiveWeapon { get; set; }

	[ClientInput]
	public Vector3 InputDirection { get; set; }
	
	[ClientInput]
	public Angles ViewAngles { get; set; }

	/// <summary>
	/// Position a player should be looking from in world space.
	/// </summary>
	[Browsable( false )]
	public Vector3 EyePosition
	{
		get => Transform.PointToWorld( EyeLocalPosition );
		set => EyeLocalPosition = Transform.PointToLocal( value );
	}

	/// <summary>
	/// Position a player should be looking from in local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Vector3 EyeLocalPosition { get; set; }

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity.
	/// </summary>
	[Browsable( false )]
	public Rotation EyeRotation
	{
		get => Transform.RotationToWorld( EyeLocalRotation );
		set => EyeLocalRotation = Transform.RotationToLocal( value );
	}

	/// <summary>
	/// Rotation of the entity's "eyes", i.e. rotation for the camera when this entity is used as the view entity. In local to the entity coordinates.
	/// </summary>
	[Net, Predicted, Browsable( false )]
	public Rotation EyeLocalRotation { get; set; }

	public BBox Hull
	{
		get => new
		(
			new Vector3( -16, -16, 0 ),
			new Vector3( 16, 16, 44 )
		);
	}

	[BindComponent] public PawnController Controller { get; }
	[BindComponent] public PawnAnimator Animator { get; }

	public override Ray AimRay => new Ray( EyePosition, EyeRotation.Forward );

	/// <summary>
	/// Called when the entity is first created 
	/// </summary>
	public override void Spawn()
	{
		SetModel( "models/player/penguin.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		SetupPhysicsFromAABB(PhysicsMotionType.Keyframed,Hull.Mins,Hull.Maxs);

		EnableHitboxes = true;

		Tags.Add("player", "solid");
	}

	public bool SetPummel( Vector3 velocity )
	{
		Velocity = velocity;
		return true;
	}

	public bool Pummel(Vector3 velocity)
	{
		Velocity += velocity;
		return true;
	}

	public void SetActiveWeapon( Weapon weapon )
	{
		ActiveWeapon?.OnHolster();
		ActiveWeapon = weapon;
		ActiveWeapon.OnEquip( this );
	}

	public void Respawn()
	{
		Components.RemoveAny<PawnController>();
		Components.Create<PawnController>();
		//Components.Create<PawnAnimator>();

		var w = new StandardProjectileWeapon().LoadWeapon( ResourceLibrary.Get<WeaponData>( "resources/fish.wep" ) );

		SetActiveWeapon( w );
	}

	public void EquiptStandardWeapon()
	{
		var w = new StandardProjectileWeapon().LoadWeapon( ResourceLibrary.Get<WeaponData>( "resources/snowball.wep" ) );

		SetActiveWeapon( w );
	}

	public void DressFromClient( IClient cl )
	{
		var c = new ClothingContainer();
		c.LoadFromClient( cl );
		c.DressEntity( this );
	}

	public override void Simulate( IClient cl )
	{
		SimulateRotation();
		Controller?.Simulate( cl );
		Animator?.Simulate();
		ActiveWeapon?.Simulate( cl );
		EyeLocalPosition = Vector3.Up * (38f * Scale);
	}

	public void SimulateBot()
	{
		float perl = Noise.Perlin( (Time.Now + this.NetworkIdent) * 10f);
		perl -= 0.5f;
		//Log.Info(perl);
		BuildInputIsolated(Vector3.Zero, new Angles( 0, (perl * 3000f) * Time.Delta, 0 ), false);
		if ( Random.Shared.NextDouble() >= 0.9 )
		{
			ActiveWeapon?.AttemptFire();
		}
		SimulateRotation();
		Controller?.Movement();
		if ( Random.Shared.NextDouble() >= 0.96 )
		{
			Controller?.DoJump();
		}
		Animator?.Simulate();
		EyeLocalPosition = Vector3.Up * (38f * Scale);
	}

	public void BuildInputIsolated(Vector3 analogMove, Angles analogLook, bool stopProcessing)
	{
		InputDirection = analogMove;

		if ( stopProcessing )
			return;

		var look = analogLook;

		if ( ViewAngles.pitch > 90f || ViewAngles.pitch < -90f )
		{
			look = look.WithYaw( look.yaw * -1f );
		}

		var viewAngles = ViewAngles;
		viewAngles += look;
		viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
		viewAngles.roll = 0f;
		ViewAngles = viewAngles.Normal;
	}

	public override void BuildInput()
	{
		BuildInputIsolated(Input.AnalogMove,Input.AnalogLook, Input.StopProcessing);
	}

	bool IsThirdPerson { get; set; } = false;

	public override void FrameSimulate( IClient cl )
	{
		SimulateRotation();

		Camera.Rotation = ViewAngles.ToRotation();
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( Game.Preferences.FieldOfView );

		if ( Input.Pressed( "view" ) )
		{
			IsThirdPerson = !IsThirdPerson;
		}

		if ( IsThirdPerson )
		{
			Vector3 targetPos;
			var pos = Position + Vector3.Up * 64;
			var rot = Camera.Rotation * Rotation.FromAxis( Vector3.Up, -16 );

			float distance = 80.0f * Scale;
			targetPos = pos + rot.Right * ((CollisionBounds.Mins.x + 50) * Scale);
			targetPos += rot.Forward * -distance;

			var tr = Trace.Ray( pos, targetPos )
				.WithAnyTags( "solid" )
				.Ignore( this )
				.Radius( 8 )
				.Run();
			
			Camera.FirstPersonViewer = null;
			Camera.Position = tr.EndPosition;
		}
		else
		{
			Camera.FirstPersonViewer = this;
			Camera.Position = EyePosition;
		}
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		return TraceBBox( start, end, Hull.Mins, Hull.Maxs, liftFeet );
	}

	public TraceResult TraceBBox( Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float liftFeet = 0.0f )
	{
		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets" )
					.Ignore( this )
					.Run();

		return tr;
	}

	protected void SimulateRotation()
	{
		EyeRotation = ViewAngles.ToRotation();
		Rotation = ViewAngles.WithPitch( 0f ).ToRotation();
	}
}
