using Pandorai.Rendering;

namespace Pandorai.Creatures.Behaviours
{
    public class Awakening : Behaviour
    {
        public bool IsAwake { get; set; }

        public void Awake(Creature incomingCreature)
        {
            if(IsAwake)
            {
                return;
            }

            IsAwake = true;
            Game1.game.Camera.CameraShake = new Shake(1000, 120, 25, Game1.game.mainRng);
			Game1.game.Camera.ShakeCamera();
            EnableVision();

            var hpBarTimer = new System.Timers.Timer(1000);
			hpBarTimer.Elapsed += (o, e) =>
			{
                owner.ShowHPBar = true;
                hpBarTimer.Stop();
                hpBarTimer.Dispose();
            };
            hpBarTimer.Enabled = true;
        }

        public void Sleep()
        {
            if(!IsAwake)
            {
                return;
            }

            IsAwake = false;
            owner.ShowHPBar = false;
            DisableVision();
        }

        public override void Bind()
        {
            owner.ShowHPBar = IsAwake;
            owner.GotHit += Awake;
            if(!IsAwake)
            {
                DisableVision();
            }
        }

        private void DisableVision()
        {
            var vision = owner.GetBehaviour<Vision>() as Vision;
            vision?.Disable();
        }

        private void EnableVision()
        {
            var vision = owner.GetBehaviour<Vision>() as Vision;
            vision?.Enable();
        }

        public override Behaviour Clone()
        {
            return new Awakening
            {
                IsAwake = IsAwake,
            };
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "IsAwake")
            {
                IsAwake = bool.Parse(value);
            }
        }
    }
}