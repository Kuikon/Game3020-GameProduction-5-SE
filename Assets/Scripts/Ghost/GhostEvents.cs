using UnityEngine;
using System;
using System.Collections.Generic;

public static class GhostEvents
{
    public static event Action<GhostType, Vector3> OnGhostCaptured;
    public static event System.Action<List<GameObject>> OnGravesCaptured;
    public static void RaiseGhostCaptured(GhostType type, Vector3 position)
    {
        OnGhostCaptured?.Invoke(type, position);

    }
    public static void RaiseGravesCaptured(List<GameObject> graves)
         => OnGravesCaptured?.Invoke(graves);
}