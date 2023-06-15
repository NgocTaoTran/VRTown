using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

namespace VRTown.Game.Utils
{
    public class Sound3D : MonoBehaviour
    {
        Transform _tfTarget = null;
        MediaPlayer _media = null;

        public void SetupMedia(MediaPlayer media)
        {
            _media = media;
            // _media.m_Loop = true;
            _tfTarget = Camera.main.transform;
        }

        // Update is called once per frame
        void Update()
        {
            if (_media != null && _tfTarget != null)
            {
                var ratio = Vector3.Distance(_media.transform.position, _tfTarget.position) / 100f;
                _media.Control.SetVolume(Mathf.Lerp(1, 0f, ratio));
            }
        }
    }
}