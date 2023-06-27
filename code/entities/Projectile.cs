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

	public float TimeOfCreation;

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
		TimeOfCreation = Time.Now;
	}

	public Projectile Load(ProjectileData data)
	{
		myData = data;
		Spawn();
		return this;
	}

	public virtual void OnHit(Pawn p)
	{
		p.Pummel(Velocity * knockScale);
	}

	[GameEvent.Tick.Server]
	public void OnServerTick()
	{
		if ((TimeOfCreation + 10f) < Time.Now )
		{
			Delete();
			return;
		}
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
			Sound.FromWorld( myData.HitSound, Position );
			OnHit( tr.Entity as Pawn );
		}
		if (tr.Hit)
		{
			if (myData.Effect == HitEffect.Explode)
			{
				TraceResult[] results = trace.FromTo( Position, Position + spd )
					.Radius(myData.ExplosionRadius)
					.UseHitboxes()
					.WithAnyTags( "solid", "player", "npc" )
					.Ignore( tr.Entity ) //ignore the thing we directly hit
					.RunAll();
				if ( results != null )
				{
					foreach ( TraceResult t in results )
					{
						if ( t.Entity is Pawn )
						{
							Pawn p = t.Entity as Pawn;
							p.SetPummel( (-(tr.HitPosition - p.Position).Normal) * (knockScale * myData.IndirectMultiplier) );
						}
					}
				}
			}
			Delete();
		}
		Position += Velocity;
	}
}
