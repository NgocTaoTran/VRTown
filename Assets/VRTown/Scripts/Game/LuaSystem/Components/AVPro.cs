using System.Collections;
using System.Collections.Generic;
using ModestTree.Util;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Video;

namespace VRTown.Game.LuaSystem
{
    [Preserve]
    public class VideoPlayerProxy
    {
        public VideoPlayer videoPlayer;
        public VideoPlayerProxy(VideoPlayer videoPlayer)
        {
            this.videoPlayer = videoPlayer;
        }

        public bool play()
        {
            if (videoPlayer == null) return false;
            if (!videoPlayer.isPrepared) return false;
            videoPlayer.Play();
            return true;
        }

        public bool pause()
        {
            if (videoPlayer == null) return false;
            if (!videoPlayer.isPrepared) return false;
            videoPlayer.Pause();
            return true;
        }
        public void mute()
        {
            if (videoPlayer == null) return;
            if (!videoPlayer.isPrepared) return;
            videoPlayer.SetDirectAudioMute(0, true);
        }

        public bool toggle()
        {
            if (videoPlayer == null) return false;
            if (!videoPlayer.isPrepared) return false;
            if (videoPlayer.isPaused) videoPlayer.Play(); else videoPlayer.Pause();
            return true;
        }
    }


    [Preserve]
    public class AVProVideoPlayerProxy
    {
        public MediaPlayer mediaPlayer;
        public ApplyToMaterial applyToMaterial;
        public string url;

        //public VideoPlayer videoPlayer;
        public AVProVideoPlayerProxy(MediaPlayer mediaPlayer, ApplyToMaterial applyToMaterial, string url)
        {
            this.mediaPlayer = mediaPlayer;
            this.applyToMaterial = applyToMaterial;
            applyToMaterial.Player = mediaPlayer;
            this.url = url;
        }

        public void playVideo(string url)
        {
            Debug.LogErrorFormat("Playing video {0}", url);
            // mediaPlayer.m_VideoPath = url;
            // mediaPlayer.m_AutoOpen = true;
            // mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, url, true);
            Debug.LogErrorFormat("Playing video finishes {0}", url);
        }

        public bool play()
        {
            if (mediaPlayer == null) return false;
            playVideo(this.url);
            return true;
        }

        public bool pause()
        {
            if (mediaPlayer == null) return false;
            mediaPlayer.Control.Pause();
            return true;
        }

        public void mute(bool mute)
        {
            if (mediaPlayer == null) return;
            mediaPlayer.Control.MuteAudio(mute);
        }
        public bool isMute()
        {
            if (mediaPlayer == null) return true;
            return mediaPlayer.Control.IsMuted();
        }

        public bool toggle()
        {
            if (mediaPlayer == null) return false;
            if (mediaPlayer.Control.IsPaused()) mediaPlayer.Control.Play(); else mediaPlayer.Control.Pause();
            return true;
        }
    }
}
