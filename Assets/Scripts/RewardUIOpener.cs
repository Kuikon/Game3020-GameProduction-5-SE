using UnityEngine;

public class RewardUIOpener : MonoBehaviour
{
   
    [SerializeField] private GameObject gorillaPanel;

    public void OpenChest()
    {
      
        RewardManager.Instance.ShowRandomRewards();

      
        if (gorillaPanel != null)
        {
            gorillaPanel.SetActive(false);
        }
    }
}
