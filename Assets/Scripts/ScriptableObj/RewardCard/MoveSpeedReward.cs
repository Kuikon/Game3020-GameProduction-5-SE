using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Move Speed Up")]
public class MoveSpeedReward : RewardData
{
    public float amount = 1f;

    public override void ApplyEffect(GameObject player)
    {
        var move = player.GetComponent<PlayerController>();
        if (move != null)
        {
            move.moveSpeed += amount;
         
        }
    }
}
