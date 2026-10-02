using LiveAppUI.Domain;
using System;

namespace LiveAppUI.Presenter
{
    public interface IResourceGimmickView : IViewBase
    {
        IObservable<(string resourceID, int collectionID, int eventID)> OnTrigger { get; }

        IObservable<(string resourceID, int collectionID, int eventID, bool value)> OnBool { get; }

        IObservable<(string resourceID, int collectionID, int eventID, int value)> OnInt { get; }

        IObservable<(string resourceID, int collectionID, int eventID, float value)> OnFloat { get; }

        IObservable<(string resourceID, int collectionID, int eventID, string value)> OnString { get; }

        void AddResourceItem(ResourceGimmickViewData data);
        void RemoveResourceItem(string resourceID);
        void Clear();
    }
}