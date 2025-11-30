using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Pickup Range Up")]
public class PickupRangeUpReward : RewardData
{
    public float radiusIncrease = 0.5f;

    public override void ApplyEffect(GameObject player)
    {
        var radius = player.GetComponentInChildren<ExpPickupRadius>();

        if (radius != null)
        {
            radius.AddRadius(radiusIncrease);
            Debug.Log($"🟢 upgrade +{radiusIncrease}");
        }
        else
        {
            Debug.LogError("❌ ExpPickupRadius ");
        }
    }
}
