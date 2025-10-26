using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BoxManager : MonoBehaviour
{
    public static BoxManager Instance;

    [Header("Score UI")]
    public Text scoreText;

    private int score = 0;
    private Dictionary<GhostType, UIBox> boxDict = new();
    private Dictionary<GhostType, int> typeCounts = new();
    void Awake()
    {
        Instance = this;

        foreach (var box in FindObjectsOfType<UIBox>())
        {
            if (!boxDict.ContainsKey(box.boxType))
            {
                boxDict.Add(box.boxType, box);
                Debug.Log($"[BoxManager] Registered box: {box.boxType}");
            }
        }
        foreach (GhostType t in System.Enum.GetValues(typeof(GhostType)))
            typeCounts[t] = 0;
    }


    public void ProcessDrop(BallController ball, UIBox targetBox)
    {
        GhostType type = ball.type;
        if (boxDict.TryGetValue(ball.type, out var correctBox) && targetBox == correctBox)
        {
           
            Debug.Log($"✅ Correct: {ball.type} → {targetBox.name}");

            typeCounts[type]++;
            targetBox.AddBall();
            Destroy(ball.gameObject);
        }
        else
        {
            Debug.Log($"❌ Wrong: {ball.type} → {targetBox.name}");
            score -= 5;
            UpdateScore();

            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            rb.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
        }
    }

    private void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

}
