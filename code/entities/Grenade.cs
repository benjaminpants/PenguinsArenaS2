using Sandbox;
using System.Collections.Generic;

namespace PenguinsArena;

public class Grenade : Projectile
{

	public bool HasHitOnce = false;
	public override void Spawn()
	{
		base.Spawn();
	}

	public override void SetRotation( Rotation r )
	{
		base.SetRotation( r );
		Velocity = Rotation.Forward * speed;
	}

	public override bool OnHit( Pawn p )
	{
		return false;
	}

	public override bool OnHit( TraceResult tr, Vector3 spd )
	{
		if (HasHitOnce)
		{
			return base.OnHit( tr, spd );
		}
		Rotation = Rotation.LookAt( tr.Normal,Vector3.Zero);
		Position = tr.HitPosition + (Rotation.Forward * (radius * 4f));
		Velocity = Rotation.Forward * (speed / 2f);
		HasHitOnce = true;
		return false;
	}

	public override Vector3 CalculateVelocity()
	{
		Velocity += (Vector3.Down * 7f) * Time.Delta;
		return Velocity;
	}
}
