using Sandbox;
using System.Collections.Generic;

namespace PenguinsArena;

public enum ProjectileTrajectory
{
	Straight,
	Grenade
}

public enum HitEffect
{
	Shove,
	Explode
}

[GameResource( "Projectile Definition", "proj", "Describes a basic projectile's properties.", Icon = "sign_language" )]
public partial class ProjectileData : GameResource
{
	//public static Dictionary<string, ProjectileData> All = new Dictionary<string, ProjectileData>();

	public string Name { get; set; }

	[ResourceType( "vmdl" )]
	public string Model { get; set; }

	[ResourceType("sound")]
	[Description( "The sound that this projectile will make when it hits something" )]
	public string HitSound { get; set; }

	[Category( "Physics" )]
	[Description("The radius of the projectile, used to detect for collisions.")]
	public float Radius { get; set; }
	[Category("Physics")]
	[Description( "The speed of the projectile in units per tick." )]
	public float Speed { get; set; }
	[Category("Physics")]
	[Description( "The trajectory that this projectile will travel.\nStraight moves in a straight line.\nGrenade will arc and bounce off of any surface it hits until it stops." )]
	public ProjectileTrajectory Trajectory { get; set; }

	[Category( "Effect" )]
	[Description( "The effect applied on hit." )]
	public HitEffect Effect { get; set; }
	[Category( "Effect" )]
	[Description( "The knockback applied to the target upon contact or explosion" )]
	public float Knockback { get; set; }

}


[GameResource( "Weapon Definition", "wep", "Describes a weapon's properties", Icon = "workspaces" )]
public partial class WeaponData : GameResource
{
	public string Name { get; set; }

	[ResourceType( "vmdl" )]
	public string ViewModel { get; set; }

	[Description("The position offset for the viewmodel")]
	public Vector3 ViewPosition { get; set; }

	[ResourceType( "vmdl" )]
	public string PickupModel { get; set; }

	[ResourceType( "proj" )]
	[Description( "The projectile to be thrown by this weapon." )]
	public string Projectile { get; set; }

	[Description("The time between shots")]
	public float Delay { get; set; }

	[Description( "How many shots this weapon can fire before its destroyed. -1 is infinite." )]
	public float Uses { get; set; }

}
