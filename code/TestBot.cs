using Sandbox;

namespace PenguinsArena;

public class PenguinBot : Bot
{
	[ConCmd.Admin( "penguin_bot", Help = "Spawn omega epic penguin bot" )]
	internal static void SpawnCustomBot()
	{
		Game.AssertServer();

		// Create an instance of your custom bot.
		_ = new PenguinBot();
	}

	public override void BuildInput()
	{
		// Here we can choose / modify the bot's input each tick.
		// We'll make them constantly attack by holding down the PrimaryAttack button.
		Input.SetAction("attack1",true);
		// And here, we'll make the bot walk forward and turn in a wide circle.
		//Input.AnalogMove = Vector3.Forward;
		//Input.AnalogLook = new Angles( 0, 10 * Time.Delta, 0 );
		// Finally, we'll call BuildInput on the bot's client's pawn. 
		// Note that Entity.BuildInput is NOT automatically called for the pawns of
		// simulated clients that are driven by bots, so that's why we call it here.
		(Client.Pawn as Entity).BuildInput();
	}

	public override void Tick()
	{
		// Here we can do something with the bot each tick.
		// Here we'll print our bot's name every tick.
		//Log.Info( Client.Name );
	}
}
