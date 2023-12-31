﻿
using Sandbox;
using System;
using System.Linq;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace PenguinsArena;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.GameManager
{
	/// <summary>
	/// Called when the game is created (on both the server and client)
	/// </summary>
	public MyGame()
	{
		if ( Game.IsClient )
		{
			Game.RootPanel = new Hud();
		}
	}

	public override void PostLevelLoaded()
	{
		//base.PostLevelLoaded();
		var spawnpoints = Entity.All.OfType<SpawnPoint>().ToList(); //convert to list so collection never changes
		foreach ( SpawnPoint spawnpoint in spawnpoints )
		{
			var pawn = new Pawn();
			var tx = spawnpoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 50.0f; // raise it up
			pawn.Transform = tx;

			pawn.Respawn();
		}
	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );
	}

	[GameEvent.Tick.Server]
	public void OnTick()
	{
		var allPawns = Entity.All.OfType<Pawn>().ToList(); //to fix the IEnumerable changing and interrupting the ForEach.
		foreach ( Pawn p in allPawns )
		{
			if ( p.Client == null )
			{
				p.SimulateBot();
			}
		}
	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		// Get all of the pawns
		var pawns = Entity.All.OfType<Pawn>();

		// chose a random one
		var randomPawn = pawns.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		client.Pawn = randomPawn;

		randomPawn.ActiveWeapon.CreateViewModel();

	}
}

