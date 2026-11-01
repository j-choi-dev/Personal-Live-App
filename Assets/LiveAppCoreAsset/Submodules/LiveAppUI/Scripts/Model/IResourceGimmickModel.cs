using System;

namespace LiveAppUI.Model
{
    public interface IResourceGimmickModel
    {
        IObservable<ResourceGimmickData> OnGimmickAdded { get; }
        IObservable<string> OnGimmickRemoved { get; }

        bool TryGetGimmick(string resourceID, out ResourceGimmickData data);

        bool InvokeTrigger(string resourceID, int collectionID, int eventID);
        bool InvokeBool(string resourceID, int collectionID, int eventID, bool value);
        bool InvokeInt(string resourceID, int collectionID, int eventID, int value);
        bool InvokeFloat(string resourceID, int collectionID, int eventID, float value);
        bool InvokeString(string resourceID, int collectionID, int eventID, string value);
    }
}