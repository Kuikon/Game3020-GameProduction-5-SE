using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    [SerializeField] private Vector2 mapSizeWorld = new Vector2(20, 20); // ワールド座標範囲

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
    public void RegisterGhost(GameObject ghost, GhostType type)
    {
        if (ghostMarkers.ContainsKey(ghost)) return;

        GameObject marker = Instantiate(ghostMarkerPrefab, mapArea);
        RectTransform rect = marker.GetComponent<RectTransform>();

        // 🟢 ゴーストタイプごとにマーカーの色を変更
        Image img = marker.GetComponent<Image>();
        if (img != null)
            img.color = GetColorByType(type);

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
