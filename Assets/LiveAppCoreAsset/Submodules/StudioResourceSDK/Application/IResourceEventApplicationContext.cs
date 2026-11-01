namespace StudioResourceSDK.Application
{
    public interface IResourceEventApplicationContext
    {
        bool TryGetGimmickDescriptor(string resourceID, out ResourceGimmickDescriptor descriptor);

        bool InvokeTrigger(string resourceID, int collectionID, int eventID);
        bool InvokeBool(string resourceID, int collectionID, int eventID, bool value);
        bool InvokeInt(string resourceID, int collectionID, int eventID, int value);
        bool InvokeFloat(string resourceID, int collectionID, int eventID, float value);
        bool InvokeString(string resourceID, int collectionID, int eventID, string value);
    }
}