using UnityEngine;

[CreateAssetMenu(menuName = "Rewards/Line Max Length Up")]
public class LineLengthReward : RewardData
{
    public float extraLength = 2f;

    public override void ApplyEffect(GameObject player)
    {
        if (LineDraw.Instance == null)
        {
            Debug.LogError("LineDraw Instance not found!");
            return;
        }

        LineDraw.Instance.maxLineLength += extraLength;

        Debug.Log($"Max line length increased by {extraLength}! New max = {LineDraw.Instance.maxLineLength}");
    }
}
