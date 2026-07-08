using StudioResourceSDK.Domain;
using StudioSystemSDK.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniRx;

namespace StudioSystemSDK.Infrastructure
{
    public class ResourceDataParser : IResourceDataParseDomain
    {
        private const int HeaderCount = 2;

        private Subject<IReadOnlyCollection<CharacterResourceItem>> _onCharacterListChanged 
            = new Subject<IReadOnlyCollection<CharacterResourceItem>>();
        public IObservable<IReadOnlyCollection<CharacterResourceItem>> OnCharacterListChanged
            => _onCharacterListChanged;

        private Subject<IReadOnlyCollection<StageResourceItem>> _onStageListChanged
            = new Subject<IReadOnlyCollection<StageResourceItem>>();
        public IObservable<IReadOnlyCollection<StageResourceItem>> OnStageListChanged
            => _onStageListChanged;


        private Subject<IReadOnlyCollection<PropResourceItem>> _onPropListChanged
            = new Subject<IReadOnlyCollection<PropResourceItem>>();
        public IObservable<IReadOnlyCollection<PropResourceItem>> OnPropListChanged
            => _onPropListChanged;

        public IReadOnlyCollection<CharacterResourceItem> ParseCharacterData( string rawData )
        {
            var list = new List<CharacterResourceItem>();
            UnityEngine.Debug.Log( rawData );
            var rows = rawData.Split("\n");
            for(var i = HeaderCount; i < rows.Length; ++i)
            {
                var cols = rows[i].Split(",");
                var id = cols[0];
                var displayName = cols[1];
                // TODO enum을 Flag 타입으로 수정 후 전용 파싱 로직 구현 필요 @Choi 26.07.07
                var item = new CharacterResourceItem(id, displayName, CharacterGeneration.Gen_1, UsingType.Test);
                list.Add( item );
            }
            _onCharacterListChanged.OnNext( list );
            return list;
        }

        public IReadOnlyCollection<PropResourceItem> ParsePropData( string rawData )
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<StageResourceItem> ParseStageData( string rawData )
        {
            throw new System.NotImplementedException();
        }
    }
}
