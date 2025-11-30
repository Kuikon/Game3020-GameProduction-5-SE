using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHP = 10;
    public int currentHP = 10;

    [SerializeField] private string hpBarName = "HP";
    [SerializeField] private string miniHpBarName = "MiniHP";
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
   
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateHPBars();
    }
    private void CreateHPBars()
    {
        if (UIManager.Instance == null) return;

        UIManager.Instance.CreateBar(hpBarName, maxHP);
        UIManager.Instance.CreateBar(miniHpBarName, maxHP);
        UpdateAllBars();
    }
    private void UpdateAllBars()
    {
        UIManager.Instance.UpdateBar(hpBarName, currentHP);
        UIManager.Instance.UpdateBar(miniHpBarName, currentHP);
        UIManager.Instance.UpdateBarAndCounter(hpBarName, currentHP, maxHP);
        UIManager.Instance.UpdateBarAndCounter(miniHpBarName, currentHP, maxHP);
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

        UIManager.Instance.CreateBar(hpBarName, maxHP);
        UIManager.Instance.CreateBar(miniHpBarName, maxHP);

        UpdateAllBars();

        Debug.Log($"💪 Max HP increased to {maxHP}!");
    }
    public void IncreaseMaxHPByReward(int addAmount)
    {
        maxHP += addAmount;
        currentHP = Mathf.Min(currentHP, maxHP);
        CreateHPBars();

        Debug.Log($"💪 HP increased! maxHP={maxHP}, currentHP={currentHP}");
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        UpdateAllBars();
    }
}
