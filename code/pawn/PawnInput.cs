using Sandbox;
using System.Collections.Generic;

namespace PenguinsArena;

public class PawnInput : EntityComponent<Pawn>
{
	public Vector3 AnalogMove;
	public Angles AnalogLook;
	public bool JumpPressed;
	public bool FirePressed;

	public void UpdateBasedOnCurrentInput()
	{
		AnalogMove = Input.AnalogMove;
		AnalogLook = Input.AnalogLook;
		JumpPressed = Input.Down( "jump" );
	}
}
