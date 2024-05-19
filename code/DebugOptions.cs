using Duccsoft.Terry;

public sealed class DebugOptions : GameObjectSystem
{
	public static TerryController PilotedTerry { get; set; }

	public DebugOptions( Scene scene ) : base( scene )
	{
		PilotedTerry = null;
		Listen( Stage.UpdateBones, 0, Update, "Debug Options" );
	}

	private void Update()
	{
		if ( Input.Pressed( "score" ) )
		{
			if ( PilotedTerry.IsValid() )
			{
				UnpilotTerry();
			}
			else
			{
				PilotTerry();
			}
		}
	}

	[ConCmd("terry_pilot")]
	public static void PilotTerry()
	{
		if ( Game.ActiveScene is null || !Game.IsPlaying )
			return;

		var spawners = Game.ActiveScene.GetAllComponents<TerrySpawner>();
		if ( !spawners.Any() )
			return;

		if ( PilotedTerry != null )
		{
			UnpilotTerry();
		}
		var randomSpawner = Game.Random.FromArray( spawners.ToArray() );
		PilotedTerry = randomSpawner.Spawn();
		var driver = PilotedTerry.Components.GetOrCreate<DirectTerryDriver>( FindMode.EverythingInSelf );
		driver.Enabled = true;
	}

	[ConCmd("terry_unpilot")]
	public static void UnpilotTerry()
	{
		if ( !PilotedTerry.IsValid() || !Game.IsPlaying )
			return;

		var directDriver = PilotedTerry.Components.Get<DirectTerryDriver>();
		if ( directDriver is not null )
		{
			directDriver.Enabled = false;
		}
		var botDriver = PilotedTerry.Components.GetOrCreate<BotTerryDriver>( FindMode.EverythingInSelf );
		botDriver.Enabled = true;
		PilotedTerry = null;
	}
}
