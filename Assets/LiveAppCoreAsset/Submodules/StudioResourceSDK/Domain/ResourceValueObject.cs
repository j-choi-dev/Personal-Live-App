namespace StudioResourceSDK.Domain
{
    public enum ResourceType
    {
        None = 0,
        Character,
        Stage,
        Prop,
        Media,
    }

    public class ResourceTableData
    {
        public string ID { get; }
        public string DisplayName { get; }
        public ResourceType ResourceType { get; }

        public ResourceTableData( string id, string displayName, ResourceType resourceType )
        {
            ID = id;
            DisplayName = displayName;
            ResourceType = resourceType;
        }
    }

    public enum CharacterGeneration
    {
        Gen_1 = 0,
        Gen_2,
    }

    public enum UsingType
    {
        Prod = 0,
        Test,
    }

    public class ResourceItemBase
    {
        public string ID { get; private set; }
        public string DisplayName{ get; private set; }
        public ResourceItemBase( string id, string displayName )
        {
            ID = id;
            DisplayName = displayName;
        }
    }

    public class CharacterResourceItem : ResourceItemBase
    {
        public CharacterGeneration Generation;
        public UsingType Type;

        public CharacterResourceItem(string  id, 
            string displayName, 
            CharacterGeneration generation, 
            UsingType type ) : base(id, displayName)
        {
            Generation = generation;
            Type = type;
        }
    }

    public class StageResourceItem
    {

    }

    public class PropResourceItem
    {

    }
}