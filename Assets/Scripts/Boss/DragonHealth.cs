using UnityEngine;

public class DragonHealth : MonoBehaviour
{
    [Header("Dragon HP Settings")]
    public int maxHP = 20;
    public int currentHP = 20;

    private UIManager ui;

    private void Start()
    {
        ui = UIManager.Instance;

        // 🐉 ドラゴンHPバーを生成
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
        Destroy(gameObject);
        // ここでエフェクトやリワード処理を呼ぶ
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        ui.UpdateBar("DragonHP", currentHP);
    }
}
