using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// ミニマップ上にプレイヤーやゴーストの位置を表示するマネージャー
/// </summary>
public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance { get; private set; }

    [Header("MiniMap Settings")]
    [SerializeField] private RectTransform mapArea;        // ミニマップの背景UI (RectTransform)
    [SerializeField] private GameObject ghostMarkerPrefab; // 👻 ゴーストマーカーPrefab
    [SerializeField] private GameObject playerMarkerPrefab; // 🧍 プレイヤーマーカーPrefab
    [SerializeField] private GameObject dragonMarkerPrefab;
    [SerializeField] private GameObject graveMarkerPrefab;
    [SerializeField] private Vector2 mapSizeWorld = new Vector2(20, 20); // ワールド座標範囲
    private RectTransform dragonMarker;                   // 🐉 1体のみ
    private List<RectTransform> graveMarkers = new();
    private Dictionary<GameObject, RectTransform> ghostMarkers = new();
    private GameObject playerMarker;       // プレイヤー用マーカー
    private GameObject player;             // プレイヤー本体

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        UpdatePlayerMarker();
        UpdateGhostMarkers();
    }

    // ============================================================
    // 🧍 プレイヤー登録
    // ============================================================
    public void RegisterPlayer(GameObject playerObj)
    {
        player = playerObj;

        if (playerMarker == null)
        {
            playerMarker = Instantiate(playerMarkerPrefab, mapArea);
        }
    }

    // ============================================================
    // 👻 ゴースト登録・削除
    // ============================================================
    public void RegisterGhost(GameObject ghost)
    {
        if (ghostMarkers.ContainsKey(ghost)) return;

        GameObject marker = Instantiate(ghostMarkerPrefab, mapArea);
        RectTransform rect = marker.GetComponent<RectTransform>();

        // 👻 ゴーストマーカー共通設定
        Image img = marker.GetComponent<Image>();
        if (img != null)
            img.color = Color.white; // ← 全員同じ色

        ghostMarkers.Add(ghost, rect);
    }


    public void UnregisterGhost(GameObject ghost)
    {
        if (!ghostMarkers.ContainsKey(ghost)) return;

        Destroy(ghostMarkers[ghost].gameObject);
        ghostMarkers.Remove(ghost);
    }

    // ============================================================
    // 📍 プレイヤー更新処理
    // ============================================================
    private void UpdatePlayerMarker()
    {
        if (player == null || playerMarker == null) return;

        Vector3 pos = player.transform.position;
        Vector2 miniMapPos = WorldToMiniMap(pos);
        playerMarker.GetComponent<RectTransform>().anchoredPosition = miniMapPos;
    }

    // ============================================================
    // 👻 ゴーストマーカー更新
    // ============================================================
    private void UpdateGhostMarkers()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (var kvp in ghostMarkers)
        {
            GameObject ghost = kvp.Key;
            RectTransform marker = kvp.Value;

            // 👻 ゴーストが消えていたら削除予約
            if (ghost == null || marker == null)
            {
                if (marker != null)
                    Destroy(marker.gameObject);

                toRemove.Add(kvp.Key);
                continue;
            }

            // 📍 マーカー位置更新
            Vector3 pos = ghost.transform.position;
            Vector2 miniMapPos = WorldToMiniMap(pos);
            marker.anchoredPosition = miniMapPos;
        }

        // 🔹 ループ後に安全に削除
        foreach (var g in toRemove)
            ghostMarkers.Remove(g);
    }
    public void RegisterDragon(GameObject dragon)
    {
        if (dragon == null || dragonMarkerPrefab == null) return;

        // すでにある場合は再利用
        if (dragonMarker != null)
        {
            dragonMarker.gameObject.SetActive(true);
            return;
        }

        GameObject marker = Instantiate(dragonMarkerPrefab, mapArea);
        dragonMarker = marker.GetComponent<RectTransform>();

        // 🔴 特別な色設定
        Image img = marker.GetComponent<Image>();
        if (img != null)
            img.color = new Color(1f, 0.3f, 0.3f);

        // ✅ ミニマップ更新で追従できるように保存
        StartCoroutine(UpdateDragonMarker(dragon));
        Debug.Log("🐉 Dragon marker registered on minimap");
    }

    private IEnumerator UpdateDragonMarker(GameObject dragon)
    {
        while (dragon != null)
        {
            Vector2 miniMapPos = WorldToMiniMap(dragon.transform.position);
            dragonMarker.anchoredPosition = miniMapPos;
            yield return null;
        }

        // 🐉 ドラゴンが消えたらマーカーも削除
        if (dragonMarker != null)
        {
            Destroy(dragonMarker.gameObject);
            dragonMarker = null;
        }
    }
    public void RegisterGrave(GameObject grave)
    {
        if (grave == null || graveMarkerPrefab == null) return;

        GameObject marker = Instantiate(graveMarkerPrefab, mapArea);
        RectTransform rect = marker.GetComponent<RectTransform>();

        // ⚫ 灰色で表示
        Image img = marker.GetComponent<Image>();
        if (img != null)
            img.color = new Color(0.4f, 0.4f, 0.4f);

        graveMarkers.Add(rect);
        StartCoroutine(UpdateGraveMarker(rect, grave));
    }

    private IEnumerator UpdateGraveMarker(RectTransform marker, GameObject grave)
    {
        while (grave != null)
        {
            Vector2 miniMapPos = WorldToMiniMap(grave.transform.position);
            marker.anchoredPosition = miniMapPos;
            yield return null;
        }

        if (marker != null)
            Destroy(marker.gameObject);
    }
    // ============================================================
    // 🧮 座標変換関数：ワールド → ミニマップ
    // ============================================================
    private Vector2 WorldToMiniMap(Vector3 worldPos)
    {
        float xNorm = Mathf.Clamp(worldPos.x / mapSizeWorld.x, -1f, 1f);
        float yNorm = Mathf.Clamp(worldPos.y / mapSizeWorld.y, -1f, 1f);

        float mapWidth = mapArea.rect.width / 2f;
        float mapHeight = mapArea.rect.height / 2f;

        return new Vector2(xNorm * mapWidth, yNorm * mapHeight);
    }

    // ============================================================
    // 🎨 タイプ別カラー定義
    // ============================================================
    private Color GetColorByType(GhostType type)
    {
        return type switch
        {
            GhostType.Normal => Color.white,
            GhostType.Quick => new Color(1f, 0.7f, 0.7f),
            GhostType.Tank => new Color(0.6f, 0.6f, 1f),
            GhostType.Suicide => new Color(1f, 0.4f, 0.4f),
            GhostType.Lucky => new Color(1f, 1f, 0.6f),
            _ => Color.gray
        };
    }
}
