using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Max HP Up")]
public class MaxHpReward : RewardData
{
    public int hpIncrease = 5;

    public override void ApplyEffect(GameObject player)
    {
        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.maxHP += hpIncrease;
            hp.currentHP += hpIncrease; // ‘‚¦‚½•ª‰ñ•œ‚³‚¹‚é
            Debug.Log($"Å‘åHP +{hpIncrease}");
        }
    }
}
