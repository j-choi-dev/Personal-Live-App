using StudioCharacterSDK.Domain;
using StudioResourceSDK.Application;
using StudioTrackingSDK.Application;
using System;
using UniRx;

namespace LiveApp
{
    public class LipSyncModel : ILipSyncModel, IDisposable
    {
        private ILipSyncContext _lipSyncContext;
        private ISceneResourceListContext _sceneResourceListContext;

        private ILipSyncData _currentCharacter = null;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public LipSyncModel( ILipSyncContext lipSyncContext,
            ISceneResourceListContext sceneResourceListContext ) 
        {
            _lipSyncContext = lipSyncContext;
            _sceneResourceListContext = sceneResourceListContext;

            SubscribeTrackingData();

            // TODO 리팩터링 대상!!! @Choi 26.07.29
            _sceneResourceListContext.OnCurrentCharacterChanged
                .Subscribe( arg =>
                {
                    var target = arg as ILipSyncData;
                    if( target == null )
                    {
                        UnityEngine.Debug.LogError( $"Invalid Resource ; Not Exist Ficial Component({arg.ID})" );
                        _lipSyncContext.SetCharacterID( string.Empty );
                        _lipSyncContext.SetIsActive( false );
                        _currentCharacter = null;
                        return;
                    }

                    _lipSyncContext.SetCharacterID( arg.ID );
                    _lipSyncContext.SetIsActive( true );
                    _currentCharacter = target;
                } )
                .AddTo( _disposables );
        }

        public bool IsAbleTracking => _lipSyncContext.IsActive;

        public float Intensity { get; private set; }

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = null;
        }

        public void SetAbleTracking( bool isOK )
            => _lipSyncContext.SetIsActive( isOK );

        public void SetIntensity( float intensity )
            => Intensity = intensity;

        private void SubscribeTrackingData()
        {
            Observable.CombineLatest(
                _lipSyncContext.OnVowelA,
                _lipSyncContext.OnVowelE,
                _lipSyncContext.OnVowelI,
                _lipSyncContext.OnVowelO,
                _lipSyncContext.OnVowelU,
                LipSyncVowelData.Normalize )
                .Subscribe( val =>
                {
                    UnityEngine.Debug.Log( $"A = {_lipSyncContext.A}, E = {_lipSyncContext.E}, I = {_lipSyncContext.I}, O = {_lipSyncContext.O}, U = {_lipSyncContext.U}" );

                    _currentCharacter.SetLipSync( val );

                } )
                .AddTo( _disposables );
        }
    }
}
