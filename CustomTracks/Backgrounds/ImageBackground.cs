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


        // just gives a bit of wiggle room for images that aren't quite 16:9
        if (aspectRatio >= 1.7f && aspectRatio <= 1.78f) {
            camera.backgroundColor = Color.black;
        }
        else
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            var pixels = renderer.sprite.texture.GetPixels();
            int total = pixels.Length;

            float r = 0;
            float g = 0;
            float b = 0;
            
            int iterAmount;

            // sampling 200 pixels should probably be enough for this
            if (total < 200)
            {
                iterAmount = total;
            }
            else
            {
                float fraction = total * 0.005f;
                iterAmount = (int)fraction;
            }

            for (int i = 0; i < total; i += iterAmount)
            {
                var color = pixels[i];
                r += color.r;
                g += color.g;
                b += color.b;
            }

            camera.backgroundColor = new Color(r / 200, g / 200, b / 200);
        }

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