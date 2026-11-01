namespace StudioResourceSDK.Domain
{
    public class SceneGimmickInfo
    {
        public string ResourceID { get; }
        public EventCollection EventCollection { get; }

        public int CollectionID => EventCollection != null ? EventCollection.CollectionID : 0;
        public string DisplayName => EventCollection != null ? EventCollection.DisplayName : string.Empty;

        public SceneGimmickInfo(string resourceID, EventCollection eventCollection)
        {
            ResourceID = resourceID;
            EventCollection = eventCollection;
        }
    }
}