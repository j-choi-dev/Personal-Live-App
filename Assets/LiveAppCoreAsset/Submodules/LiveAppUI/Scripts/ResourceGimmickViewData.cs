using System.Collections.Generic;

namespace LiveAppUI.Domain
{
    public enum GimmickControlType
    {
        Trigger,
        Bool,
        Int,
        Float,
        String
    }

    public class ResourceGimmickViewData
    {
        public string ResourceID;
        public string ResourceName;
        public int CollectionID;

        public readonly List<GimmickViewEntry> Entries = new List<GimmickViewEntry>();
    }

    public class GimmickViewEntry
    {
        public GimmickGroupViewData Group;
        public GimmickEventViewData Event;

        public bool IsGroup => Group != null;
        public bool IsEvent => Event != null;
    }

    public class GimmickGroupViewData
    {
        public string DisplayName;
        public readonly List<GimmickViewEntry> Entries = new List<GimmickViewEntry>();
    }

    public class GimmickEventViewData
    {
        public int EventID;
        public string DisplayName;
        public GimmickControlType ControlType;
    }
}