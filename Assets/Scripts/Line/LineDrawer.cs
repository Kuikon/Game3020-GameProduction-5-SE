using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LineDraw : MonoBehaviour
{
    public static LineDraw Instance;

    [Header("Line Settings")]
    public float maxLineLength = 5f;
    [SerializeField] private float interval = 0.1f;

    [Header("References")]
    [SerializeField] private Camera _cam;
    [SerializeField] private LineVisualEffectManager effectManager;

    // Components
    private LineRenderer _rend;
    private EdgeCollider2D edgeCol;
    private PolygonCollider2D poly;

    // Line data
    private readonly Queue<Vector3> linePoints = new();
    private readonly Dictionary<GameObject, int> insideCount = new();

    private int posCount = 0;
    private float currentLineLength = 0f;

    // Icons
    [SerializeField] private GameObject startIconPrefab;
    [SerializeField] private GameObject endIconPrefab;
    [SerializeField] private GameObject droppedBallPrefab;

    private GameObject startIcon;
    private GameObject endIcon;
    [SerializeField] private string[] collidableTags;
    // Flash 管理
    private readonly HashSet<SpriteRenderer> flashingSet = new();
    private bool isForceStopped = false;
    // =========================================================
    // INITIALIZE
    // =========================================================
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        insideCount.Clear();
        DontDestroyOnLoad(gameObject);

        if (droppedBallPrefab == null)
        {
            // Resources/EXPOrb.prefab があればここでロード
            droppedBallPrefab = Resources.Load<GameObject>("EXPOrb");
        }
    }

    void Start()
    {
        _rend = GetComponent<LineRenderer>();
        _rend.positionCount = 0;
        _rend.startWidth = 0.1f;
        _rend.endWidth = 0.1f;
        _rend.useWorldSpace = true;
        _rend.material = new Material(Shader.Find("Sprites/Default"));

        if (_cam == null)
            _cam = Camera.main;

        //edgeCol = gameObject.AddComponent<EdgeCollider2D>();
        //edgeCol.isTrigger = true;
    }

    // =========================================================
    // UPDATE
    // =========================================================
    void Update()
    {
        if (isForceStopped)
            return;
        Vector3 mouse = Input.mousePosition;
        if (!new Rect(0, 0, Screen.width, Screen.height).Contains(mouse))
            return;

        mouse.z = Mathf.Abs(_cam.transform.position.z);
        mouse = _cam.ScreenToWorldPoint(mouse);
        mouse.z = 0f;

        if (Input.GetMouseButtonDown(0))
        {
            RemoveIcons();
            CreateIcons(mouse);
            if (edgeCol != null) Destroy(edgeCol);
            edgeCol = gameObject.AddComponent<EdgeCollider2D>();
            edgeCol.isTrigger = true;
        }

        if (Input.GetMouseButton(0))
        {
            SetPosition(mouse);

            if (posCount > 2)
                CheckIntersection();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndLine();
        }
    }

    // =========================================================
    // ICONS
    // =========================================================
    void CreateIcons(Vector3 pos)
    {
        if (startIconPrefab != null)
            startIcon = Instantiate(startIconPrefab, pos, Quaternion.identity);

        if (endIconPrefab != null)
            endIcon = Instantiate(endIconPrefab, pos, Quaternion.identity);
    }

    void RemoveIcons()
    {
        if (startIcon != null) Destroy(startIcon);
        if (endIcon != null) Destroy(endIcon);
    }

    void UpdateIcons()
    {
        if (startIcon != null && posCount > 0)
            startIcon.transform.position = _rend.GetPosition(0);

        if (endIcon != null && posCount > 0)
            endIcon.transform.position = _rend.GetPosition(posCount - 1);
    }

    // =========================================================
    // LINE DRAWING
    // =========================================================
    void SetPosition(Vector3 pos)
    {
        if (!PosCheck(pos)) return;

        // speed-based width
        float width = 0.1f;
        if (posCount > 0)
        {
            float speed = Vector3.Distance(_rend.GetPosition(posCount - 1), pos) / Mathf.Max(Time.deltaTime, 0.0001f);
            width = Mathf.Lerp(0.05f, 0.35f, speed * 0.015f);
        }
        _rend.startWidth = width * 0.3f;
        _rend.endWidth = width * 0.7f;

        if (posCount > 0)
            currentLineLength += Vector3.Distance(_rend.GetPosition(posCount - 1), pos);

        posCount++;
        _rend.positionCount = posCount;
        _rend.SetPosition(posCount - 1, pos);
        linePoints.Enqueue(pos);

        TrimLine();
        UpdateEdgeCollider();
        UpdateIcons();
    }

    bool PosCheck(Vector3 pos)
    {
        if (posCount == 0) return true;
        return Vector3.Distance(_rend.GetPosition(posCount - 1), pos) > interval;
    }

    void TrimLine()
    {
        while (linePoints.Count >= 2 && GetTotalLength(linePoints) > maxLineLength)
        {
            linePoints.Dequeue();
        }

        Vector3[] arr = linePoints.ToArray();
        _rend.positionCount = arr.Length;
        _rend.SetPositions(arr);
        posCount = arr.Length;
    }

    float GetTotalLength(IEnumerable<Vector3> pts)
    {
        float len = 0f;
        Vector3? prev = null;

        foreach (var p in pts)
        {
            if (prev.HasValue)
                len += Vector3.Distance(prev.Value, p);
            prev = p;
        }
        return len;
    }

    void UpdateEdgeCollider()
    {
         if (edgeCol == null) return;
         if (!edgeCol.enabled) return;
        Vector3[] arr = linePoints.ToArray();
        posCount = arr.Length;

        Vector2[] pts2D = new Vector2[posCount];
        for (int i = 0; i < posCount; i++)
            pts2D[i] = arr[i];

        edgeCol.points = pts2D;
    }

    // =========================================================
    // INTERSECTION DETECTION
    // =========================================================
    void CheckIntersection()
    {
        if (posCount < 4) return;

        Vector3 p1 = _rend.GetPosition(posCount - 2);
        Vector3 p2 = _rend.GetPosition(posCount - 1);

        for (int i = 0; i < posCount - 3; i++)
        {
            Vector3 p3 = _rend.GetPosition(i);
            Vector3 p4 = _rend.GetPosition(i + 1);

            if (LineSegmentsIntersect(p1, p2, p3, p4, out Vector2 cross))
            {
                // エフェクト
                effectManager?.CreateLineAfterImage(_rend);
                Light2DRadiusController.Instance?.FlashRadius();

                // 1フレームだけ有効にするポリゴン
                CreatePolygon(i, cross);
                CheckInsideObjects();   // この瞬間だけ poly が生きてる
                CleanPolygon();         // すぐ破棄

                // ゲームロジック（ゴーストヒット回数を加算して縮める）
                AddLoopHit();

                // 線を交差位置までトリムして、その後も描き続けられるようにする
                TrimLoop(i, cross);
                return;
            }
        }
    }

    bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 cross)
    {
        cross = Vector2.zero;

        float d = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        if (Mathf.Approximately(d, 0f)) return false;

        float u = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;
        float v = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;

        if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
        {
            cross = p1 + v * (p2 - p1);
            return true;
        }
        return false;
    }

    void TrimLoop(int startIndex, Vector3 crossPoint)
    {
        List<Vector3> pts = new List<Vector3>(linePoints);

        // ★ RemoveRange を使う前に安全チェック
        int removeStart = startIndex + 1;
        int removeCount = pts.Count - removeStart;

        if (removeStart < 0 || removeStart > pts.Count)
            return;

        if (removeCount < 0)
            removeCount = 0;

        if (removeCount > 0)
            pts.RemoveRange(removeStart, removeCount);

        // 最後の点を交差点に置き換える
        if (pts.Count > 0)
            pts[pts.Count - 1] = crossPoint;

        // 再セット
        linePoints.Clear();
        foreach (var p in pts)
            linePoints.Enqueue(p);

        _rend.positionCount = pts.Count;
        _rend.SetPositions(pts.ToArray());
        posCount = pts.Count;

        // コライダー更新
        Vector2[] colPoints = new Vector2[pts.Count];
        for (int i = 0; i < pts.Count; i++)
            colPoints[i] = pts[i];

        if (edgeCol != null)
            edgeCol.points = colPoints;

        Debug.Log($"[TrimLoop] Loop trimmed safely. New vertex count = {pts.Count}");
    }


    // =========================================================
    // POLYGON CREATION
    // =========================================================
    void CreatePolygon(int startIndex, Vector2 crossPoint)
    {
        CleanPolygon();

        List<Vector2> pts = new();
        Vector3[] arr = linePoints.ToArray();

        for (int i = startIndex; i < arr.Length; i++)
            pts.Add(arr[i]);

        // 最後の点を交差位置にする
        if (pts.Count > 0)
            pts[pts.Count - 1] = crossPoint;

        poly = gameObject.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;
        poly.points = pts.ToArray();
    }

    void CleanPolygon()
    {
        if (poly != null)
        {
            Destroy(poly);
            poly = null;
        }
    }

    // =========================================================
    // CHECK OBJECTS INSIDE LOOP (このフレームだけ)
    // =========================================================
    void CheckInsideObjects()
    {
        CheckGhosts();
        CheckGraves();
        CheckDragons();
        CheckChests();
    }

    void CheckGhosts()
    {
        foreach (GameObject ghost in GameObject.FindGameObjectsWithTag("Dog"))
        {
            if (!IsInside(ghost)) continue;

            var gb = ghost.GetComponent<GhostBase>();
            if (gb == null)
            {
                Debug.Log($"[CheckGhosts] Ghost {ghost.name} has no GhostBase!");
                continue;
            }

            if (!insideCount.ContainsKey(ghost))
            {
                insideCount[ghost] = 0;
                Debug.Log($"[CheckGhosts] {ghost.name} 初期化 → insideCount = 0");
            }

            // 視覚的なエフェクトだけ
            LineVisualEffectManager.Instance.PlayCaptureEffect(_rend, ghost.transform);

            var sr = ghost.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(FlashOnce(sr, Color.yellow));
        }
    }

    void CheckGraves()
    {
        List<GameObject> brokenGraves = new();

        foreach (var grave in GameObject.FindGameObjectsWithTag("Grave"))
        {
            if (!IsInside(grave)) continue;
            LineVisualEffectManager.Instance.PlayCaptureEffect(_rend, grave.transform);

            var sr = grave.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(FlashOnce(sr, new Color(1f, 0.7f, 0f)));
            if (!insideCount.ContainsKey(grave))
                insideCount[grave] = 0;

            insideCount[grave]++;

            if (insideCount[grave] >= 5)
            {
                brokenGraves.Add(grave);
            }
        }

        if (brokenGraves.Count > 0)
        {
            GraveManager gm = FindFirstObjectByType<GraveManager>();
            if (gm != null)
            {
                gm.ReplaceCapturedGraves(brokenGraves);
                Debug.Log($"🪦 {brokenGraves.Count} graves broken & replaced!");
            }
            else
            {
                Debug.LogWarning("⚠ GraveManager not found!");
            }
        }
        
    }

    void CheckDragons()
    {
        foreach (var dragon in GameObject.FindGameObjectsWithTag("Dragon"))
        {
            if (!IsInside(dragon)) continue;
            LineVisualEffectManager.Instance.PlayCaptureEffect(_rend, dragon.transform);

            var sr = dragon.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(FlashOnce(sr, Color.yellow));
            var health = dragon.GetComponent<DragonHealth>();
            if (health != null)
            {
                health.TakeDamage(1);
                effectManager?.PlayHitEffect(dragon.transform.position);
                Debug.Log($"🔥 Dragon hit! HP = {health.currentHP}/{health.maxHP}");
            }
        }
    }

    void CheckChests()
    {
        foreach (var chest in GameObject.FindGameObjectsWithTag("Chest"))
        {
            if (!IsInside(chest)) continue;

            chest.GetComponent<Chest>()?.OpenChest();
        }
    }

    bool IsInside(GameObject obj)
    {
        if (poly == null) return false;
        return poly.OverlapPoint(obj.transform.position);
    }

    // =========================================================
    // LOOP HIT LOGIC（ゴーストのヒット回数 & 縮小）
    // =========================================================
    void AddLoopHit()
    {
        foreach (GameObject ghost in GameObject.FindGameObjectsWithTag("Dog"))
        {
            // CheckGhosts の時に poly 内にいたゴーストだけが insideCount にキーを持っている
            if (!insideCount.ContainsKey(ghost)) continue;

            insideCount[ghost]++;

            var gb = ghost.GetComponent<GhostBase>();
            if (gb == null) continue;

            // progress: 0 → 1
            float progress = Mathf.Clamp01((float)insideCount[ghost] / gb.data.captureHitsNeeded);
            // ratio: 1.0 → 0.6 に縮むイメージ
            float minRatio = 0.6f;
            float ratio = Mathf.Lerp(1f, minRatio, progress);

            gb.Shrink(ratio);

            if (insideCount[ghost] >= gb.data.captureHitsNeeded)
            {
                CaptureGhost(ghost);
            }
        }
    }

    void CaptureGhost(GameObject ghost)
    {
        Debug.Log("CaptureGhost called on: " + ghost.name);

        var gb = ghost.GetComponent<GhostBase>();
        if (gb == null) return;

        effectManager?.PlayHitEffect(ghost.transform.position);
        LineVisualEffectManager.Instance.ReleaseAllGlowsUpward();

        Transform player = GameObject.FindWithTag("Player").transform;

        ExpDropManager.Instance.SpawnExpOrbs(
            ghost.transform.position,
            gb.data.dropAmount,
            gb.data.expPerDrop,
            player
        );

        gb.Kill();
        GhostEvents.RaiseGhostCaptured(gb.data.type, ghost.transform.position);
    }

    // =========================================================
    // FLASH EFFECT
    // =========================================================
    private IEnumerator FlashOnce(SpriteRenderer sr, Color flashColor, float duration = 0.3f)
    {
        if (sr == null || flashingSet.Contains(sr)) yield break;

        flashingSet.Add(sr);

        Color original = sr.color;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            if (sr == null || sr.gameObject == null)
            {
                flashingSet.Remove(sr);
                yield break;
            }
            sr.color = Color.Lerp(flashColor, original, t / duration);
            yield return null;
        }

        if (sr != null && sr.gameObject != null && sr.gameObject.activeInHierarchy)
            sr.color = original;

        flashingSet.Remove(sr);
    }

    // =========================================================
    // RESET
    // =========================================================
    void ResetLine()
    {
        currentLineLength = 0f;
        posCount = 0;
        linePoints.Clear();
        _rend.positionCount = 0;
    }

    void RestoreAllGhosts()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("Dog"))
        {
            var gb = g.GetComponent<GhostBase>();
            if (gb != null) gb.Restore();
        }
    }

    // =========================================================
    // REVERSE CAPTURE EFFECT（線がゴーストに触れたとき）
    // =========================================================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (edgeCol == null || !edgeCol.enabled) return;
        foreach (var tag in collidableTags)
        {
            if (other.CompareTag(tag))
            {
                Transform target = other.transform;

                LineVisualEffectManager.Instance.PlayReverseCaptureEffect(_rend, target.transform);

                RestoreAllGhosts();
                ResetLine();
                CleanPolygon();
                insideCount.Clear();
            }
        }
           
    }

    // マウスを離したときの終了処理
    void EndLine()
    {
        RestoreAllGhosts();
        ResetLine();
        insideCount.Clear();

        if (edgeCol != null) Destroy(edgeCol);
        if (poly != null) Destroy(poly);
        edgeCol = null;
        poly = null;
    }

    public void ForceStopDrawing()
    {
        // Update() の描画処理を止めるためのフラグ
        isForceStopped = true;

        // ラインポイントを完全クリア
        linePoints.Clear();
        posCount = 0;
        currentLineLength = 0f;

        // LineRenderer を消す
        if (_rend != null)
            _rend.positionCount = 0;

        // EdgeCollider を安全に破棄
        if (edgeCol != null)
        {
            Destroy(edgeCol);
            edgeCol = null;
        }

        // PolygonCollider も破棄
        if (poly != null)
        {
            Destroy(poly);
            poly = null;
        }

        // アイコン削除
        RemoveIcons();
    }
}
