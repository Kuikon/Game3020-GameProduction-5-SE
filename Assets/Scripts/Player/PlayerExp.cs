using UnityEngine;

public class PlayerExp : MonoBehaviour
{
    public int level = 1;

    public int currentExp = 0;      // 現在のEXP（整数）
    public int expToNextLevel = 5;  // 次レベルまでの必要EXP（整数）

    public void AddExp(int amount)
    {
        currentExp += amount;

        // レベルアップ処理（すべて整数）
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            level++;

            // 必要経験値を整数として上昇
            expToNextLevel = Mathf.CeilToInt(expToNextLevel * 1.3f);
        }

        // UIバー更新（あなたのUIManager用）
        //UIManager.Instance?.UpdateBar("EXP", currentExp, expToNextLevel);
    }

    // UIのブロックバーが割合で計算したいときに使う
    public float GetExpRatio()
    {
        return (float)currentExp / expToNextLevel;
    }
}
