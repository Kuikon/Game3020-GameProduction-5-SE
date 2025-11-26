using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    [Header("UI")]
    public GameObject rewardPanel;
    public RewardCardUI[] cardUIs;

    [Header("Rewards")]
    public RewardData[] allRewards;

    private GameObject player;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rewardPanel.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private RewardData[] GetUniqueRandomRewards(int count)
    {
        List<RewardData> pool = new List<RewardData>(allRewards);
        RewardData[] result = new RewardData[count];

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result[i] = pool[index];
            pool.RemoveAt(index);
        }
        return result;
    }

    public void ShowRandomRewards()
    {
        rewardPanel.SetActive(true);
        Time.timeScale = 0f;

        RewardData[] selected = GetUniqueRandomRewards(3);

        for (int i = 0; i < 3; i++)
        {
            cardUIs[i].Setup(selected[i]);
            cardUIs[i].PlaySpawnAnimation();
        }
    }

    public void SelectReward(RewardData reward)
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ");
                return;
            }
        }
        reward.ApplyEffect(player);

        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
