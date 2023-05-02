namespace RpgCollector.Models.MasterModel
{
    public enum TypeDefinition
    {
        UNKNOWN = -1,
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
