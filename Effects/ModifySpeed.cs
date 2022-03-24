using Pandorai.Creatures;
using Pandorai.UI;

namespace Pandorai.Effects
{
    public class ModifySpeed : Effect
    {
        public int Amount;
        public int Duration;

        private string _modifier;

        public override void SetAttribute(string name, string value)
        {
            if (name == "Amount")
            {
                Amount = int.Parse(value);
                _modifier = Amount > 0 ? "increased" : "decreased";
            }
            else if (name == "Duration")
            {
                Duration = int.Parse(value);
            }
        }

        public override void Use(Creature usingCreature)
        {
            int turnCounter = 0;

            usingCreature.Stats.Speed += Amount;
            usingCreature.TurnEnded += checkDuration;

            void checkDuration()
            {
                turnCounter++;
                if (turnCounter >= Duration)
                {
                    usingCreature.Stats.Speed -= Amount;

                    usingCreature.TurnEnded -= checkDuration;

                    if(usingCreature.IsPossessedCreature())
                    {
                        MessageLog.DisplayMessage($"The {_modifier} speed effect wore off");
                    }
                }
            };
            
            DisplayMessage(usingCreature);
        }

		protected override string GetMessage()
        {
            if(Amount > 0)
            {
                return $"Your speed increased by \\c[green]{Amount}";
            }
            else
            {
                return $"Your speed decreased by \\c[red]{-Amount}";
            }
        }   
    }
}