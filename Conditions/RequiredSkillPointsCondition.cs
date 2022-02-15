namespace Pandorai.Conditions
{
    public class RequiredSkillPointsCondition : Condition
    {
        public int Amount;

        public override bool Check()
        {
            bool conditionMet = Game1.game.Player.PossessedCreature.Stats.SkillPoints >= Amount;
            return conditionMet;
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "Amount")
            {
                Amount = int.Parse(value);
            }
        }
    }
}