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
			Main.Game.Camera.CameraShake = new Shake(1000, 120, 25, Main.Game.MainRng);
			Main.Game.Camera.ShakeCamera();
			Console.WriteLine("leave");
			CreatureLeftLibrary?.Invoke(incomingCreature);
		}
	}
}
