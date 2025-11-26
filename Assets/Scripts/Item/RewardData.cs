using UnityEngine;

public abstract class RewardData : ScriptableObject
{
    public string rewardName;
    public Sprite icon;
    public string description;

    // Pass the GameObject player to access each component
    public abstract void ApplyEffect(GameObject player);
}
