using LiveAppUI.Domain;
using LiveAppUI.Model;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace LiveAppUI.Presenter
{
    public class ResourceGimmickPresenter : MonoBehaviour
    {
        private IResourceGimmickView _resourceGimmickView;
        private IResourceGimmickModel _resourceGimmickModel;

        private readonly Dictionary<string, ResourceGimmickViewData> _gimmickTable =
            new Dictionary<string, ResourceGimmickViewData>();

        private readonly List<string> _resourceOrder = new List<string>();

        private string _selectedResourceID;

        [Inject]
        public void Initialize(
            IResourceGimmickView resourceGimmickView,
            IResourceGimmickModel resourceGimmickModel)
        {
            _resourceGimmickView = resourceGimmickView;
            _resourceGimmickModel = resourceGimmickModel;
        }

        private void Awake()
        {
            SubscribeView();
            SubscribeModel();
        }

        private void SubscribeView()
        {
            _resourceGimmickView.OnSelectResource
                .Subscribe(SelectResource)
                .AddTo(this);

            _resourceGimmickView.OnTrigger
                .Subscribe(data =>
                {
                    _resourceGimmickModel.InvokeTrigger(
                        data.resourceID,
                        data.collectionID,
                        data.eventID
                    );
                })
                .AddTo(this);

            _resourceGimmickView.OnBool
                .Subscribe(data =>
                {
                    _resourceGimmickModel.InvokeBool(
                        data.resourceID,
                        data.collectionID,
                        data.eventID,
                        data.value
                    );
                })
                .AddTo(this);

            _resourceGimmickView.OnInt
                .Subscribe(data =>
                {
                    _resourceGimmickModel.InvokeInt(
                        data.resourceID,
                        data.collectionID,
                        data.eventID,
                        data.value
                    );
                })
                .AddTo(this);

            _resourceGimmickView.OnFloat
                .Subscribe(data =>
                {
                    _resourceGimmickModel.InvokeFloat(
                        data.resourceID,
                        data.collectionID,
                        data.eventID,
                        data.value
                    );
                })
                .AddTo(this);

            _resourceGimmickView.OnString
                .Subscribe(data =>
                {
                    _resourceGimmickModel.InvokeString(
                        data.resourceID,
                        data.collectionID,
                        data.eventID,
                        data.value
                    );
                })
                .AddTo(this);
        }

        private void SubscribeModel()
        {
            _resourceGimmickModel.OnGimmickAdded
                .Subscribe(OnGimmickAdded)
                .AddTo(this);

            _resourceGimmickModel.OnGimmickRemoved
                .Subscribe(OnGimmickRemoved)
                .AddTo(this);
        }

        private void OnGimmickAdded(ResourceGimmickData data)
        {
            ResourceGimmickViewData viewData = ConvertViewData(data);

            if (_gimmickTable.ContainsKey(data.ResourceID))
            {
                _gimmickTable[data.ResourceID] = viewData;

                if (_selectedResourceID == data.ResourceID)
                {
                    _resourceGimmickView.SetGimmickContent(viewData);
                }

                return;
            }

            _gimmickTable.Add(data.ResourceID, viewData);
            _resourceOrder.Add(data.ResourceID);

            _resourceGimmickView.AddResourceButton(
                data.ResourceID,
                data.CollectionID
            );

            if (string.IsNullOrEmpty(_selectedResourceID))
            {
                SelectResource(data.ResourceID);
            }
        }

        private void OnGimmickRemoved(string resourceID)
        {
            _gimmickTable.Remove(resourceID);
            _resourceOrder.Remove(resourceID);

            _resourceGimmickView.RemoveResourceButton(resourceID);

            if (_selectedResourceID != resourceID)
            {
                return;
            }

            _selectedResourceID = null;
            _resourceGimmickView.ClearGimmickContent();

            if (_resourceOrder.Count > 0)
            {
                SelectResource(_resourceOrder[0]);
            }
        }

        private void SelectResource(string resourceID)
        {
            if (_gimmickTable.TryGetValue(resourceID, out ResourceGimmickViewData data) == false)
            {
                return;
            }

            _selectedResourceID = resourceID;

            _resourceGimmickView.SetSelectedResource(resourceID);
            _resourceGimmickView.SetGimmickContent(data);
        }

        private ResourceGimmickViewData ConvertViewData(ResourceGimmickData data)
        {
            ResourceGimmickViewData result = new ResourceGimmickViewData
            {
                ResourceID = data.ResourceID,
                ResourceName = data.DisplayName,
                CollectionID = data.CollectionID
            };

            foreach (GimmickEntryData entry in data.Entries)
            {
                GimmickViewEntry converted = ConvertEntry(entry);

                if (converted != null)
                {
                    result.Entries.Add(converted);
                }
            }

            return result;
        }

        private GimmickViewEntry ConvertEntry(GimmickEntryData data)
        {
            if (data.IsGroup)
            {
                GimmickGroupViewData group = new GimmickGroupViewData
                {
                    DisplayName = data.Group.DisplayName
                };

                foreach (GimmickEntryData child in data.Group.Entries)
                {
                    GimmickViewEntry childEntry = ConvertEntry(child);

                    if (childEntry != null)
                    {
                        group.Entries.Add(childEntry);
                    }
                }

                return new GimmickViewEntry
                {
                    Group = group
                };
            }

            if (data.IsEvent)
            {
                return new GimmickViewEntry
                {
                    Event = new GimmickEventViewData
                    {
                        EventID = data.Event.EventID,
                        DisplayName = data.Event.DisplayName,
                        ControlType = ConvertControlType(data.Event.ValueType)
                    }
                };
            }

            return null;
        }

        private GimmickControlType ConvertControlType(GimmickValueType valueType)
        {
            switch (valueType)
            {
                case GimmickValueType.Trigger:
                    return GimmickControlType.Trigger;

                case GimmickValueType.Bool:
                    return GimmickControlType.Bool;

                case GimmickValueType.Int:
                    return GimmickControlType.Int;

                case GimmickValueType.Float:
                    return GimmickControlType.Float;

                case GimmickValueType.String:
                    return GimmickControlType.String;

                default:
                    return GimmickControlType.Trigger;
            }
        }
    }
}