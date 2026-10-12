using UnityEngine;
using UnityEngine.Events;

namespace StudioResourceSDK.Domain
{
    [DisallowMultipleComponent]
    public class BoolEmitter : MonoBehaviour
    {
        [SerializeField] private UnityEvent _trueMethod = new UnityEvent();
        [SerializeField] private UnityEvent _falseMethod = new UnityEvent();

        public void Emit(bool value)
        {
            if (value)
            {
                _trueMethod.Invoke();
            }
            else
            {
                _falseMethod.Invoke();
            }
        }
    }
}