using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Video;

namespace VRTown.Game
{
    public class GVideoScreen : MonoBehaviour, ISerializationCallbackReceiver
    {
        [Header("Reference Data")]
        public SunflotVideoSO Data;

        [Header("Elements")]
        [SerializeField] TMP_Text _textTitle;
        [SerializeField] TMP_Text _textDescription;
        [SerializeField] VideoPlayer _videoPlayer;
        [SerializeField] SpriteRenderer _play;

        public bool IsPlay = false;

        void Start()
        {
            if (Data != null)
            {
                PauseVideo();
            }
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            if (Data != null)
            {
                _textTitle.text = Data.Title;
                _textDescription.text = Data.Message;
                _videoPlayer.url = Data.URL;
            }
        }
        public void PlayVideo()
        {
            _videoPlayer.Play();
            _play.gameObject.SetActive(false);
        }
        public void PauseVideo()
        {
            _videoPlayer.Pause();
            _play.gameObject.SetActive(true);
        }
    }
}