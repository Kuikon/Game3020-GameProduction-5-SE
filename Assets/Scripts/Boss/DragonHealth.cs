using System.Collections;
using UnityEngine;

public class DragonHealth : MonoBehaviour
{
    [Header("Dragon HP Settings")]
    public int maxHP = 20;
    public int currentHP = 20;
    [Header("Victory Settings")]
    [SerializeField] private GameObject confettiPrefab;
    [SerializeField] private float victoryDelay = 2f;
    private UIManager ui;
    Transform player;
    private void Start()
    {
        ui = UIManager.Instance;
        ui.CreateBar("DragonHP", maxHP);
        ui.UpdateBar("DragonHP", currentHP);
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        ui.UpdateBar("DragonHP", currentHP);
        Debug.Log($"🐉 Dragon HP: {currentHP}/{maxHP}");
        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("💀 Dragon Defeated!");
        StartCoroutine(HandleVictorySequence());
    }

    private IEnumerator HandleVictorySequence()
    {
        if (LineDraw.Instance != null)
            LineDraw.Instance.ForceStopDrawing();
        // 1. 全ゴースト削除
        foreach (var g in FindObjectsOfType<GhostBase>())
            Destroy(g.gameObject);
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.PlaySE(SESoundData.SE.Audience);
        Vector3 dragonPos = transform.position;

        // 3. ドラゴン縮小
        yield return StartCoroutine(ShrinkAndDisappear(1f));

        // 4. 紙吹雪を プレイヤー追従で生成！
        if (confettiPrefab != null)
        {
            var confetti = Instantiate(confettiPrefab, player.position, Quaternion.identity);
            confetti.transform.SetParent(player); 
        }
        
        // 5. 演出待ち
        yield return new WaitForSeconds(victoryDelay);

        // 6. 勝利シーンへ
        GameManager.Instance.Victory();
        Destroy(gameObject);
    }

    private IEnumerator ShrinkAndDisappear(float duration)
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            // 線形縮小
            transform.localScale = Vector3.Lerp(startScale, endScale, normalized);

            yield return null;
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        ui.UpdateBar("DragonHP", currentHP);
    }
}
