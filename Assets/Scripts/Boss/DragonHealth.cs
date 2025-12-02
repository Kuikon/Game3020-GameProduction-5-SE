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

    private void Start()
    {
        ui = UIManager.Instance;
        ui.CreateBar("DragonHP", maxHP);
        ui.UpdateBar("DragonHP", currentHP);
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
        // 1. すべてのゴースト消去
        GhostBase[] ghosts = FindObjectsOfType<GhostBase>();
        foreach (var g in ghosts)
        {
            Destroy(g.gameObject);
        }

        // 2. 花吹雪エフェクト再生
        if (confettiPrefab != null)
            Instantiate(confettiPrefab, transform.position, Quaternion.identity);

        // 3. ボス自身を消す
        Destroy(gameObject);

        // 4. エフェクト待ち
        yield return new WaitForSeconds(victoryDelay);

        // 5. Victory へ遷移
        GameManager.Instance.Victory();
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        ui.UpdateBar("DragonHP", currentHP);
    }
}
