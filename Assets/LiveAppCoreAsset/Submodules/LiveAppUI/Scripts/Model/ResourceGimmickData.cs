using System.Collections.Generic;

namespace LiveAppUI.Model
{
    public enum GimmickValueType
    {
        Trigger,
        Bool,
        Int,
        Float,
        String
    }

    public class ResourceGimmickData
    {
        public string ResourceID { get; }
        public int CollectionID { get; }
        public string DisplayName { get; }

        public IReadOnlyList<GimmickEntryData> Entries => _entries;

        private readonly List<GimmickEntryData> _entries = new List<GimmickEntryData>();

        public ResourceGimmickData(
            string resourceID,
            int collectionID,
            string displayName)
        {
            ResourceID = resourceID;
            CollectionID = collectionID;
            DisplayName = displayName;
        }

        public void Add(GimmickEntryData entry)
        {
            if (entry != null)
            {
                _entries.Add(entry);
            }
        }
    }

    public class GimmickEntryData
    {
        public GimmickGroupData Group { get; }
        public GimmickEventData Event { get; }

        public bool IsGroup => Group != null;
        public bool IsEvent => Event != null;

        public GimmickEntryData(GimmickGroupData group)
        {
            Group = group;
        }

        public GimmickEntryData(GimmickEventData eventData)
        {
            Event = eventData;
        }
    }

    public class GimmickGroupData
    {
        public int GroupID { get; }
        public string DisplayName { get; }

        public IReadOnlyList<GimmickEntryData> Entries => _entries;

        private readonly List<GimmickEntryData> _entries = new List<GimmickEntryData>();

        public GimmickGroupData(int groupID, string displayName)
        {
            GroupID = groupID;
            DisplayName = displayName;
        }

        public void Add(GimmickEntryData entry)
        {
            if (entry != null)
            {
                _entries.Add(entry);
            }
        }
    }

    public class GimmickEventData
    {
        public int EventID { get; }
        public string DisplayName { get; }
        public GimmickValueType ValueType { get; }

        public GimmickEventData(
            int eventID,
            string displayName,
            GimmickValueType valueType)
        {
            EventID = eventID;
            DisplayName = displayName;
            ValueType = valueType;
        }
    }
}