using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHP = 10;
    public int currentHP = 10;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private string hpBarName = "HP";
    [SerializeField] private string miniHpBarName = "MiniHP";

    private void Start()
    {
        if (uiManager == null)
            uiManager = UIManager.Instance;

        // --- バー初期化（動的生成） ---
        uiManager.CreateBar(hpBarName, maxHP);
        uiManager.CreateBar(miniHpBarName, maxHP);

        // --- 現在HPを反映 ---
        UpdateAllBars();
    }

    private void UpdateAllBars()
    {
        uiManager.UpdateBar(hpBarName, currentHP);
        uiManager.UpdateBar(miniHpBarName, currentHP);
    }

    public void TakeDamage(int damage)
    {

        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        UpdateAllBars();

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
}
