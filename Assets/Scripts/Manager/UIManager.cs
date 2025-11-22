using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    //===================================================
    // 🔵 BarLabelPair（HP / 弾数などの数値表示）
    //===================================================
    [System.Serializable]
    public class BarLabelPair
    {
        public string barName;              // 例: "HP", "CommonBullet"
        public TextMeshProUGUI currentText; // 現在値（例: 3）
        public TextMeshProUGUI maxText;     // 最大値（例: 10）
    }

    [Header("Bar Label Pairs")]
    [SerializeField] private List<BarLabelPair> barLabelPairs = new();
    private Dictionary<string, BarLabelPair> barLabelDict = new();

    //===================================================
    // 🔴 共通バー（HP, 弾など）
    //===================================================
    [System.Serializable]
    public class BarUI
    {
        public string barName;
        public RectTransform container;
        public GameObject blockPrefab;  
        public Color activeColor = Color.white;
        public Color inactiveColor = new Color(0.3f, 0.3f, 0.3f);
        public Vector2 blockSize = new Vector2(15f, 40f);

        public enum LayoutDirection { Horizontal, Vertical }
        public LayoutDirection layoutDirection = LayoutDirection.Horizontal;

        [HideInInspector] public List<Image> blocks = new();
    }

    [Header("Common Bar Settings")]
    [SerializeField] private List<BarUI> bars = new();

    //===================================================
    // 📌 弾ストック（共通弾）
    //===================================================
    private int commonBullet = 0;
    private int commonBulletMax = 10;

    //===================================================
    // 🎨 UI Move Settings（そのまま）
    //===================================================
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
    [SerializeField] private GameObject dragonHPUI;
    [SerializeField] private RectTransform dragonUI;
    [SerializeField] private Vector3 dragonVisiblePos = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 dragonHiddenPos = new Vector3(0, 200f, 0);
    [SerializeField] private GameObject playerStatusPanel;

    private Dictionary<string, BarUI> barDict = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Bar辞書
        foreach (var bar in bars)
            barDict[bar.barName] = bar;

        // ラベル辞書
        foreach (var pair in barLabelPairs)
        {
            if (pair != null && !string.IsNullOrEmpty(pair.barName))
                barLabelDict[pair.barName] = pair;
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
        // 共通弾バー生成
        CreateBar("CommonBullet", commonBulletMax);

        // 初期値は 0
        UpdateBarAndCounter("CommonBullet", commonBullet, commonBulletMax);
    }

    private void Update()
    {
        HandleUIVisibility();
    }

    //===================================================
    // 🟣 共通弾として Ghost 捕獲を処理
    //===================================================
    private void OnGhostCaptured(GhostType type, Vector3 pos)
    {
        commonBullet++;
        commonBullet = Mathf.Clamp(commonBullet, 0, commonBulletMax);

        UpdateBarAndCounter("CommonBullet", commonBullet, commonBulletMax);
    }

    //===================================================
    // 🎯 弾を使う（共通弾）
    //===================================================
    public bool TryUseBullet()
    {
        if (commonBullet <= 0)
            return false;

        commonBullet--;
        UpdateBarAndCounter("CommonBullet", commonBullet, commonBulletMax);

        return true;
    }
    public void AddCommonBullet()
    {
        commonBullet++;
        commonBullet = Mathf.Clamp(commonBullet, 0, commonBulletMax);

        UpdateBarAndCounter("CommonBullet", commonBullet, commonBulletMax);
    }
    //===================================================
    // 🔵 バー + 数値 同時更新
    //===================================================
    public void UpdateBarAndCounter(string barName, int current, int max)
    {
        UpdateBar(barName, current);

        if (!barLabelDict.TryGetValue(barName, out var pair))
            return;

        if (pair.currentText != null)
            pair.currentText.text = current.ToString();

        if (pair.maxText != null)
            pair.maxText.text = max.ToString();
    }

    //===================================================
    // 🔴 バーブロック生成
    //===================================================
    public void CreateBar(string barName, int blockCount)
    {
        if (!barDict.ContainsKey(barName))
        {
            Debug.LogWarning($"⚠️ Bar '{barName}' not found");
            return;
        }

        BarUI bar = barDict[barName];

        foreach (Transform child in bar.container)
            Destroy(child.gameObject);
        bar.blocks.Clear();

        for (int i = 0; i < blockCount; i++)
        {
            GameObject block = Instantiate(bar.blockPrefab, bar.container);
            RectTransform rt = block.GetComponent<RectTransform>();
            Image img = block.GetComponent<Image>();

            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.sizeDelta = bar.blockSize;

                if (bar.layoutDirection == BarUI.LayoutDirection.Horizontal)
                    rt.anchoredPosition = new Vector2(i * (bar.blockSize.x + 2f), 0);
                else
                    rt.anchoredPosition = new Vector2(0, i * (bar.blockSize.y + 2f));
            }

            img.color = bar.inactiveColor;
            bar.blocks.Add(img);
        }

        Debug.Log($"🟩 {barName} created ({blockCount})");
    }

    //===================================================
    // 🔴 バー更新
    //===================================================
    public void UpdateBar(string barName, int currentValue)
    {
        if (!barDict.ContainsKey(barName)) return;
        var bar = barDict[barName];

        for (int i = 0; i < bar.blocks.Count; i++)
        {
            var img = bar.blocks[i];
            if (img == null) continue;

            bool active = (i < currentValue);

            if (active)
            {
                Color c = bar.activeColor;
                c.a = 1f;
                img.color = c;
            }
            else
            {
                Color endColor = bar.inactiveColor;
                endColor.a = 1f;
                StartCoroutine(AnimateBlockLoss(img, img.color, endColor, 0.2f));
            }
        }
    }

    private IEnumerator AnimateBlockLoss(Image img, Color startColor, Color endColor, float duration)
    {
        if (img == null) yield break;

        RectTransform rt = img.rectTransform;
        rt.localScale = Vector3.one;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;

            float p = t / duration;
            float scale = 1f - 0.2f * Mathf.Sin(p * Mathf.PI);

            rt.localScale = new Vector3(scale, scale, 1f);
            img.color = Color.Lerp(startColor, endColor, p);

            yield return null;
        }

        rt.localScale = Vector3.one;
        img.color = endColor;
    }

    //===================================================
    // 🎥 UI 移動処理
    //===================================================
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

        dragonUI.anchoredPosition = Vector3.Lerp(
            dragonUI.anchoredPosition,
            leftClick ? dragonHiddenPos : dragonVisiblePos,
            Time.deltaTime * moveSpeed
        );
    }

    //===================================================
    // 🟢 UI On/Off
    //===================================================
    public void ShowPlayerStatus(bool show)
    {
        if (playerStatusPanel != null)
            playerStatusPanel.SetActive(show);
    }

    public void ShowDragonUI(bool show)
    {
        if (dragonHPUI != null)
            dragonHPUI.SetActive(show);
    }
}
