using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Light Recovery Slow")]
public class LightRecoveryReward : RewardData
{
    public float slowMultiplier = 1.5f;

    public override void ApplyEffect(GameObject player)
    {
        Debug.Log("LightRecoveryReward ApplyEffect CALLED");

        var light = player.GetComponentInChildren<Light2DRadiusController>();
        if (light != null)
        {
            float newValue = light.flashDuration * slowMultiplier;
            light.SetFlashDuration(newValue);
        }
        else
        {
            Debug.Log("Player has NO Light2DRadiusController");
        }
    }

}
