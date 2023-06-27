using Sandbox;

namespace PenguinsArena;

public partial class StandardProjectileWeapon : Weapon
{
	//has to be networked so viewmodels
	[Net]
	public WeaponData myData { get; set; }
	public override string ModelPath => myData == null ? null : myData.ViewModel;
	public override string ViewModelPath => myData == null ? null : myData.ViewModel;

	public int MaxUses => myData == null ? -1 : myData.Uses;

	public int TimesUsed = 0;

	public int Uses => MaxUses - TimesUsed;

	public override float PrimaryRate => myData == null ? 1f : myData.Delay;

	public StandardProjectileWeapon LoadWeapon(WeaponData data)
	{
		myData = data;
		Spawn();
		return this;
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		Game.AssertClient();

		//Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		Pawn.SetAnimParameter( "b_attack", true );
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void PrimaryAttack()
	{
		//ShootEffects();
		Pawn.PlaySound( "sounds/snowball_fire.sound" );
		ShootBullet( 0.1f, 100, 20, 1 );
		if ( !Game.IsServer ) return;
		if (MaxUses != -1)
		{
			TimesUsed++;
			if (Uses == 0)
			{
				Pawn.EquiptStandardWeapon();
				Delete();
			}
		}
	}

	public override void ShootBullet( Vector3 pos, Vector3 dir, float spread, float force, float damage, float bulletSize )
	{
		if ( !Game.IsServer ) return;
		Projectile sb = new Projectile();
		sb.Load(ResourceLibrary.Get<ProjectileData>(myData.Projectile));
		sb.Owner = Owner;
		sb.Position = pos;
		sb.Position += Vector3.Down * 5f;
		sb.Rotation = Rotation.LookAt(dir,sb.Rotation.Up);
	}

	protected override void Animate()
	{
		Pawn.SetAnimParameter( "holdtype", (int)CitizenAnimationHelper.HoldTypes.Pistol );
	}
}
