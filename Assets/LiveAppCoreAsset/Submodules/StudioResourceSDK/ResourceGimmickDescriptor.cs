using StudioResourceSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    public enum GimmickEntryType
    {
        Group,
        Event
    }

    public class ResourceGimmickDescriptor
    {
        public string ResourceID { get; }
        public int CollectionID { get; }
        public string DisplayName { get; }
        public IReadOnlyList<GimmickEntryDescriptor> Entries => _entries;

        private readonly List<GimmickEntryDescriptor> _entries = new List<GimmickEntryDescriptor>();

        public ResourceGimmickDescriptor(string resourceID, int collectionID, string displayName)
        {
            ResourceID = resourceID;
            CollectionID = collectionID;
            DisplayName = displayName;
        }

        public void Add(GimmickEntryDescriptor entry)
        {
            if (entry != null)
            {
                _entries.Add(entry);
            }
        }
    }

    public class GimmickEntryDescriptor
    {
        public GimmickEntryType EntryType { get; }
        public GimmickGroupDescriptor Group { get; }
        public GimmickEventDescriptor Event { get; }

        public GimmickEntryDescriptor(GimmickGroupDescriptor group)
        {
            EntryType = GimmickEntryType.Group;
            Group = group;
        }

        public GimmickEntryDescriptor(GimmickEventDescriptor eventData)
        {
            EntryType = GimmickEntryType.Event;
            Event = eventData;
        }
    }

    public class GimmickGroupDescriptor
    {
        public int GroupID { get; }
        public string DisplayName { get; }
        public IReadOnlyList<GimmickEntryDescriptor> Entries => _entries;

        private readonly List<GimmickEntryDescriptor> _entries = new List<GimmickEntryDescriptor>();

        public GimmickGroupDescriptor(int groupID, string displayName)
        {
            GroupID = groupID;
            DisplayName = displayName;
        }

        public void Add(GimmickEntryDescriptor entry)
        {
            if (entry != null)
            {
                _entries.Add(entry);
            }
        }
    }

    public class GimmickEventDescriptor
    {
        public int EventID { get; }
        public string DisplayName { get; }
        public ResourceEventValueType ResourceEventValueType { get; }

        public GimmickEventDescriptor(int eventID, string displayName, ResourceEventValueType valueType)
        {
            EventID = eventID;
            DisplayName = displayName;
            ResourceEventValueType = valueType;
        }
    }
}