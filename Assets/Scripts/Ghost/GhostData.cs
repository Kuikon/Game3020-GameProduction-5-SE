using UnityEngine;

public enum GhostType
{
    Normal = 0,
    Quick = 1,
    Tank = 2,
    Suicide = 3,
    Lucky = 4
}

[CreateAssetMenu(fileName = "GhostData", menuName = "ScriptableObjects/GhostData")]
public class GhostData : ScriptableObject
{
    [Header("Common Settings")]
    public GhostType type;
    public float walkSpeed = 2f;
    public float absorbTime = 10f;
    public int captureHitsNeeded = 3;
    public float changeDirectionTime = 2f;
    public int dropAmount = 3;   
    public int expPerDrop = 1;   
    [Header("Visual Settings")]
    public Color ghostColor = Color.white;

    public float baseScale = 1f;

    public float floatAmplitude = 0.2f;

    public float floatSpeed = 2f;

    public float blinkSpeed = 3f;

    public float blinkIntensity = 0.4f;

    public GameObject fireCirclePrefab;
    public float fireCircleLifetime = 3f;
}
