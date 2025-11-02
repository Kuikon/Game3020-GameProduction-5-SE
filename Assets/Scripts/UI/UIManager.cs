using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Drawing;
using Color = UnityEngine.Color;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public class GhostUISlot
    {
        public GhostType type;
        public Image icon;
        public TextMeshProUGUI countText;
        public Transform stackParent;
    }
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
    [Header("Bullet Slots")]
    [SerializeField] private List<GhostUISlot> ghostSlots;
    [SerializeField] private Color inactiveSlotColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] private Color activeSlotColor = Color.white;
    [SerializeField] private GameObject stackBlockPrefab;
    [SerializeField] private float blockSpacing = 20f;
    private List<Image> hpBlocks = new();
    private List<Image> enemyBlocks = new();
     private int currentEnemyCount = 0;
    private const int MaxVisualBlocks = 10;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        CreateHPBlocks();
        CreateEnemyBlocks();
    }


    private void OnEnable()
    {
        GhostEvents.OnGhostCaptured += OnGhostCaptured;
    }

    private void OnDisable()
    {
        GhostEvents.OnGhostCaptured -= OnGhostCaptured;
    }

    private void OnGhostCaptured(GhostType type, Vector3 pos)
    {
        // LuckyはNormal扱い
        if (type == GhostType.Lucky)
            type = GhostType.Normal;

        // GameManagerでカウント済みの値を参照
        //int count = GameManager.Instance.capturedGhosts[type] + 1;
        //UpdateSlot(type, count);
    }
    private void Start()
    {
        InitializeSlots();
    }
    private void InitializeSlots()
    {
        foreach (var slot in ghostSlots)
        {
            // --- アイコン色をゴーストタイプ別に設定 ---
            if (slot.icon != null)
            {
                // GhostBaseの定義したタイプごとの色を使用
                Color baseColor = GhostBase.GetColorByType(slot.type);

                // 未捕獲状態では半透明・暗めに
                slot.icon.color = baseColor * 0.5f;
            }

            // --- テキストを初期化 ---
            if (slot.countText != null)
            {
                slot.countText.text = "0";
            }

            // --- 既存ブロックを削除 ---
            foreach (Transform child in slot.stackParent)
            {
                Destroy(child.gameObject);
            }

            // --- ブロックを10個生成して灰色に設定 ---
            for (int i = 0; i < MaxVisualBlocks; i++)
            {
                GameObject block = Instantiate(stackBlockPrefab, slot.stackParent);
                RectTransform rt = block.GetComponent<RectTransform>();

                if (rt != null)
                {
                    // blockSpacingで縦に積む
                    rt.anchoredPosition = new Vector2(0, i * blockSpacing);
                }

                Image img = block.GetComponent<Image>();
                if (img != null)
                {
                    img.color = inactiveColor; // 初期状態は灰色
                }
            }
        }

        Debug.Log("🎨 All ghost slots initialized (10 gray blocks each, type-based icons set).");
    }

    public void UpdateSlot(GhostType type, int count)
    {
        var slot = ghostSlots.Find(s => s.type == type);
        if (slot == null) return;

        // --- アイコン色 ---
        if (slot.icon != null)
        {
            Color baseColor = GhostBase.GetColorByType(slot.type);
            slot.icon.color = (count > 0) ? baseColor : baseColor * 0.5f;
        }

        // --- テキスト更新 ---
        // 10体未満では非表示（空欄）
        if (count < MaxVisualBlocks)
        {
            slot.countText.text = ""; // テキスト非表示
        }
        else
        {
            slot.countText.text = count.ToString() + "+"; // 10体以上で表示
        }

        // --- ブロック生成（常に10個固定） ---
        int blockCount = slot.stackParent.childCount;
        if (blockCount < MaxVisualBlocks)
        {
            for (int i = blockCount; i < MaxVisualBlocks; i++)
            {
                GameObject block = Instantiate(stackBlockPrefab, slot.stackParent);
                RectTransform rt = block.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(0, i * blockSpacing);
                }

                Image img = block.GetComponent<Image>();
                if (img != null)
                    img.color = inactiveColor;
            }
        }

        // --- ブロック色変更（最大10まで） ---
        int visibleBlocks = Mathf.Min(count, MaxVisualBlocks);

        for (int i = 0; i < MaxVisualBlocks; i++)
        {
            Transform child = slot.stackParent.GetChild(i);
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                if (i < visibleBlocks)
                    img.color = activeSlotColor; // 捕獲済み
                else
                    img.color = inactiveColor;   // 未捕獲
            }
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
