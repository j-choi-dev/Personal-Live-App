namespace StudioResourceSDK.Domain
{
    public class ResourceConstValue
    {
        public static readonly string BinFileName = "ResourceInfo.bin"; // TODO 리팩터링 대상 @Choi 26.07.04
        public static readonly string ReosurceCloudConfigFille = "awsConfig.bin";
    }
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

    public class CloudConfigData
    {
        public string AccessKey{ get; private set; }

        public string SecretAccessKey { get; private set; }

        public CloudConfigData(string accessKey, string secretAccessKey)
        {
            AccessKey = accessKey;
            SecretAccessKey = secretAccessKey;
        }


    }
}