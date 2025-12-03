using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Heal HP")]
public class HealHpReward : RewardData
{
    public int healAmount = 3;

    public override void ApplyEffect(GameObject player)
    {
        var health = player.GetComponentInChildren<PlayerHealth>();
        if (health != null)
        {
            health.Heal(healAmount);
        }
    }
}
