using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LineDraw : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private float maxLineLength = 5f;
    [SerializeField] private float interval = 0.1f;

    [Header("References")]
    [SerializeField] private Camera _cam;
    [SerializeField] private LineVisualEffectManager effectManager;

    private LineRenderer _rend;
    private EdgeCollider2D edgeCol;
    private PolygonCollider2D poly;

    private Queue<Vector3> linePoints = new();
    private Dictionary<GameObject, int> insideCount = new();

    private int posCount = 0;
    private float currentLineLength = 0f;

    private HashSet<SpriteRenderer> flashingSet = new();
    [SerializeField] private GameObject startIconPrefab;
    [SerializeField] private GameObject endIconPrefab;
    private GameObject startIcon;
    private GameObject endIcon;
    void Awake()
    {
        insideCount = new Dictionary<GameObject, int>();
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _rend = GetComponent<LineRenderer>();
        _rend.positionCount = 0;
        _rend.startWidth = 0.1f;
        _rend.endWidth = 0.1f;
        _rend.material = new Material(Shader.Find("Sprites/Default"));
        _rend.useWorldSpace = true;

        if (_cam == null) _cam = Camera.main;

        edgeCol = gameObject.AddComponent<EdgeCollider2D>();
        edgeCol.isTrigger = true;
    }

    void Update()
    {
        Vector3 mouse = Input.mousePosition;
        if (!new Rect(0, 0, Screen.width, Screen.height).Contains(mouse))
            return;

        mouse.z = Mathf.Abs(_cam.transform.position.z);
        mouse = _cam.ScreenToWorldPoint(mouse);
        if (Input.GetMouseButtonDown(0))
        {
            RemoveIcons(); // 前のを消す
            CreateIcons(mouse); // 今回の線のスタートとエンド用アイコンを生成
        }
        if (Input.GetMouseButton(0))
        {
            SetPosition(mouse);
            if (posCount > 2) CheckIntersection();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            RestoreAllGhosts();
            ResetLine();
            insideCount.Clear();
            CleanPolygon();
        }
    }
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
            float speed = Vector3.Distance(_rend.GetPosition(posCount - 1), pos) / Time.deltaTime;
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

        // Update edge collider
        Vector2[] points2D = new Vector2[posCount];
        for (int i = 0; i < posCount; i++)
            points2D[i] = _rend.GetPosition(i);
        edgeCol.points = points2D;
        if (startIcon != null && posCount > 0)
        {
            startIcon.transform.position = _rend.GetPosition(0); // 最初の点
        }

        if (endIcon != null && posCount > 0)
        {
            endIcon.transform.position = _rend.GetPosition(posCount - 1); // 最新の点
        }
    }

    bool PosCheck(Vector3 pos)
    {
        if (posCount == 0) return true;
        return Vector3.Distance(_rend.GetPosition(posCount - 1), pos) > interval;
    }

    void TrimLine()
    {
        while (linePoints.Count >= 2 && GetTotalLength(linePoints) > maxLineLength)
            linePoints.Dequeue();

        Vector3[] arr = linePoints.ToArray();
        _rend.positionCount = arr.Length;
        _rend.SetPositions(arr);
        posCount = arr.Length;
    }

    float GetTotalLength(IEnumerable<Vector3> pts)
    {
        float len = 0;
        Vector3? prev = null;
        foreach (var p in pts)
        {
            if (prev.HasValue) len += Vector3.Distance(prev.Value, p);
            prev = p;
        }
        return len;
    }

    // =========================================================
    // INTERSECTION DETECTION
    // =========================================================
    void CheckIntersection()
    {
        //edgeCol.enabled = false;
        Vector3 p1 = _rend.GetPosition(posCount - 2);
        Vector3 p2 = _rend.GetPosition(posCount - 1);

        for (int i = 0; i < posCount - 3; i++)
        {
            Vector3 p3 = _rend.GetPosition(i);
            Vector3 p4 = _rend.GetPosition(i + 1);

            if (LineSegmentsIntersect(p1, p2, p3, p4, out Vector2 cross))
            {
                effectManager?.CreateLineAfterImage(_rend);
                
                CreatePolygon(i, cross);
                AddLoopHit();
                TrimLoop(i, cross);
                return;
            }
        }
    }
    void TrimLoop(int startIndex, Vector3 crossPoint)
    {

        List<Vector3> pts = new List<Vector3>(linePoints);

        int lastIndex = pts.Count - 1;

        // 内側削除
        int removeStart = startIndex + 1;
        int removeCount = lastIndex - removeStart;

        if (removeCount > 0)
            pts.RemoveRange(removeStart, removeCount);

        // 交差点に閉じる
        pts[pts.Count - 1] = crossPoint;

        linePoints = new Queue<Vector3>(pts);

        // LineRenderer 更新
        _rend.positionCount = pts.Count;
        _rend.SetPositions(pts.ToArray());

        posCount = pts.Count;

        // ★ここが重要：EdgeCollider2D の当たり判定更新！
        Vector2[] newPoints = new Vector2[pts.Count];
        for (int i = 0; i < pts.Count; i++)
            newPoints[i] = pts[i];
        edgeCol.points = newPoints;
        //edgeCol.enabled = true; 
    }

    void AddLoopHit()
    {
        //-------------------------
        // 🐶 Ghost（Dog）
        //-------------------------
        foreach (GameObject ghost in GameObject.FindGameObjectsWithTag("Dog"))
        {
            if (!IsInside(ghost)) continue;

            if (!insideCount.ContainsKey(ghost))
                insideCount[ghost] = 0;

            insideCount[ghost]++;

            var gb = ghost.GetComponent<GhostBase>();
            if (gb != null)
            {
                float progress = Mathf.Clamp01((float)insideCount[ghost] / gb.data.captureHitsNeeded);
                float scale = Mathf.Lerp(1f, 0.6f, progress);
                gb.Shrink(scale);

                if (insideCount[ghost] >= gb.data.captureHitsNeeded)
                    CaptureGhost(ghost);
            }
        }

        //-------------------------
        // 🪦 Grave（墓）
        //-------------------------
        foreach (GameObject grave in GameObject.FindGameObjectsWithTag("Grave"))
        {
            if (!IsInside(grave)) continue;

            if (!insideCount.ContainsKey(grave))
                insideCount[grave] = 0;

            insideCount[grave]++;

            var sr = grave.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.yellow;

            if (insideCount[grave] >= 5)
            {
                //HandleGraveCaptured(grave);
            }
        }

        //-------------------------
        // 🐉 Dragon（ドラゴン）
        //-------------------------
        foreach (GameObject dragon in GameObject.FindGameObjectsWithTag("Dragon"))
        {
            if (!IsInside(dragon)) continue;

            var health = dragon.GetComponent<DragonHealth>();
            if (health != null)
            {
                health.TakeDamage(1);
                effectManager?.PlayHitEffect(dragon.transform.position);

                Debug.Log($"🔥 Dragon hit! HP = {health.currentHP}/{health.maxHP}");
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

    // =========================================================
    // POLYGON CREATION
    // =========================================================
    void CreatePolygon(int start, Vector2 cross)
    {
        CleanPolygon();

        List<Vector2> pts = new();
        for (int j = start + 1; j < posCount; j++)
            pts.Add(_rend.GetPosition(j));
        pts.Add(cross);

        poly = gameObject.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;

        Vector2[] local = new Vector2[pts.Count];
        for (int i = 0; i < pts.Count; i++)
            local[i] = pts[i];

        poly.points = local;

        CheckInsideObjects();
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
    // CHECK OBJECTS INSIDE LOOP
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
            // ★デバッグ：Inside判定
            Debug.Log($"[CheckGhosts] Ghost:{ghost.name}  Inside? = {IsInside(ghost)}");

            if (!IsInside(ghost)) continue;

            var gb = ghost.GetComponent<GhostBase>();
            if (gb == null)
            {
                Debug.Log($"[CheckGhosts] Ghost {ghost.name} has no GhostBase!");
                continue;
            }

            // ★デバッグ：insideCount の初期化
            if (!insideCount.ContainsKey(ghost))
            {
                insideCount[ghost] = 0;
                Debug.Log($"[CheckGhosts] Ghost:{ghost.name} -> Initialize insideCount to 0");
            }

            // ★ insideCount 増加
           
            Debug.Log($"[CheckGhosts] Ghost:{ghost.name} -> insideCount = {insideCount[ghost]}/{gb.data.captureHitsNeeded}");

            // 閉じたループ時エフェクト
            LineVisualEffectManager.Instance.PlayCaptureEffect(_rend, ghost.transform);

            var sr = ghost.GetComponent<SpriteRenderer>();
            if (sr != null)
                StartCoroutine(FlashOnce(sr, Color.softYellow));

            // ★ Shrink の計算状態
            float progress = Mathf.Clamp01((float)insideCount[ghost] / gb.data.captureHitsNeeded);
            float scale = Mathf.Lerp(1f, 0.6f, progress);

            Debug.Log($"[CheckGhosts] Ghost:{ghost.name} -> shrink progress={progress}, scale={scale}");

            gb.Shrink(scale);

            // ★ キャプチャされたか？
            if (insideCount[ghost] >= gb.data.captureHitsNeeded)
            {
                Debug.Log($"[CheckGhosts] Ghost:{ghost.name} CAPTURED!");
                CaptureGhost(ghost);
            }
        }
    }


    void CaptureGhost(GameObject ghost)
    {
        var gb = ghost.GetComponent<GhostBase>();
        effectManager?.PlayHitEffect(ghost.transform.position);
        LineVisualEffectManager.Instance.ReleaseAllGlowsUpward();
        gb.Kill();

        GhostEvents.RaiseGhostCaptured(gb.data.type, ghost.transform.position);
    }

    void CheckGraves()
    {
        foreach (var grave in GameObject.FindGameObjectsWithTag("Grave"))
        {
            if (!IsInside(grave)) continue;

            if (!insideCount.ContainsKey(grave))
                insideCount[grave] = 0;

            insideCount[grave]++;

            if (insideCount[grave] == 5)
                HighlightGrave(grave);
        }
    }

    void HighlightGrave(GameObject g)
    {
        var sr = g.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow;
            StartCoroutine(ResetColor(sr));
        }
    }

    IEnumerator ResetColor(SpriteRenderer sr)
    {
        yield return new WaitForSeconds(1.5f);
        if (sr != null) sr.color = Color.white;
    }

    void CheckDragons()
    {
        foreach (var dragon in GameObject.FindGameObjectsWithTag("Dragon"))
        {
            if (!IsInside(dragon)) continue;

            var health = dragon.GetComponent<DragonHealth>();
            if (health != null)
                health.TakeDamage(1);
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
    private IEnumerator FlashOnce(SpriteRenderer sr, Color flashColor, float duration = 0.3f)
    {
        // すでに点滅中なら同時実行しない
        if (sr == null || flashingSet.Contains(sr)) yield break;

        flashingSet.Add(sr);

        Color original = sr.color;
        sr.color = flashColor;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            // 途中でオブジェクトが消える場合の安全処理
            if (sr == null || sr.gameObject == null)
            {
                flashingSet.Remove(sr);
                yield break;
            }

            // flashColor → original に戻っていく
            sr.color = Color.Lerp(flashColor, original, t / duration);
            yield return null;
        }

        // 最後に元の色へ
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
    //-----------------------------------------------
    // Line hit detection (Reverse Capture Effect)
    //-----------------------------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!edgeCol.enabled) return;
        // Only trigger if line touches a ghost (Dog)
        if (other.CompareTag("Dog"))
        {
            // Get the target ghost transform
            Transform target = other.transform;

            // Play reverse effect (glow travels backwards along the line)
            LineVisualEffectManager.Instance.PlayReverseCaptureEffect(_rend, target.transform);

            RestoreAllGhosts();
            ResetLine();
            CleanPolygon();

        }
    }

}
