namespace RpgCollector.Models.MasterData
{
    public enum TypeDefinition
    {
        CONSUMPTION = 0, 
        EQUIPMENT = 1, 
        MONEY = 2,
    }

    public class MasterItemType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
    }
}
