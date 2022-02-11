namespace Pandorai.Creatures
{
    public class CreatureSounds
    {
        public string Attack { get; set; } = string.Empty;
        public string Hurt { get; set; } = string.Empty;
        public string Death { get; set; } = string.Empty;
        public string Aggro { get; set; } = string.Empty;
        public string Ambient { get; set; } = string.Empty;

        public CreatureSounds Clone()
        {
            return new CreatureSounds
            {
                Attack = Attack,
                Hurt = Hurt,
                Death = Death,
                Aggro = Aggro,
                Ambient = Ambient,
            };
        }
    }
}