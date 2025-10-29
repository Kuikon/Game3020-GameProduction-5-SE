using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Drawing;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Prefabs & Settings")]
    [SerializeField] private GameObject overheadTextPrefab; // 頭上テキストのプレハブ
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1.2f, 0); // テキストの位置

    [SerializeField] private Canvas mainCanvas;
    private Dictionary<GameObject, GameObject> activeTexts = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowOverheadText(GameObject target, string message, UnityEngine.Color color)
    {
        if (!activeTexts.ContainsKey(target))
        {
            GameObject textObj = Instantiate(overheadTextPrefab, mainCanvas.transform);
            activeTexts[target] = textObj;
        }

        var tmp = activeTexts[target].GetComponent<TMPro.TextMeshProUGUI>();
        tmp.text = message;
        tmp.color = color;

        UpdateTextPosition(target);

    }

    public void HideOverheadText(GameObject target)
    {
        if (activeTexts.ContainsKey(target) && activeTexts[target] != null)
        {
            Destroy(activeTexts[target]);
            activeTexts.Remove(target);
        }
    }
    private void UpdateTextPosition(GameObject target)
    {
        if (!activeTexts.ContainsKey(target) || activeTexts[target] == null) return;

        Vector3 worldPos = target.transform.position + textOffset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        activeTexts[target].transform.position = screenPos;
    }
    private void LateUpdate()
    {
        foreach (var kvp in activeTexts)
        {
            GameObject target = kvp.Key;
            GameObject textObj = kvp.Value;
            if (target == null || textObj == null) continue;

            UpdateTextPosition(target);
        }
    }
}
