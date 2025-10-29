using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LineDraw : MonoBehaviour
{
    [SerializeField] private LineRenderer _rend;
    [SerializeField] private Camera _cam;
    [SerializeField] private float maxLineLength = 5f; 
    private float currentLineLength = 0f;
    private Queue<Vector3> linePoints = new();
    private int posCount = 0;
    private float interval = 0.1f;
    private PolygonCollider2D _poly;

    public float lineLifeTime = 2.5f;
    public Dictionary<GameObject, int> insideCount { get; private set; }

    void Awake()
    {
        insideCount = new Dictionary<GameObject, int>();
    }

    private void Start()
    {
        _rend.positionCount = 0;
        _rend.startWidth = 0.1f;
        _rend.endWidth = 0.1f;
        _rend.useWorldSpace = true;
        _rend.material = new Material(Shader.Find("Sprites/Default"));

        if (_cam == null)
            _cam = Camera.main;
    }

    private void Update()
    {
        if (BallController.IsAnyBallBeingDragged)
        {
            if (_rend.positionCount > 0)
                ResetLine();
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        if (!new Rect(0, 0, Screen.width, Screen.height).Contains(mousePos))
            return;

        mousePos.z = Mathf.Abs(_cam.transform.position.z);
        mousePos = _cam.ScreenToWorldPoint(mousePos);
        mousePos.z = 0f;

        if (Input.GetMouseButton(0))
        {
            SetPosition(mousePos);
            if (posCount > 2)
                CheckIntersection();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetLine();
            HideAllTextsWithTag("Dog");
            HideAllTextsWithTag("Grave");
        }
    }

    // ============================================================
    // 線を管理する処理
    // ============================================================
    private void SetPosition(Vector3 pos)
    {
        if (!PosCheck(pos)) return;

        if (posCount > 0)
            currentLineLength += Vector3.Distance(_rend.GetPosition(posCount - 1), pos);

        posCount++;
        _rend.positionCount = posCount;
        _rend.SetPosition(posCount - 1, pos);
        linePoints.Enqueue(pos);

        TrimLineToMaxLength();
    }
    private void TrimLineToMaxLength()
    {
        while (linePoints.Count >= 2 && GetTotalLength(linePoints) > maxLineLength)
        {
            // 最古の点を削除
            linePoints.Dequeue();

            // LineRendererを更新
            Vector3[] updated = linePoints.ToArray();
            _rend.positionCount = updated.Length;
            _rend.SetPositions(updated);
        }

        posCount = linePoints.Count;
    }
    private float GetTotalLength(IEnumerable<Vector3> points)
    {
        float length = 0f;
        Vector3? prev = null;
        foreach (var p in points)
        {
            if (prev.HasValue)
                length += Vector3.Distance(prev.Value, p);
            prev = p;
        }
        return length;
    }
    private bool PosCheck(Vector3 pos)
    {
        if (posCount == 0) return true;
        float distance = Vector3.Distance(_rend.GetPosition(posCount - 1), pos);
        return distance > interval;
    }

    private void ResetLine()
    {
        posCount = 0;
        _rend.positionCount = 0;
        currentLineLength = 0f;
        linePoints.Clear();
    }

    // ============================================================
    // 線が交差したときの処理（囲み検出）
    // ============================================================
    private void CheckIntersection()
    {
        Vector3 p1 = _rend.GetPosition(posCount - 2);
        Vector3 p2 = _rend.GetPosition(posCount - 1);

        for (int i = 0; i < posCount - 3; i++)
        {
            Vector3 p3 = _rend.GetPosition(i);
            Vector3 p4 = _rend.GetPosition(i + 1);

            if (LineSegmentsIntersect(p1, p2, p3, p4, out Vector2 intersection))
            {
                CreatePolygonFromLoop(i, intersection);
                return;
            }
        }
    }

    private void CreatePolygonFromLoop(int startIndex, Vector2 intersection)
    {
        List<Vector2> loopPoints = new List<Vector2>();
        for (int j = startIndex + 1; j < posCount; j++)
        {
            Vector3 wp = _rend.GetPosition(j);
            loopPoints.Add(transform.InverseTransformPoint(wp));
        }
        loopPoints.Add(intersection);

        if (_poly != null) Destroy(_poly);
        _poly = gameObject.AddComponent<PolygonCollider2D>();
        _poly.isTrigger = true;
        _poly.points = loopPoints.ToArray();

        CheckObjectsInside();  // 🔹囲まれたオブジェクトを確認
        Destroy(_poly);
        ResetLine();
    }

    // ============================================================
    // 囲み検出処理のメイン関数
    // ============================================================
    private void CheckObjectsInside()
    {
        bool isMouseHeld = Input.GetMouseButton(0);

        CheckGhosts(isMouseHeld);
        CheckGraves(isMouseHeld);
    }

    // ------------------------------------------------------------
    // 👻 ゴースト処理
    // ------------------------------------------------------------
    private void CheckGhosts(bool isMouseHeld)
    {
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Dog");

        foreach (GameObject ghost in ghosts)
        {
            if (!IsInsidePolygon(ghost)) continue;

            IncrementCaptureCount(ghost);
            if (isMouseHeld)
                UIManager.Instance.ShowOverheadText(ghost, insideCount[ghost].ToString(), Color.yellow);

            if (insideCount[ghost] == 3)
                HandleGhostCaptured(ghost);
        }

        if (!isMouseHeld)
            HideAllTextsWithTag("Dog");
    }

    // ------------------------------------------------------------
    // 🪦 墓処理
    // ------------------------------------------------------------
    private void CheckGraves(bool isMouseHeld)
    {
        GameObject[] graves = GameObject.FindGameObjectsWithTag("Grave");

        foreach (GameObject grave in graves)
        {
            if (!IsInsidePolygon(grave)) continue;

            IncrementCaptureCount(grave);
            if (isMouseHeld)
                UIManager.Instance.ShowOverheadText(grave, insideCount[grave].ToString(), Color.yellow);

            if (insideCount[grave] == 5)
                HandleGraveCaptured(grave);
        }

        if (!isMouseHeld)
            HideAllTextsWithTag("Grave");
    }

    // ------------------------------------------------------------
    // 🧩 共通ユーティリティ関数
    // ------------------------------------------------------------
    private bool IsInsidePolygon(GameObject obj)
    {
        Vector2 localPos = transform.InverseTransformPoint(obj.transform.position);
        return _poly != null && _poly.OverlapPoint(localPos);
    }

    private void IncrementCaptureCount(GameObject obj)
    {
        if (!insideCount.ContainsKey(obj))
            insideCount[obj] = 0;
        insideCount[obj]++;
    }

    private void HandleGhostCaptured(GameObject ghost)
    {
        UIManager.Instance.HideOverheadText(ghost);
        var gb = ghost.GetComponent<GhostBase>();
        if (gb != null) gb.Kill();
        Debug.Log($"{ghost.name} Captured 3");
    }

    private void HandleGraveCaptured(GameObject grave)
    {
        Debug.Log($"🪦 {grave.name} Grave captured!");
        HighlightGrave(grave);
        UIManager.Instance.HideOverheadText(grave);

        BossBehaviour boss = FindFirstObjectByType<BossBehaviour>();
        if (boss != null)
        {
            List<GameObject> captured = new List<GameObject> { grave };
            boss.ReplaceCapturedGraves(captured);
        }
    }

    private void HideAllTextsWithTag(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in objs)
            UIManager.Instance.HideOverheadText(obj);
    }

    // ------------------------------------------------------------
    // 🟡 視覚エフェクト処理
    // ------------------------------------------------------------
    private void HighlightGrave(GameObject target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.yellow;
            StartCoroutine(ResetColor(sr, 1.5f));
        }
    }

    private IEnumerator ResetColor(SpriteRenderer sr, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (sr != null)
            sr.color = Color.white;
    }

    // ============================================================
    // 線の交差判定
    // ============================================================
    private bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        float d = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        if (Mathf.Approximately(d, 0f)) return false;

        float u = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;
        float v = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;

        if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
        {
            intersection = p1 + v * (p2 - p1);
            return true;
        }
        return false;
    }
}
