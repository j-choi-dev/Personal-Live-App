using LiveAppUI.Domain;
using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class ResourceGimmickView : MonoBehaviour, IResourceGimmickView
    {
        [Header("Left Side")]
        [SerializeField] private Transform _resourceButtonRoot = null;
        [SerializeField] private GimmickResourceButtonView _resourceButtonPrefab = null;

        [Header("Main Side")]
        [SerializeField] private TMP_Text _resourceTitle = null;
        [SerializeField] private Transform _gimmickContentRoot = null;

        [Header("Gimmick Prefabs")]
        [SerializeField] private GimmickGroupItemView _groupPrefab = null;
        [SerializeField] private GimmickButtonItemView _buttonPrefab = null;
        [SerializeField] private GimmickToggleItemView _togglePrefab = null;
        [SerializeField] private GimmickInputItemView _inputPrefab = null;

        private readonly Dictionary<string, GimmickResourceButtonView> _resourceButtons =
            new Dictionary<string, GimmickResourceButtonView>();

        private readonly Subject<string> _onSelectResource = new Subject<string>();
        public IObservable<string> OnSelectResource => _onSelectResource;

        private readonly Subject<(string resourceID, int collectionID, int eventID)> _onTrigger =
            new Subject<(string resourceID, int collectionID, int eventID)>();
        public IObservable<(string resourceID, int collectionID, int eventID)> OnTrigger => _onTrigger;

        private readonly Subject<(string resourceID, int collectionID, int eventID, bool value)> _onBool =
            new Subject<(string resourceID, int collectionID, int eventID, bool value)>();
        public IObservable<(string resourceID, int collectionID, int eventID, bool value)> OnBool => _onBool;

        private readonly Subject<(string resourceID, int collectionID, int eventID, int value)> _onInt =
            new Subject<(string resourceID, int collectionID, int eventID, int value)>();
        public IObservable<(string resourceID, int collectionID, int eventID, int value)> OnInt => _onInt;

        private readonly Subject<(string resourceID, int collectionID, int eventID, float value)> _onFloat =
            new Subject<(string resourceID, int collectionID, int eventID, float value)>();
        public IObservable<(string resourceID, int collectionID, int eventID, float value)> OnFloat => _onFloat;

        private readonly Subject<(string resourceID, int collectionID, int eventID, string value)> _onString =
            new Subject<(string resourceID, int collectionID, int eventID, string value)>();
        public IObservable<(string resourceID, int collectionID, int eventID, string value)> OnString => _onString;

        public bool IsActive => gameObject.activeSelf;

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void AddResourceButton(string resourceID, int collectionID)
        {
            if (_resourceButtons.ContainsKey(resourceID))
            {
                return;
            }

            GimmickResourceButtonView button = Instantiate(_resourceButtonPrefab, _resourceButtonRoot);
            button.Initialize(resourceID, collectionID);

            button.OnClick
                .Subscribe(_ => _onSelectResource.OnNext(resourceID))
                .AddTo(button);

            _resourceButtons.Add(resourceID, button);
        }

        public void RemoveResourceButton(string resourceID)
        {
            if (_resourceButtons.TryGetValue(resourceID, out GimmickResourceButtonView button) == false)
            {
                return;
            }

            _resourceButtons.Remove(resourceID);

            if (button != null)
            {
                Destroy(button.gameObject);
            }
        }

        public void SetSelectedResource(string resourceID)
        {
            foreach (KeyValuePair<string, GimmickResourceButtonView> item in _resourceButtons)
            {
                item.Value.SetSelected(item.Key == resourceID);
            }
        }

        public void SetGimmickContent(ResourceGimmickViewData data)
        {
            ClearGimmickContent();

            _resourceTitle.text = data.ResourceName;

            foreach (GimmickViewEntry entry in data.Entries)
            {
                CreateEntry(data, entry, _gimmickContentRoot);
            }
        }

        public void ClearGimmickContent()
        {
            _resourceTitle.text = string.Empty;

            for (int i = _gimmickContentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_gimmickContentRoot.GetChild(i).gameObject);
            }
        }

        public void ClearResourceButtons()
        {
            foreach (GimmickResourceButtonView button in _resourceButtons.Values)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            _resourceButtons.Clear();
        }

        private void CreateEntry(ResourceGimmickViewData resource, GimmickViewEntry entry, Transform parent)
        {
            if (entry.IsGroup)
            {
                CreateGroup(resource, entry.Group, parent);
                return;
            }

            if (entry.IsEvent)
            {
                CreateEvent(resource, entry.Event, parent);
            }
        }

        private void CreateGroup(ResourceGimmickViewData resource, GimmickGroupViewData group, Transform parent)
        {
            GimmickGroupItemView groupView = Instantiate(_groupPrefab, parent);
            groupView.Initialize(group.DisplayName);

            foreach (GimmickViewEntry entry in group.Entries)
            {
                CreateEntry(resource, entry, groupView.ContentRoot);
            }
        }

        private void CreateEvent(ResourceGimmickViewData resource, GimmickEventViewData eventData, Transform parent)
        {
            switch (eventData.ControlType)
            {
                case GimmickControlType.Trigger:
                    CreateTrigger(resource, eventData, parent);
                    break;

                case GimmickControlType.Bool:
                    CreateBool(resource, eventData, parent);
                    break;

                case GimmickControlType.Int:
                case GimmickControlType.Float:
                case GimmickControlType.String:
                    CreateInput(resource, eventData, parent);
                    break;
            }
        }

        private void CreateTrigger(ResourceGimmickViewData resource, GimmickEventViewData eventData, Transform parent)
        {
            GimmickButtonItemView item = Instantiate(_buttonPrefab, parent);
            item.Initialize(eventData.DisplayName);

            item.OnClick
                .Subscribe(_ =>
                {
                    _onTrigger.OnNext((
                        resource.ResourceID,
                        resource.CollectionID,
                        eventData.EventID
                    ));
                })
                .AddTo(item);
        }

        private void CreateBool(ResourceGimmickViewData resource, GimmickEventViewData eventData, Transform parent)
        {
            GimmickToggleItemView item = Instantiate(_togglePrefab, parent);
            item.Initialize(eventData.DisplayName);

            item.OnValueChanged
                .Subscribe(value =>
                {
                    _onBool.OnNext((
                        resource.ResourceID,
                        resource.CollectionID,
                        eventData.EventID,
                        value
                    ));
                })
                .AddTo(item);
        }

        private void CreateInput(ResourceGimmickViewData resource, GimmickEventViewData eventData, Transform parent)
        {
            GimmickInputItemView item = Instantiate(_inputPrefab, parent);
            item.Initialize(eventData.DisplayName);

            switch (eventData.ControlType)
            {
                case GimmickControlType.Int:
                    item.SetContentType(TMP_InputField.ContentType.IntegerNumber);
                    break;

                case GimmickControlType.Float:
                    item.SetContentType(TMP_InputField.ContentType.DecimalNumber);
                    break;

                case GimmickControlType.String:
                    item.SetContentType(TMP_InputField.ContentType.Standard);
                    break;
            }

            item.OnEndEdit
                .Subscribe(value => PublishInput(resource, eventData, value))
                .AddTo(item);
        }

        private void PublishInput(ResourceGimmickViewData resource, GimmickEventViewData eventData, string value)
        {
            switch (eventData.ControlType)
            {
                case GimmickControlType.Int:
                    if (int.TryParse(value, out int intValue))
                    {
                        _onInt.OnNext((
                            resource.ResourceID,
                            resource.CollectionID,
                            eventData.EventID,
                            intValue
                        ));
                    }
                    break;

                case GimmickControlType.Float:
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                    {
                        _onFloat.OnNext((
                            resource.ResourceID,
                            resource.CollectionID,
                            eventData.EventID,
                            floatValue
                        ));
                    }
                    break;

                case GimmickControlType.String:
                    _onString.OnNext((
                        resource.ResourceID,
                        resource.CollectionID,
                        eventData.EventID,
                        value
                    ));
                    break;
            }
        }

        private void OnDestroy()
        {
            _onSelectResource.Dispose();
            _onTrigger.Dispose();
            _onBool.Dispose();
            _onInt.Dispose();
            _onFloat.Dispose();
            _onString.Dispose();
        }
    }
}