using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Drawing;
using Color = UnityEngine.Color;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Prefabs & Settings")]
    [SerializeField] private GameObject overheadTextPrefab;
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1.2f, 0);
    [SerializeField] private Canvas mainCanvas;

    [Header("HP Bar Settings")]
    [SerializeField] private GameObject hpBlockPrefab;          
    [SerializeField] private Transform hpBlockContainer;        
    [SerializeField] private int maxHP = 10;                  
    [SerializeField] private Color hpActiveColor = Color.red;   
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.3f); 
    [SerializeField] private Vector2 hpBlockSize = new Vector2(15f, 40f);
    [Header("Enemy  Bar Settings")]
    [SerializeField] private GameObject enemyBlockPrefab;
    [SerializeField] private Transform enemyBlockContainer;
    [SerializeField] private int maxEnemy = 10;
    [SerializeField] private Color enemyActiveColor = Color.red;
    [SerializeField] private Vector2 enemyBlockSize = new Vector2(15f, 40f);
    private List<Image> hpBlocks = new();
    private List<Image> enemyBlocks = new();
     private int currentEnemyCount = 0;
    private Dictionary<GameObject, GameObject> activeTexts = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        CreateHPBlocks();
        CreateEnemyBlocks();
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
    private void CreateHPBlocks()
    {
        foreach (Transform child in hpBlockContainer)
            Destroy(child.gameObject);
        hpBlocks.Clear();

        for (int i = 0; i < maxHP; i++)
        {
            GameObject block = Instantiate(hpBlockPrefab, hpBlockContainer);
            Image img = block.GetComponent<Image>();
            img.color = hpActiveColor;
            RectTransform rt = block.GetComponent<RectTransform>();
            rt.sizeDelta = hpBlockSize;
            hpBlocks.Add(img);
        }
    }
    private void CreateEnemyBlocks()
    {
        foreach (Transform child in enemyBlockContainer)
            Destroy(child.gameObject);
        enemyBlocks.Clear();

        for (int i = 0; i < maxEnemy; i++)
        {
            GameObject enemyBlock = Instantiate(enemyBlockPrefab, enemyBlockContainer);
            Image img = enemyBlock.GetComponent<Image>();
            img.color = inactiveColor;
            RectTransform rt = enemyBlock.GetComponent<RectTransform>();
            rt.sizeDelta = enemyBlockSize;
            enemyBlocks.Add(img);
        }
        currentEnemyCount = 0;
    }
    public void UpdateHP(int currentHP)
    {
        for (int i = 0; i < hpBlocks.Count; i++)
        {
            hpBlocks[i].color = (i < currentHP) ? hpActiveColor : inactiveColor;
        }
    }
    public void AddEnemyBlock()
    {
        if (currentEnemyCount >= maxEnemy)
        {
            Debug.Log("🧱 Enemy block is already full!");
            return;
        }

        enemyBlocks[currentEnemyCount].color = enemyActiveColor;
        currentEnemyCount++;

        Debug.Log($"👹 Enemy captured! Total: {currentEnemyCount}/{maxEnemy}");
    }
    public void ResetEnemyBlocks()
    {
        foreach (var block in enemyBlocks)
        {
            block.color = inactiveColor;
        }
        currentEnemyCount = 0;
    }
    public void SetupHPBar(int newMaxHP)
    {
        maxHP = newMaxHP; 
        CreateHPBlocks(); 
    }
}
