using System;
using Duccsoft.Terry;

public sealed class TerrySpawner : Component
{
	[Property] public Action<TerryController> OnTerrySpawned { get; set; }

	[Property] public GameObject Prefab { get; set; }
	[Property] public float TickDelay { get; set; } = 1f;
	[Property] public int Rarity { get; set; } = 5;

	private TimeUntil _untilNextTick;

	protected override void OnStart()
	{
		_untilNextTick = TickDelay;
	}

	[Button("Spawn Terry")]
	public TerryController Spawn()
	{
		var terry = Prefab.Clone( Transform.World.WithScale( 1f ) );
		terry.BreakFromPrefab();

		var controller = terry.Components.Get<TerryController>();

		var exit = GetExit();
		if ( exit is null )
			return controller;

		var botDriver = terry.Components.GetOrCreate<BotTerryDriver>();
		botDriver.NavigateTo( exit.Transform.Position );

		return controller;
	}

	[Button("Roll Spawn")]
	public void RollSpawn()
	{
		var roll = Game.Random.Int( 0, Rarity );
		if ( roll == 0 )
		{
			Spawn();
		}
	}

	protected override void OnUpdate()
	{
		if ( _untilNextTick )
		{
			RollSpawn();
			_untilNextTick = TickDelay;
		}
	}

	private TerryDestroyer GetExit()
	{
		var exits = Scene.GetAllComponents<TerryDestroyer>();
		if ( !exits.Any() )
			return null;

		if ( exits.Count() == 1 )
			return exits.First();

		var nearest = exits
			.OrderBy( d => d.Transform.Position.Distance( Transform.Position ) )
			.First();
		exits = exits.Except( new[] { nearest } );
		return Game.Random.FromArray( exits.ToArray() );
	}

	protected override void DrawGizmos()
	{
		var mins = new Vector3( -16f, -16f, 0f );
		var maxs = new Vector3( 16f, 16f, 72f );
		var bounds = new BBox( mins, maxs );
		Gizmo.Hitbox.BBox( bounds );
		Gizmo.Draw.Color = Gizmo.IsHovered || Gizmo.IsSelected
			? Color.White
			: Color.Gray;
		Gizmo.Draw.LineBBox( bounds );
		Gizmo.Draw.Color = Color.Green.WithAlpha( 0.75f );
		Gizmo.Draw.Model( "models/citizen/citizen.vmdl" );
	}
}
