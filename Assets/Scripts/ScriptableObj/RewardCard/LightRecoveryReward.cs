using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Light Recovery Slow")]
public class LightRecoveryReward : RewardData
{
    public float slowMultiplier = 0.8f;

    public override void ApplyEffect(GameObject player)
    {
        var light = player.GetComponent<Light2DRadiusController>();
        if (light != null)
        {
            light.fadeSpeed *= slowMultiplier;
       
        }
    }
}
