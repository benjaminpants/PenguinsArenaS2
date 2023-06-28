using Editor;
using Sandbox;
using System.Xml.Linq;

namespace PenguinsArena;

[HammerEntity]
public class Pickup : ModelEntity
{
	[Property( Title = "Weapon Data" )]
	public WeaponData weaponData { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		SetModel( weaponData.PickupModel );
		EnableDrawing = true;
		//SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3(-6f,-6f,-6f), new Vector3( 6f, 6f, 6f ) );
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		EnableTouch = true;
		Tags.Clear();
		UsePhysicsCollision = true;
		EnableSolidCollisions = true;
		Tags.Add("trigger");
	}

	public virtual void OnPickup(Pawn p)
	{
		p.SetActiveWeapon(new StandardProjectileWeapon().LoadWeapon(weaponData) );
	}

	public override void Touch( Entity other )
	{
		Log.Info(other);
		if ( other is Pawn )
		{
			OnPickup( other as Pawn );
		}
	}

}
