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
            float before = move.moveSpeed;
            move.moveSpeed += amount;
            Debug.Log($"🔥 moveSpeed: {before} → {move.moveSpeed}");

        }
    }
}
