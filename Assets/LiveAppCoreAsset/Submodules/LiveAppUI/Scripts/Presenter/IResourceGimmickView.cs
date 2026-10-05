using LiveAppUI.Domain;
using System;

namespace LiveAppUI.Presenter
{
    public interface IResourceGimmickView : IViewBase
    {
        IObservable<string> OnSelectResource { get; }

        IObservable<(string resourceID, int collectionID, int eventID)> OnTrigger { get; }
        IObservable<(string resourceID, int collectionID, int eventID, bool value)> OnBool { get; }
        IObservable<(string resourceID, int collectionID, int eventID, int value)> OnInt { get; }
        IObservable<(string resourceID, int collectionID, int eventID, float value)> OnFloat { get; }
        IObservable<(string resourceID, int collectionID, int eventID, string value)> OnString { get; }

        void AddResourceButton(string resourceID, int collectionID);
        void RemoveResourceButton(string resourceID);
        void SetSelectedResource(string resourceID);

        void SetGimmickContent(ResourceGimmickViewData data);
        void ClearGimmickContent();
        void ClearResourceButtons();
    }
}