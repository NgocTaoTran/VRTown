using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;
using UnityEngine;

public class MyVideoPlayer : MonoBehaviour
{
    public string url;
    public Renderer view;
    public RenderHeads.Media.AVProVideo.MediaPlayer mediaPlayer;
    public RenderHeads.Media.AVProVideo.ApplyToMaterial applyToMaterial;
    public Material mappingMaterial;
    public bool autoPlay;
    void Start()
    {
        // var mat = Instantiate(mappingMaterial);
        // applyToMaterial.Material = mat;
        // view.sharedMaterial = mat;
        // applyToMaterial.Player = mediaPlayer;
        applyToMaterial.Material = view.sharedMaterial;
        if (autoPlay)
        {
            playVideo(url);
        }
    }
    public void playVideo(string url)
    {
        this.url = url;
        // mediaPlayer.m_VideoPath = url;
        // mediaPlayer.m_AutoOpen = true;
        // mediaPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.AbsolutePathOrURL, url, true);
    }
}
