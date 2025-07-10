namespace DragonBallBattles.Domain
{
    public class Battle
    {
        public Character Fighter1 { get; set; }
        public Character Fighter2 { get; set; }

        public override string ToString()
        {
            return $"{Fighter1.Name} vs {Fighter2.Name}";
        }
    }
}