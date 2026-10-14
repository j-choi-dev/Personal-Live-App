using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using System.Text;
using UniRx;

namespace LiveApp.UI.Presenter
{
    public interface IResourceInspectorView : IViewBase
    {

        /// <summary>
        /// Filter 변경 이벤트
        /// </summary>
        IObservable<int> OnFilterChanged { get; }
        /// <summary>
        /// Remove Btn Click
        /// </summary>
        IObservable<Unit> OnClickRemove { get; }
        /// <summary>
        /// 현재 선택한 리소스 ID값
        /// </summary>
        IReadOnlyList<string> CurrSelectedItemList { get; }
        /// <summary>
        /// 현재 선택한 필터의 dropdown index
        /// </summary>
        int CurrentFilterIndex { get; }
        /// <summary>
        /// List View 타이틀에 표시할 리소스 타입을 세팅
        /// </summary>
        /// <param name="title">리소스 타입</param>
        void SetTitle(string title);
        /// <summary>
        /// filter list 세팅
        /// </summary>
        /// <param name="filters">필터 리스트</param>
        void SetFilterList(IReadOnlyList<string> filters);
        /// <summary>
        /// dropdown index를 기준으로 filter명을 dropdown에 표시
        /// </summary>
        /// <param name="index">dropdown index</param>
        void SetFilterItem(int index);
        /// <summary>
        /// list view에 resource list를 세팅
        /// </summary>
        /// <param name="list">Resource List</param>
        void SetResourceItemList(IReadOnlyList<(string id, string name)> list);
        /// <summary>
        /// list view에 resource item 추가
        /// </summary>
        /// <param name="id"> item id(= resource id)</param>
        /// <param name="name">표시명</param>
        void AddListItem(string id, string name);
        /// <summary>
        /// list view에 item을 삭제
        /// </summary>
        /// <param name="id"></param>
        void RemoveListItem(string id);
        /// <summary>
        /// list 초기화
        /// </summary>
        void ClearList();



    }
}
