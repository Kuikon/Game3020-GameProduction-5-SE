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
        uiManager.CreateBar(hpBarName, maxHP);
        uiManager.CreateBar(miniHpBarName, maxHP);

        UpdateAllBars();
    }

    private void UpdateAllBars()
    {
        uiManager.UpdateBar(hpBarName, currentHP);
        uiManager.UpdateBar(miniHpBarName, currentHP);
        uiManager.UpdateBarAndCounter(hpBarName, currentHP, maxHP);
        uiManager.UpdateBarAndCounter(miniHpBarName, currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {

        currentHP = Mathf.Max(0, currentHP - damage);

        UpdateAllBars();

        if (currentHP <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }
    public void IncreaseMaxHP(int amount)
    {
        maxHP += amount;
        currentHP = maxHP;

        uiManager.CreateBar(hpBarName, maxHP);
        uiManager.CreateBar(miniHpBarName, maxHP);

        UpdateAllBars();

        Debug.Log($"💪 Max HP increased to {maxHP}!");
    }
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        UpdateAllBars();
    }
}
