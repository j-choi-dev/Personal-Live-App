using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StudioCharacterSDK.Infrastructure;

namespace StudioTrackingSDK.Infrastructure
{

    /// <summary>
    /// // TODO Layerd Architecture 대상 @Choi 25.02.18
    /// </summary>
    public class LipSyncManager : MonoBehaviour
    {
        [SerializeField] private StudioAvatar _avatar;
        [SerializeField] private TMP_Text _log;
        //[SerializeField] private OVRLipSyncContext _lipSyncContext; // TODO 복원대상 @Choi 26.07.08
        [SerializeField] private Toggle _micToggle;
        private string selectedMic;
        private bool isMicActive = false;
        private const float NOISE_TRESHOLD = 0.1f;

        // Start is called before the first frame update
        private void Awake()
        {
            _micToggle.SetIsOnWithoutNotify( false );
            var micArray = Microphone.devices;
            if( micArray.Length > 0 )
            {
                selectedMic = micArray[0]; // 첫 번째 마이크 선택
                Debug.Log( "Using Mic: " + selectedMic );
            }
            else
            {
                Debug.LogError( "No microphone detected." );
            }
        }

        private void Update()
        {
            if( _micToggle.isOn && !isMicActive )
            {
                StartMicrophoneProcess();
            }
            else if( !_micToggle.isOn && isMicActive )
            {
                StopMicrophoneProcess();
            }
            UpdateMouthShape();
        }

        private void StartMicrophoneProcess()
        {
            if( selectedMic == null )
            {
                return;
            }

            //_lipSyncContext.audioSource.clip = Microphone.Start( selectedMic, true, 10, 44100 );
            //_lipSyncContext.audioSource.loop = true;

            while( !( Microphone.GetPosition( selectedMic ) > 0 ) )
            {
            }

            //_lipSyncContext.audioSource.Play();
            isMicActive = true;
            Debug.Log( "Mic Started" );
        }

        private void StopMicrophoneProcess()
        {
            //_lipSyncContext.audioSource.Stop();
            Microphone.End( selectedMic );
            isMicActive = false;
            Debug.Log( "Mic Stopped" );
        }

        private void UpdateMouthShape()
        {
            //if( _lipSyncContext == null )
            //{
            //    return;
            //}
            //var frame = _lipSyncContext.GetCurrentPhonemeFrame();
            //if( frame == null )
            //{
            //    return;
            //}

            //var retVal = 0f;
            //var a = frame.Visemes[( int )Viseme.aa];
            //var e = frame.Visemes[( int )Viseme.E];
            //var i = frame.Visemes[( int )Viseme.ih];
            //var o = frame.Visemes[( int )Viseme.oh];
            //var u = frame.Visemes[( int )Viseme.ou];

            //var maxViseme = Mathf.Max( a, e, i, o, u );
            //var detectedVowel = "NONE";
            //if( maxViseme > NOISE_TRESHOLD )
            //{
            //    if( Mathf.Approximately( maxViseme, a ) )
            //    {
            //        detectedVowel = "a";
            //        retVal = a;
            //    }
            //    else if( Mathf.Approximately( maxViseme, e ) )
            //    {
            //        detectedVowel = "e";
            //        retVal = e;
            //    }
            //    else if( Mathf.Approximately( maxViseme, i ) )
            //    {
            //        detectedVowel = "i";
            //        retVal = i;
            //    }
            //    else if( Mathf.Approximately( maxViseme, o ) )
            //    {
            //        detectedVowel = "o";
            //        retVal = o;
            //    }
            //    else if( Mathf.Approximately( maxViseme, u ) )
            //    {
            //        detectedVowel = "u";
            //        retVal = u;
            //    }
            //}

            //_log.text = $"{detectedVowel.ToUpper()} : {maxViseme:F2} / {retVal:F2}";
            //_avatar.SetMouthParamY( retVal );
        }
    }
}
