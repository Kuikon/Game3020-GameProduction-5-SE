using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCardUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image highlight; 

    private RewardData reward;

    public void Setup(RewardData data)
    {
        reward = data;
        icon.sprite = data.icon;
        nameText.text = data.rewardName;
        descText.text = data.description;
        if (highlight != null)
            highlight.color = new Color(1f, 1f, 1f, 0f);

        transform.localScale = Vector3.one;
    }
    public void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.one;
        StartCoroutine(ScaleInRoutine());
    }
    private System.Collections.IEnumerator ScaleInRoutine()
    {
        transform.localScale = Vector3.zero;
        float t = 0f;
        float duration = 0.2f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; 
            float f = t / duration;
            transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, f);
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
    public void PlaySelectEffect(System.Action onComplete)
    {
        if (highlight != null)
            highlight.color = new Color(1f, 1f, 1f, 1f);
        transform.localScale = Vector3.one * 1.2f;
        onComplete?.Invoke();
    }

    public void OnClick()
    {

        PlaySelectEffect(() =>
        {
            if (RewardManager.Instance != null)
            {
                SoundManager.Instance.PlaySE(SESoundData.SE.RewardSelect);
                RewardManager.Instance.SelectReward(reward);
            }
            else
            {
                Debug.LogError("RewardManager.Instance is not found");
            }
        });
    }
}
