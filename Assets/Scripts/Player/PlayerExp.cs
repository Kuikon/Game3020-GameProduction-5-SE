using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int level = 1;

    public int currentExp = 0;
    public int expToNextLevel = 5;

    public void AddExp(int amount)
    {
        currentExp += amount;
        if (currentExp >= expToNextLevel)
        {
            level++;
            expToNextLevel = Mathf.CeilToInt(expToNextLevel * 1.3f);
            currentExp = 0;
            RewardManager.Instance.ShowRandomRewards();
            SoundManager.Instance.PlaySE(SESoundData.SE.LevelUp);
        }
        UIManager.Instance.UpdateExpBar(currentExp, expToNextLevel, level);
    }
}
