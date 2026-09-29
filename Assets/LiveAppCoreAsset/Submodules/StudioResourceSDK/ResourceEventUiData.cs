using StudioResourceSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Presentation
{
    public class ResourceEventUiData
    {
        public string ResourceID;
        public string ResourceName;
        public int CollectionID;
        public readonly List<EventEntryUiData> Entries = new List<EventEntryUiData>();
    }

    public class EventEntryUiData
    {
        public EventGroupUiData Group;
        public EventElementUiData Element;

        public bool IsGroup => Group != null;
        public bool IsElement => Element != null;
    }

    public class EventGroupUiData
    {
        public int GroupID;
        public string DisplayName;
        public readonly List<EventEntryUiData> Entries = new List<EventEntryUiData>();
    }

    public class EventElementUiData
    {
        public int EventID;
        public string DisplayName;
        public EventDataType DataType;
    }
}