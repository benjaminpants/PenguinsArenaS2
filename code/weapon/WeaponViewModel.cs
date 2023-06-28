using Sandbox;

namespace PenguinsArena;

public partial class WeaponViewModel : BaseViewModel
{
	protected Weapon Weapon { get; init; }

	public WeaponViewModel( Weapon weapon )
	{
		Weapon = weapon;
		EnableShadowCasting = false;
		EnableViewmodelRendering = true;
	}

	public override void PlaceViewmodel()
	{
		base.PlaceViewmodel();

		if (Weapon is StandardProjectileWeapon)
		{
			StandardProjectileWeapon spw = Weapon as StandardProjectileWeapon;
			EnableDrawing = spw.CanPrimaryAttack();
			Position += spw.myData.ViewPosition.RotateAround(Vector3.Zero,Rotation);
		}

		Camera.Main.SetViewModelCamera( 80f, 1, 500 );
	}
}
