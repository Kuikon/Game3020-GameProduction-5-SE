using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Max HP Up")]
public class MaxHpReward : RewardData
{
    public int hpIncrease = 1;

    public override void ApplyEffect(GameObject player)
    {
        var health = player.GetComponentInChildren<PlayerHealth>();
        if (health != null)
        {
            health.IncreaseMaxHPByReward(hpIncrease);
        }
    }
}
