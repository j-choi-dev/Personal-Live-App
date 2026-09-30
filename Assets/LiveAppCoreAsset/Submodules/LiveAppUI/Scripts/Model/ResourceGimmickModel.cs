using StudioResourceSDK.Application;
using StudioResourceSDK.Domain;
using System;
using UniRx;

namespace LiveAppUI.Model
{
    public class ResourceGimmickModel : IResourceGimmickModel, IDisposable
    {
        private readonly IResourceEventApplicationContext _resourceEventContext;
        private readonly ISceneResourceLifecycleContext _resourceLifecycleContext;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        private readonly Subject<ResourceGimmickData> _onGimmickAdded =
            new Subject<ResourceGimmickData>();

        public IObservable<ResourceGimmickData> OnGimmickAdded =>
            _onGimmickAdded;

        private readonly Subject<string> _onGimmickRemoved =
            new Subject<string>();

        public IObservable<string> OnGimmickRemoved =>
            _onGimmickRemoved;

        public ResourceGimmickModel(
            IResourceEventApplicationContext resourceEventContext,
            ISceneResourceLifecycleContext resourceLifecycleContext)
        {
            _resourceEventContext = resourceEventContext;
            _resourceLifecycleContext = resourceLifecycleContext;

            _resourceLifecycleContext.OnGimmickRegistered
                .Subscribe(OnGimmickRegistered)
                .AddTo(_disposable);

            _resourceLifecycleContext.OnGimmickRemoved
                .Subscribe(resourceID =>
                {
                    _onGimmickRemoved.OnNext(resourceID);
                })
                .AddTo(_disposable);
        }

        public bool TryGetGimmick(
            string resourceID,
            out ResourceGimmickData data)
        {
            data = null;

            if (_resourceEventContext.TryGetGimmickDescriptor(
                    resourceID,
                    out ResourceGimmickDescriptor descriptor) == false)
            {
                return false;
            }

            data = ConvertDescriptor(descriptor);

            return data != null;
        }

        public bool InvokeTrigger(
            string resourceID,
            int collectionID,
            int eventID)
        {
            return _resourceEventContext.InvokeTrigger(
                resourceID,
                collectionID,
                eventID
            );
        }

        public bool InvokeBool(
            string resourceID,
            int collectionID,
            int eventID,
            bool value)
        {
            return _resourceEventContext.InvokeBool(
                resourceID,
                collectionID,
                eventID,
                value
            );
        }

        public bool InvokeInt(
            string resourceID,
            int collectionID,
            int eventID,
            int value)
        {
            return _resourceEventContext.InvokeInt(
                resourceID,
                collectionID,
                eventID,
                value
            );
        }

        public bool InvokeFloat(
            string resourceID,
            int collectionID,
            int eventID,
            float value)
        {
            return _resourceEventContext.InvokeFloat(
                resourceID,
                collectionID,
                eventID,
                value
            );
        }

        public bool InvokeString(
            string resourceID,
            int collectionID,
            int eventID,
            string value)
        {
            return _resourceEventContext.InvokeString(
                resourceID,
                collectionID,
                eventID,
                value
            );
        }

        private void OnGimmickRegistered(string resourceID)
        {
            if (TryGetGimmick(resourceID, out ResourceGimmickData data) == false)
            {
                return;
            }

            _onGimmickAdded.OnNext(data);
        }

        private ResourceGimmickData ConvertDescriptor(
            ResourceGimmickDescriptor descriptor)
        {
            ResourceGimmickData result = new ResourceGimmickData(
                descriptor.ResourceID,
                descriptor.CollectionID,
                descriptor.DisplayName
            );

            foreach (GimmickEntryDescriptor entry in descriptor.Entries)
            {
                GimmickEntryData converted = ConvertEntry(entry);

                if (converted != null)
                {
                    result.Add(converted);
                }
            }

            return result;
        }

        private GimmickEntryData ConvertEntry(
            GimmickEntryDescriptor descriptor)
        {
            if (descriptor == null)
            {
                return null;
            }

            if (descriptor.Group != null)
            {
                GimmickGroupData group = new GimmickGroupData(
                    descriptor.Group.GroupID,
                    descriptor.Group.DisplayName
                );

                foreach (GimmickEntryDescriptor child in descriptor.Group.Entries)
                {
                    GimmickEntryData childData = ConvertEntry(child);

                    if (childData != null)
                    {
                        group.Add(childData);
                    }
                }

                return new GimmickEntryData(group);
            }

            if (descriptor.Event != null)
            {
                return new GimmickEntryData(
                    new GimmickEventData(
                        descriptor.Event.EventID,
                        descriptor.Event.DisplayName,
                        ConvertValueType(descriptor.Event.ResourceEventValueType)
                    )
                );
            }

            return null;
        }

        private GimmickValueType ConvertValueType(ResourceEventValueType type)
        {
            switch (type)
            {
                case ResourceEventValueType.Trigger:
                    return GimmickValueType.Trigger;

                case ResourceEventValueType.Bool:
                    return GimmickValueType.Bool;

                case ResourceEventValueType.Int:
                    return GimmickValueType.Int;

                case ResourceEventValueType.Float:
                    return GimmickValueType.Float;

                case ResourceEventValueType.String:
                    return GimmickValueType.String;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(type),
                        type,
                        null
                    );
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();

            _onGimmickAdded.Dispose();
            _onGimmickRemoved.Dispose();
        }
    }
}