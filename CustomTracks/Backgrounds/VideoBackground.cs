using System.Collections.Generic;
using BaboonAPI.Hooks.Tracks;
using UnityEngine;
using UnityEngine.Video;

namespace TrombLoader.CustomTracks.Backgrounds;

public class VideoBackground : HijackedBackground
{
    private readonly string _videoPath;
    private VideoPlayer _videoPlayer;

    public VideoBackground(string videoPath)
    {
        _videoPath = videoPath;
    }
    
    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        DisableParts(bg);

        var bgplane = bg.transform.GetChild(0);
        var pc = bgplane.GetChild(0);

        bgplane.gameObject.SetActive(true);
        pc.gameObject.SetActive(true);
        pc.GetComponent<SpriteRenderer>().color = Color.black;

        _videoPlayer = pc.GetComponent<VideoPlayer>() ?? pc.gameObject.AddComponent<VideoPlayer>();

        _videoPlayer.url = _videoPath;
        _videoPlayer.isLooping = true;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.skipOnDrop = true;
        _videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        _videoPlayer.targetCamera = bg.transform.GetComponent<Camera>();

        _videoPlayer.enabled = true;
        _videoPlayer.Pause();

        controller.StartCoroutine(PlayVideoDelayed(_videoPlayer).GetEnumerator());
    }

    public override bool CanResume => true;

    public override void OnPause(PauseContext ctx)
    {
        _videoPlayer.Pause();
    }

    public override void OnResume(PauseContext ctx)
    {
        _videoPlayer.Play();
    }

    public static IEnumerable<YieldInstruction> PlayVideoDelayed(VideoPlayer videoPlayer)
    {
        yield return new WaitForSeconds(2.4f);

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }
}