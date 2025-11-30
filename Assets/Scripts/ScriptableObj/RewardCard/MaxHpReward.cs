using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Max HP Up")]
public class MaxHpReward : RewardData
{
    public int hpIncrease = 5;

    public override void ApplyEffect(GameObject player)
    {
        var health = player.GetComponentInChildren<PlayerHealth>();
        if (health != null)
        {
            health.IncreaseMaxHPByReward(3);
        }
    }
}
