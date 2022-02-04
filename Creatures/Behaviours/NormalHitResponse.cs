namespace Pandorai.Creatures.Behaviours
{
	public class NormalHitResponse : Behaviour
	{
		public override void Bind()
		{
			Owner.GotHit += TakeDamage;
		}

		private void TakeDamage(Creature incomingCreature)
		{
			Owner.GetHit(incomingCreature.MeleeHitDamage);
		}

		public override Behaviour Clone()
		{
			return new NormalHitResponse();
		}

		public override void SetAttribute(string name, string value)
		{
			
		}
	}
}
