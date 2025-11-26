using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Line Max Length Up")]
public class LineLengthReward : RewardData
{
    public float extraLength = 2f;

    public override void ApplyEffect(GameObject player)
    {
        var line = player.GetComponent<LineDraw>();
        if (line != null)
        {
            line.maxLineLength += extraLength;
            Debug.Log($"Max line length +{extraLength}");
        }
    }
}
