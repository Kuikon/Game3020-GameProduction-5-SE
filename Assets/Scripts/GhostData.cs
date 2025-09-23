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


    [Header("Suicide Only")]
    public GameObject fireCirclePrefab;
    public float fireCircleLifetime = 3f;

    [Header("Lucky Only")]
    public float luckyChargeTime = 15f;
}
