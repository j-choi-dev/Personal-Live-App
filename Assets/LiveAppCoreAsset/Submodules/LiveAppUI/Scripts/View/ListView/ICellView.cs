using System;
using UniRx;
using UnityEngine;

namespace LiveApp.UI
{
    public interface ICellView
    {
        GameObject Object { get; }
        IObservable<Unit> OnSelected { get; }
        bool IsUsable { get; }
        string ID { get; }
        void SetIsUsable( bool isVal );
        void SetItem( string text, string id );
        void SetIdIfNull( string id );
    }
}
