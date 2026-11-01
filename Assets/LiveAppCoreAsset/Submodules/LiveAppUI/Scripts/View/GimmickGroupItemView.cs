using TMPro;
using UnityEngine;

namespace LiveAppUI.View
{
    public class GimmickGroupItemView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _displayName = null;
        [SerializeField] private Transform _contentRoot = null;

        public Transform ContentRoot => _contentRoot;

        public void Initialize(string displayName)
        {
            _displayName.text = displayName;
        }
    }
}