namespace RpgCollector.Models
{
    public class Player
    {
        public int UserId { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentExp { get; set; }
        public int MaxExp { get; set; }
        public int Level { get; set; }
        public int Money { get; set; }
    }
}
