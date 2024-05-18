using Sandbox;
using System.Linq;

namespace Duccsoft.Terry;

public abstract class TerryDriver : Component
{
	[Property] public TerryController Controller { get; set; }

	protected override void OnEnabled()
	{
		var others = Components.GetAll<TerryDriver>()
			.Where( c => c != this );

		foreach ( var other in others )
		{
			other.Enabled = false;
		}

		Controller ??= Components.Get<TerryController>();
	}
}
