using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHP = 10;
    public int currentHP = 10;

    [SerializeField] private　UIManager hpUI;

    private void Start()
    {
        hpUI.SetupHPBar(maxHP);
        hpUI.UpdateHP(currentHP);
    }

    public void TakeDamage(int damage)
    {
        currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);
        hpUI.UpdateHP(currentHP);
        GameManager.Instance.RegisterDamage();
        if (currentHP <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }
    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        hpUI.UpdateHP(currentHP);
    }
}
