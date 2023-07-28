using Editor;
using Sandbox;
using System.Xml.Linq;

namespace PenguinsArena;

[HammerEntity]
public class Pickup : ModelEntity
{
	[Property( Title = "Weapon Data" )]
	public WeaponData weaponData { get; set; }

	[Property( Title = "Respawn Time" )]
	public float respawnTime { get; set; }

	public bool OnCooldown = false;
	public float RespawnAtTime = -1f;

	public override void Spawn()
	{
		base.Spawn();
		SetModel( weaponData.PickupModel );
		EnableDrawing = true;
		SetupPhysicsFromSphere(PhysicsMotionType.Keyframed, Vector3.Up * 5f,5f);
		EnableTouch = true;
		Tags.Clear();
		UsePhysicsCollision = true;
		EnableSolidCollisions = true;
		Tags.Add("trigger");
	}

	[GameEvent.Tick.Server]
	public void OnServerTick()
	{
		if ( OnCooldown && (RespawnAtTime != -1) )
		{
			if ( Time.Now >= RespawnAtTime )
			{
				EnableDrawing = true;
				OnCooldown = false;
				RespawnAtTime = -1f;
			}
		}
		Rotation = new Angles(new Vector3(0f, Time.Now * 150f, 0f)).ToRotation();
	}

	public void EnterCooldown()
	{
		if ( OnCooldown ) return;
		RespawnAtTime = Time.Now + respawnTime;
		OnCooldown = true;
		EnableDrawing = false;
	}

	public virtual void OnPickup(Pawn p)
	{
		if ( !Game.IsServer ) return;
		if ( OnCooldown ) return;
		EnterCooldown();
		var w = new StandardProjectileWeapon().LoadWeapon( weaponData );
		p.ForceWeapon(w);
	}

	public override void Touch( Entity other )
	{
		if ( other is Pawn )
		{
			OnPickup( other as Pawn );
		}
	}

}
