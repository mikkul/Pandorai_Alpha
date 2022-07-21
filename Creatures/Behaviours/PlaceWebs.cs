using System.Collections.Generic;
using System.Linq;
using Pandorai.Tilemaps;

namespace Pandorai.Creatures.Behaviours
{
    public class PlaceWebs : Behaviour
    {
        class Web
        {
            public Tile Tile;

            public Web(Tile tile)
            {
                Tile = tile;
            }

            public int TurnCounter;
        }

        public int FrequencyTurns;
        public int WebDurabilityTurns;
        public int EntanglementDurationEnergy;

        private int _turnCounter;
        private List<Web> _webs = new();

        public override void Bind()
        {
            Owner.TurnEnded += Work;
        }

        private void Work()
        {
            _turnCounter++;
            for (int i = _webs.Count - 1; i >= 0 ; i--)
            {
                _webs[i].TurnCounter++;
                if(_webs[i].TurnCounter >= WebDurabilityTurns)
                {
                    _webs[i].Tile.Modifier ^= TileModifier.Web;
                    _webs[i].Tile.RemoveTexture(87);
                    _webs[i].Tile.CreatureCame -= CatchInWeb;
                    _webs.RemoveAt(i);
                }
            }

            if(_turnCounter >= FrequencyTurns)
            {
                _turnCounter = 0;
                // place a web
                var tile = Main.Game.Map.GetTile(Owner.MapIndex);
                if(!_webs.Any(x => x.Tile == tile))
                {
                    tile.Modifier |= TileModifier.Web;
                    tile.AddTexture(87);
                    tile.CreatureCame += CatchInWeb;
                    _webs.Add(new Web(tile));
                }
            }
        }

        private void CatchInWeb(Creature incomingCreature)
        {
            if(incomingCreature.TemplateName != "Spider")
            {
                incomingCreature.Energy -= EntanglementDurationEnergy;
            }
        }

        public override Behaviour Clone()
        {
            return new PlaceWebs
            {
                FrequencyTurns = FrequencyTurns,
                WebDurabilityTurns = WebDurabilityTurns,
                EntanglementDurationEnergy = EntanglementDurationEnergy,
            };
        }

        public override void SetAttribute(string name, string value)
        {
            if(name == "FrequencyTurns")
            {
                FrequencyTurns = int.Parse(value);
            }
            else if(name == "WebDurabilityTurns")
            {
                WebDurabilityTurns = int.Parse(value);
            }
             else if(name == "EntanglementDurationEnergy")
            {
                EntanglementDurationEnergy = int.Parse(value);
            }
        }
    }
}