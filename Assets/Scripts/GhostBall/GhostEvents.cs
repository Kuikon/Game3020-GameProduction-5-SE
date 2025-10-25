using UnityEngine;
using System;

public static class GhostEvents
{
    public static event Action<GhostType, Vector3> OnGhostCaptured;

    public static void RaiseGhostCaptured(GhostType type, Vector3 position)
    {
        OnGhostCaptured?.Invoke(type, position);
    }
}
