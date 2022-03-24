using Pandorai.Creatures;
using Pandorai.Rendering;
using Pandorai.Tilemaps;
using System;

namespace Pandorai.Triggers
{
	public static partial class Trigger
	{
		public static event CreatureIncomingHandler CreatureLeftLibrary;

		public static void LeaveLibraryTrigger(Creature incomingCreature)
		{
			incomingCreature.Game.Camera.CameraShake = new Shake(1000, 120, 25, incomingCreature.Game.MainRng);
			incomingCreature.Game.Camera.ShakeCamera();
			Console.WriteLine("leave");
			CreatureLeftLibrary?.Invoke(incomingCreature);
		}
	}
}
