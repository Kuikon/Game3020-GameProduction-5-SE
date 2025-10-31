using UnityEngine;
using TMPro;

public class ResultUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ghostStatsText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI sortFailText;

    private void Start()
    {
        DisplayResults();
    }

    private void DisplayResults()
    {
        var gm = GameManager.Instance;

        string ghostText = "";
        foreach (var kvp in gm.capturedGhosts)
        {
            ghostText += $"{kvp.Key}: {kvp.Value}\n";
        }

        ghostStatsText.text = " Ghosts Captured:\n" + ghostText;
        damageText.text = $" Damage Taken: {gm.damageTaken}";
        sortFailText.text = $" Sort Fails: {gm.sortFails}";
    }
}
