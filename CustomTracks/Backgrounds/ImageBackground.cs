using BaboonAPI.Hooks.Tracks;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.CustomTracks.Backgrounds;

public class ImageBackground : HijackedBackground
{
    private readonly string _imagePath;

    public ImageBackground(string imagePath)
    {
        _imagePath = imagePath;
    }

    public override void SetUpBackground(BGController controller, GameObject bg)
    {
        DisableParts(bg);

        var camera = bg.GetComponent<Camera>();
        var bgplane = bg.transform.GetChild(0);
        var renderer = bgplane.GetChild(0).GetComponent<SpriteRenderer>();
        renderer.sprite = ImageHelper.LoadSpriteFromFile(_imagePath);

        float scaleFactor;
        float aspectRatio = renderer.sprite.rect.width / renderer.sprite.rect.height;

        if (aspectRatio > 1.7778f)
        {
            float spriteWidth = renderer.sprite.rect.width / renderer.sprite.pixelsPerUnit;
            scaleFactor = 17.778f / spriteWidth;
        }
        else
        {
            float spriteHeight = renderer.sprite.rect.height / renderer.sprite.pixelsPerUnit;
            scaleFactor = 10 / spriteHeight;
        }

        renderer.transform.localScale = Vector3.one * scaleFactor;

        camera.backgroundColor = Color.black;

        bgplane.gameObject.SetActive(true);
        renderer.gameObject.SetActive(true);
    }

    public override bool CanResume => true;

    public override void OnPause(PauseContext ctx)
    {
        // muffin
    }

    public override void OnResume(PauseContext ctx)
    {
        // muffin
    }
}