using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHP = 10;
    public int currentHP = 10;

    [Header("References")]
    [SerializeField] private UIManager uiManager;       // メインUIマネージャー
    [SerializeField] private string hpBarName = "HP";   // 通常HPバー
    [SerializeField] private string miniHpBarName = "MiniHP"; // ミニHPバー名

    private void Start()
    {
        if (uiManager == null)
            uiManager = UIManager.Instance;

        // --- バー初期化 ---
        uiManager.SetupBar(hpBarName, maxHP);
        uiManager.SetupBar(miniHpBarName, maxHP);

        // --- 現在HPで反映 ---
        UpdateAllBars();
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        UpdateAllBars();

        GameManager.Instance.RegisterDamage();

        if (currentHP <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        UpdateAllBars();
    }

    private void UpdateAllBars()
    {
        if (uiManager == null) return;

        // メインHPバー更新
        uiManager.UpdateBar(hpBarName, currentHP);

        // ミニHPバー（存在する場合のみ）更新
        if (!string.IsNullOrEmpty(miniHpBarName))
            uiManager.UpdateBar(miniHpBarName, currentHP);
    }
}
