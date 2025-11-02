using UnityEngine;

public enum GhostType { Normal, Quick, Tank, Suicide, Lucky }

[CreateAssetMenu(fileName = "GhostData", menuName = "ScriptableObjects/GhostData")]
public class GhostData : ScriptableObject
{
    [Header("Common Settings")]
    public GhostType type;
    public float walkSpeed = 2f;
    public float absorbTime = 10f;
    public int captureHitsNeeded = 3;
    public float changeDirectionTime = 2f;

    [Header("Visual Settings")]
    [Tooltip("見た目の基本色（SpriteRendererに反映される）")]
    public Color ghostColor = Color.white;

    [Tooltip("初期スケール倍率（1.0で等倍）")]
    public float baseScale = 1f;

    [Tooltip("上下ふわふわの振幅")]
    public float floatAmplitude = 0.2f;

    [Tooltip("上下ふわふわの速度")]
    public float floatSpeed = 2f;

    [Tooltip("点滅速度（0で点滅なし）")]
    public float blinkSpeed = 3f;

    [Tooltip("点滅の強さ（0〜1）")]
    public float blinkIntensity = 0.4f;

    [Header("Suicide Only")]
    public GameObject fireCirclePrefab;
    public float fireCircleLifetime = 3f;
}
