using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    //=============================================
    // 🟣 Ghost Slot UI（そのまま維持）
    //=============================================
    [System.Serializable]
    public class GhostUISlot
    {
        public GhostType type;
        public Image slotBackground;
        public Image icon;
        public TextMeshProUGUI countText;
        public RectTransform stackParent; 
    }

    //=============================================
    // 🔴 共通バークラス（Prefab・maxBlocksなし）
    //=============================================
    [System.Serializable]
    public class BarUI
    {
        public string barName;                      // "HP", "MiniHP", "Enemy"など
        public RectTransform container;             // 配置先 (Canvas内)
        public Color activeColor = Color.red;       // 有効色
        public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);
        public Vector2 blockSize = new Vector2(15f, 40f);
        [HideInInspector] public List<Image> blocks = new();
    }

    //=============================================
    // 🎨 Serialized Fields
    //=============================================
    [Header("Common Bar Settings")]
    [SerializeField] private GameObject commonBlockPrefab; // すべてのバーで共通
    [SerializeField] private List<BarUI> bars = new();

    [Header("Bullet Slots")]
    [SerializeField] private List<GhostUISlot> ghostSlots;
    [SerializeField] private Color inactiveSlotColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] private Color activeSlotColor = Color.white;
    [SerializeField] private GameObject stackBlockPrefab;
    [SerializeField] private float blockSpacing = 2f;
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

        // 辞書登録（初期化のみ）
        foreach (var bar in bars)
            barDict[bar.barName] = bar;
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
    // 📦 Ghost Slot 関連
    //=============================================
    private void OnGhostCaptured(GhostType type, Vector3 pos)
    {
        if (type == GhostType.Lucky)
            type = GhostType.Normal;
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
            Debug.Log($"🎨 UpdateSlot({type}) → icon.color={slot.icon.color}");
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
    // 🩸 共通バー制御 (Prefab / maxBlocksなし)
    //=============================================
    public void CreateBar(string barName, int blockCount)
    {
        if (!barDict.ContainsKey(barName) || stackBlockPrefab == null)
        {
            Debug.LogWarning($"⚠️ Bar '{barName}' not found or prefab missing");
            return;
        }

        BarUI bar = barDict[barName];
        if (bar.container == null) return;

        // 既存削除
        foreach (Transform child in bar.container)
            Destroy(child.gameObject);

        bar.blocks.Clear();

        // 新規生成
        for (int i = 0; i < blockCount; i++)
        {
            GameObject block = Instantiate(stackBlockPrefab, bar.container);
            block.transform.SetAsLastSibling();

            RectTransform rt = block.GetComponent<RectTransform>();
            Image img = block.GetComponent<Image>();

            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.sizeDelta = bar.blockSize;
                rt.anchoredPosition = new Vector2(i * (bar.blockSize.x + 2f), 0);
            }

            if (img != null)
                img.color = bar.inactiveColor;

            bar.blocks.Add(img);
        }

        Debug.Log($"✅ {barName} Bar Created ({blockCount}個)");
    }

    public void UpdateBar(string barName, int currentValue)
    {
        if (!barDict.ContainsKey(barName)) return;

        var bar = barDict[barName];
        for (int i = 0; i < bar.blocks.Count; i++)
        {
            Image block = bar.blocks[i];
            if (block == null) continue;

            // 🟢 明るさを常に最大に固定（activeColorを常に使用）
            block.color = bar.activeColor;

            // もし透明度でオンオフしたい場合は、アルファだけ変えることも可能：
            var c = bar.activeColor;
            c.a = (i < currentValue) ? 1f : 0.3f;
            block.color = c;
        }
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
