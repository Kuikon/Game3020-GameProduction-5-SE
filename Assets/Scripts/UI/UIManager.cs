using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Drawing;
using Color = UnityEngine.Color;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    //=============================================
    // 🟣 Ghost Slot UI
    //=============================================
    [System.Serializable]
    public class GhostUISlot
    {
        public GhostType type;
        public Image slotBackground;
        public Image icon;
        public TextMeshProUGUI countText;
        public Transform stackParent;
    }

    //=============================================
    // 🔴 共通バークラス
    //=============================================
    [System.Serializable]
    public class BarUI
    {
        public string barName;                  // "HP", "MiniHP", "Enemy"など
        public GameObject blockPrefab;          // ブロックプレハブ
        public Transform container;             // 配置先
        public int maxBlocks = 10;              // 最大ブロック数
        public Color activeColor = Color.red;   // 有効色
        public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);
        public Vector2 blockSize = new Vector2(15f, 40f);
        [HideInInspector] public List<Image> blocks = new();

        public void CreateBlocks()
        {
            if (container == null || blockPrefab == null) return;

            foreach (Transform child in container)
                GameObject.Destroy(child.gameObject);

            blocks.Clear();

            for (int i = 0; i < maxBlocks; i++)
            {
                GameObject block = GameObject.Instantiate(blockPrefab, container);
                Image img = block.GetComponent<Image>();
                RectTransform rt = block.GetComponent<RectTransform>();
                if (img != null) img.color = activeColor;
                if (rt != null) rt.sizeDelta = blockSize;
                blocks.Add(img);
            }
        }

        public void UpdateBlocks(int current)
        {
            if (blocks == null || blocks.Count == 0) return;
            for (int i = 0; i < blocks.Count; i++)
                blocks[i].color = (i < current) ? activeColor : inactiveColor;
        }
    }

    //=============================================
    // 🎨 Serialized Fields
    //=============================================
    [Header("Bar Settings")]
    [SerializeField] private List<BarUI> bars = new();

    [Header("Bullet Slots")]
    [SerializeField] private List<GhostUISlot> ghostSlots;
    [SerializeField] private Color inactiveSlotColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] private Color activeSlotColor = Color.white;
    [SerializeField] private GameObject stackBlockPrefab;
    [SerializeField] private float blockSpacing = 20f;
    [SerializeField] private Color focusColor = new Color(1f, 1f, 0.3f, 1f);
    [SerializeField] private Color normalSlotColor = new Color(1f, 1f, 1f, 0.4f);

    [Header("UI Move Settings")]
    [SerializeField] private RectTransform hpAndEnemyGroup;
    [SerializeField] private RectTransform bulletSlotsGroup;
    [SerializeField] private Vector3 hpHiddenPos = new Vector3(0, 200f, 0);
    [SerializeField] private Vector3 hpVisiblePos = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 bulletHiddenPos = new Vector3(0, -300f, 0);
    [SerializeField] private Vector3 bulletVisiblePos = new Vector3(0, 0, 0);
    [SerializeField] private float moveSpeed = 10f;

    [SerializeField] private RectTransform miniHpUI;
    [SerializeField] private Vector3 miniHpVisiblePos = new Vector3(-50f, 50f, 0);
    [SerializeField] private Vector3 miniHpHiddenPos = new Vector3(-50f, -200f, 0);

    //=============================================
    // 🧠 内部変数
    //=============================================
    private const int MaxVisualBlocks = 10;
    private Dictionary<GhostType, int> bulletStock = new();
    private Dictionary<string, BarUI> barDict = new();
    private int currentEnemyCount = 0;

    //=============================================
    // ⚙️ Awake / Start / Update
    //=============================================
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // すべてのバーを初期化
        foreach (var bar in bars)
        {
            bar.CreateBlocks();
            barDict[bar.barName] = bar;
        }
    }

    private void OnEnable()
    {
        GhostEvents.OnGhostCaptured += OnGhostCaptured;
    }

    private void OnDisable()
    {
        GhostEvents.OnGhostCaptured -= OnGhostCaptured;
    }

    private void Start()
    {
        InitializeSlots();
    }

    private void Update()
    {
        HandleUIVisibility();
    }

    //=============================================
    // 📦 Ghost UI 関連
    //=============================================
    private void OnGhostCaptured(GhostType type, Vector3 pos)
    {
        if (type == GhostType.Lucky)
            type = GhostType.Normal;
        // GameManager側でカウント更新済み想定
        // UpdateSlot(type, newCount);
    }

    private void InitializeSlots()
    {
        foreach (var slot in ghostSlots)
        {
            if (slot.icon != null)
            {
                Color baseColor = GhostBase.GetColorByType(slot.type);
                slot.icon.color = baseColor * 0.5f;
            }

            if (slot.countText != null)
                slot.countText.text = "0";

            foreach (Transform child in slot.stackParent)
                Destroy(child.gameObject);

            for (int i = 0; i < MaxVisualBlocks; i++)
            {
                GameObject block = Instantiate(stackBlockPrefab, slot.stackParent);
                RectTransform rt = block.GetComponent<RectTransform>();
                if (rt != null)
                    rt.anchoredPosition = new Vector2(0, i * blockSpacing);

                Image img = block.GetComponent<Image>();
                if (img != null)
                    img.color = inactiveSlotColor;
            }

            UpdateSlot(slot.type, 0);
        }

        foreach (GhostType type in System.Enum.GetValues(typeof(GhostType)))
            bulletStock[type] = 0;

        Debug.Log("🎨 Ghost slots initialized.");
    }

    public void UpdateSlot(GhostType type, int count)
    {
        if (type == GhostType.Lucky)
            type = GhostType.Normal;

        bulletStock[type] = count;
        var slot = ghostSlots.Find(s => s.type == type);
        if (slot == null) return;

        if (slot.icon != null)
        {
            Color baseColor = GhostBase.GetColorByType(slot.type);
            slot.icon.color = (count > 0) ? baseColor : baseColor * 0.5f;
        }

        if (count < MaxVisualBlocks)
            slot.countText.text = "";
        else
            slot.countText.text = count.ToString() + "+";

        int visibleBlocks = Mathf.Min(count, MaxVisualBlocks);
        for (int i = 0; i < MaxVisualBlocks; i++)
        {
            Transform child = slot.stackParent.GetChild(i);
            Image img = child.GetComponent<Image>();
            if (img != null)
                img.color = (i < visibleBlocks) ? activeSlotColor : inactiveSlotColor;
        }
    }

    public bool TryUseBullet(GhostType type)
    {
        if (!bulletStock.ContainsKey(type)) return false;
        if (bulletStock[type] <= 0) return false;
        bulletStock[type]--;
        UpdateSlot(type, bulletStock[type]);
        return true;
    }

    public void FocusBulletSlot(GhostType type)
    {
        foreach (var slot in ghostSlots)
        {
            if (slot.slotBackground == null) continue;
            bool hasBullets = bulletStock.ContainsKey(slot.type) && bulletStock[slot.type] > 0;

            if (slot.type == type)
                slot.slotBackground.color = hasBullets ? focusColor : focusColor * 0.8f;
            else
                slot.slotBackground.color = hasBullets ? normalSlotColor : inactiveSlotColor;
        }

        Debug.Log($"🎯 Focused Bullet Slot: {type}");
    }

    //=============================================
    // 🩸 共通バー制御 (HP, Enemy, MiniHPなど)
    //=============================================
    public void UpdateBar(string barName, int currentValue)
    {
        if (barDict.TryGetValue(barName, out var bar))
        {
            bar.UpdateBlocks(currentValue);
        }
        else
        {
            Debug.LogWarning($"⚠️ Bar '{barName}' not found!");
        }
    }

    public void SetupBar(string barName, int newMax)
    {
        if (barDict.TryGetValue(barName, out var bar))
        {
            bar.maxBlocks = newMax;
            bar.CreateBlocks();
        }
    }

    //=============================================
    // 🧱 Enemy 専用の追加メソッド（互換維持）
    //=============================================
    public void AddEnemyBlock()
    {
        if (!barDict.ContainsKey("Enemy")) return;
        var enemyBar = barDict["Enemy"];

        if (currentEnemyCount >= enemyBar.maxBlocks)
        {
            Debug.Log("🧱 Enemy block is already full!");
            return;
        }

        enemyBar.blocks[currentEnemyCount].color = enemyBar.activeColor;
        currentEnemyCount++;
        Debug.Log($"👹 Enemy captured! Total: {currentEnemyCount}/{enemyBar.maxBlocks}");
    }

    public void ResetEnemyBlocks()
    {
        if (!barDict.ContainsKey("Enemy")) return;
        var enemyBar = barDict["Enemy"];
        foreach (var block in enemyBar.blocks)
            block.color = enemyBar.inactiveColor;
        currentEnemyCount = 0;
    }

    //=============================================
    // 🎥 UI移動制御
    //=============================================
    private void HandleUIVisibility()
    {
        bool leftClick = Input.GetMouseButton(0);
        bool rightClick = Input.GetMouseButton(1);

        hpAndEnemyGroup.anchoredPosition = Vector3.Lerp(
            hpAndEnemyGroup.anchoredPosition,
            (leftClick || rightClick) ? hpHiddenPos : hpVisiblePos,
            Time.deltaTime * moveSpeed
        );

        bulletSlotsGroup.anchoredPosition = Vector3.Lerp(
            bulletSlotsGroup.anchoredPosition,
            rightClick ? bulletVisiblePos : bulletHiddenPos,
            Time.deltaTime * moveSpeed
        );

        miniHpUI.anchoredPosition = Vector3.Lerp(
            miniHpUI.anchoredPosition,
            (leftClick && !rightClick) ? miniHpVisiblePos : miniHpHiddenPos,
            Time.deltaTime * moveSpeed
        );
    }

    //=============================================
    // 🔍 ヘルパー
    //=============================================
    public int GetCurrentCount(GhostType type)
    {
        if (!bulletStock.ContainsKey(type))
            return 0;
        return bulletStock[type];
    }
}
