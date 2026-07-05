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
}