using System;
using Duccsoft.Terry;

public sealed class TerryDestroyer : Component, Component.ITriggerListener
{
	[Property] public Action<TerryController> OnBeforeTerryDestroyed { get; set; }

	public void OnTriggerEnter(Collider other)
	{
		if ( !other.Tags.Has( "terry" ) || other.GameObject is null )
			return;

		var controller = other.GameObject.Components.GetInAncestorsOrSelf<TerryController>();
		if ( controller is null )
			return;

		OnBeforeTerryDestroyed?.Invoke( controller );
		controller.GameObject.Destroy();
	}
}
