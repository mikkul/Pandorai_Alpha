using Microsoft.Xna.Framework;
using Pandorai.Creatures;
using Pandorai.Items;
using Pandorai.ParticleSystems;
using Pandorai.Sounds;
using Pandorai.UI;
using Pandorai.Utility;
using System.Timers;

namespace Pandorai.Structures.Behaviours
{
	public class PandoraiPedestal : Behaviour
	{
		public bool Activated { get; set; }

		public override void Bind()
		{
			Structure.Interacted += Interact;
			Structure.Tile.Tile.ItemUsed += ItemInteract;
			Structure.DisableBehaviour("LightEmitter");
		}

		public override void Unbind()
		{
			Structure.Interacted -= Interact;
			Structure.Tile.Tile.ItemUsed -= ItemInteract;
		}

		public override Behaviour Clone()
		{
			return new PandoraiPedestal()
			{
				
			};
		}

		public override void ForceHandler(ForceType force)
		{
		}

		public override void Interact(Creature creature)
		{
			if(!Activated)
			{
				return;
			}

            creature.Stats.Health = creature.Stats.MaxHealth;
            creature.Stats.Mana = creature.Stats.MaxMana;

			DoFancyEffectsAndJingle();
		}

		public override void SetAttribute(string name, string value)
		{
		}

		public void ItemInteract(Item item)
        {
            if (Activated)
            {
                return;
            }

            if (item.TemplateName != "PandoraiGem")
            {
                return;
            }

            Main.Game.Player.PossessedCreature.Stats.Experience += 1500;

            Main.Game.Player.PossessedCreature.Inventory.RemoveElement(item);
            Structure.EnableBehaviour("LightEmitter");

            DoFancyEffectsAndJingle();

            Main.Game.Player.PossessedCreature.Inventory.AddElement(ItemLoader.GetItem("MassDestructionRune"));

            MessageLog.DisplayMessage("You restored the altar of Pandorai!");
            MessageLog.DisplayMessage("You win!");
        }

        private void DoFancyEffectsAndJingle()
        {
            Main.Game.Camera.CameraShake = new Rendering.Shake(5000, 60, 50, Main.Game.MainRng);
            Main.Game.Camera.ShakeCamera();

            var position = new Vector2(Structure.Tile.Index.X * Main.Game.Map.TileSize, Structure.Tile.Index.Y * Main.Game.Map.TileSize);
            int rangeCovered = 0;
            int time = 5000;
            var range = 15;
            Timer effectTimer = new Timer(100);
            effectTimer.Elapsed += (s, a) =>
            {
                PSExplosion wavePS;
                if (rangeCovered % 2 == 0)
                {
                    wavePS = new PSExplosion(position, 150, Main.Game.smokeParticleTexture, time * 1.4f, 64 * range, 100, Helper.GetColorFromHex("#3f6aeb"), true);
                }
                else
                {
                    wavePS = new PSExplosion(position, 150, Main.Game.smokeParticleTexture, time * 1.4f, 64 * range, 85, Helper.GetColorFromHex("#819ae6"), true);
                }

                ParticleSystemManager.AddSystem(wavePS, true);

                if (++rangeCovered > range)
                {
                    effectTimer.Stop();
                    effectTimer.Dispose();
                    return;
                }
            };
            effectTimer.Start();

			SoundManager.StopMusic();
            SoundManager.PlaySound("FX150");
            SoundManager.PlaySound("Jingle_Win_00");
        }
    }
}
