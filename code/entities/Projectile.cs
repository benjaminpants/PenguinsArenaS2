using Sandbox;

namespace PenguinsArena;

public partial class Projectile : ModelEntity
{
	public string modelPath => myData == null ? null : myData.Model;

	public float speed => myData == null ? 0f : myData.Speed;

	public float radius => myData == null ? 4f : myData.Radius;

	public float knockScale => myData == null ? 0f : myData.Knockback;

	[Net]
	public ProjectileData myData { get; set; }

	public Pawn Pawn => Owner as Pawn;

	public Trace trace;

	public override void Spawn()
	{
		trace = new Trace();
		if ( modelPath != null )
		{
			SetModel( modelPath );
		}
		trace = trace.Radius(radius);
		EnableHitboxes = true;
	}

	public Projectile Load(ProjectileData data)
	{
		myData = data;
		Spawn();
		return this;
	}

	public virtual void OnHit(Pawn p)
	{
		p.Velocity += Velocity * knockScale;
		Sound.FromWorld( myData.HitSound, Position );
	}

	[GameEvent.Tick.Server]
	public void OnServerTick()
	{
		Vector3 spd = Rotation.Forward * speed;
		Velocity = spd;
		TraceResult tr = trace.FromTo( Position, Position + spd )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc" )
			.Ignore( Owner )
			.Run();
		
		//DebugOverlay.TraceResult( tr, 2f );

		if ( tr.Entity is Pawn )
		{
			OnHit( tr.Entity as Pawn );
		}
		if (tr.Hit)
		{
			Delete();
		}
		Position += Velocity;
	}
}
