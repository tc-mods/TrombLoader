using System.Collections.Generic;
using UnityEngine;

namespace TrombLoader.Data;

/// <summary>
/// Holds all the tromboners in a background
/// </summary>
public class BackgroundPuppetController : MonoBehaviour
{
    public List<Tromboner> Tromboners { get; }

    public BackgroundPuppetController()
    {
        Tromboners = new List<Tromboner>();
    }

    public void StartPuppetBob(float bob)
    {
        foreach (var tromboner in Tromboners)
        {
            tromboner.controller.startPuppetBob(bob);
        }
    }

    public void DoPuppetControl(float vp, float vibrato)
    {
        foreach (var tromboner in Tromboners)
        {
            tromboner.controller.doPuppetControl(vp);
            tromboner.controller.vibrato = vibrato;
        }
    }

    public void SetPuppetBreath(bool hasBreath)
    {
        foreach (var tromboner in Tromboners)
        {
            tromboner.controller.outofbreath = hasBreath;
            tromboner.controller.applyFaceTex();
        }
    }

    public void SetPuppetShake(bool shaking)
    {
        foreach (var tromboner in Tromboners)
        {
            tromboner.controller.shaking = shaking;
            tromboner.controller.applyFaceTex();
        }
    }
}
