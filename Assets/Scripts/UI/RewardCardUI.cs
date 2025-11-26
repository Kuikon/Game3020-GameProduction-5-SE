using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardCardUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image highlight; // 光る用イメージ（通常は非表示）

    private RewardData reward;

    public void Setup(RewardData data)
    {
        reward = data;
        icon.sprite = data.icon;
        nameText.text = data.rewardName;
        descText.text = data.description;

        // 最初は光オフ & スケール等倍
        if (highlight != null)
            highlight.color = new Color(1f, 1f, 1f, 0f);

        transform.localScale = Vector3.one;
    }

    // 生成時の演出（DOTween 無し版）
    // 今は「そのまま表示」にしておく（安定重視）
    public void PlaySpawnAnimation()
    {
        // アニメいらなければ何もしなくてOK
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
            t += Time.unscaledDeltaTime; // TimeScale=0でも動かしたいならこっち
            float f = t / duration;
            transform.localScale = Vector3.one * Mathf.SmoothStep(0f, 1f, f);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
    // 選択時の演出（DOTween 無し版）
    public void PlaySelectEffect(System.Action onComplete)
    {
        // 光る
        if (highlight != null)
            highlight.color = new Color(1f, 1f, 1f, 1f);

        // ちょっとだけ大きくしてみる
        transform.localScale = Vector3.one * 1.2f;

        // すぐコールバック実行
        onComplete?.Invoke();
    }

    public void OnClick()
    {
        // reward がセットされていない事故防止
        if (reward == null)
        {
            Debug.LogWarning("RewardCardUI: reward がセットされていません");
            return;
        }

        PlaySelectEffect(() =>
        {
            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.SelectReward(reward);
            }
            else
            {
                Debug.LogError("RewardManager.Instance が見つかりません");
            }
        });
    }
}
