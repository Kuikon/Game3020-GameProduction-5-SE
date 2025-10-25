using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BoxManager : MonoBehaviour
{
    public static BoxManager Instance;

    [Header("Score UI")]
    public Text scoreText;
    private int score = 0;

    void Awake()
    {
        Instance = this;
    }

    public void ProcessDrop(BallController ball, UIBox box)
    {
        if (ball.type == box.boxType)
        {
            // ✅ Correct
            score += 10;
            UpdateScore();
            StartCoroutine(ball.MoveToBoxCenter(box.RectTransform.position));
            Debug.Log($"✅ Correct! {ball.type} → {box.boxType}");
        }
        else
        {
            // ❌ Incorrect
            score -= 5;
            UpdateScore();

            Rigidbody2D rb = ball.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(Vector2.up * 3f, ForceMode2D.Impulse);
            Debug.Log($"❌ Wrong! {ball.type} → {box.boxType}");
        }
    }

    private void UpdateScore()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}
