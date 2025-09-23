using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class LineDraw : MonoBehaviour
{
    [SerializeField] private LineRenderer _rend;
    [SerializeField] private Camera _cam;

    private int posCount = 0;
    private float interval = 0.1f;

    // 作られるポリゴン
    private PolygonCollider2D _poly;

    // オブジェクトが何回囲まれたか記録
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
            _cam = Camera.main; // Inspectorに指定が無ければ自動でMainCameraを使う
    }

    private void Update()
    {
        // マウス座標取得（画面外なら処理しない）
        Vector3 mousePos = Input.mousePosition;
        if (!new Rect(0, 0, Screen.width, Screen.height).Contains(mousePos))
            return;

        mousePos.z = Mathf.Abs(_cam.transform.position.z); // カメラからの距離を設定
        mousePos = _cam.ScreenToWorldPoint(mousePos);
        mousePos.z = 0f;

        // 線を引く処理
        if (Input.GetMouseButton(0))
        {
            SetPosition(mousePos);

            if (posCount > 2)
                CheckIntersection();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ResetLine();
        }
    }

    private void SetPosition(Vector3 pos)
    {
        if (!PosCheck(pos)) return;

        posCount++;
        _rend.positionCount = posCount;
        _rend.SetPosition(posCount - 1, pos);
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
    }

    /// <summary>
    /// 新しい線分と過去の線分が交差していたらポリゴンを作る
    /// </summary>
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
                Debug.Log("交差発見！ポリゴンを生成");

                // 交差点を含めたループの頂点リストを作成
                List<Vector2> loopPoints = new List<Vector2>();
                for (int j = i + 1; j < posCount; j++)
                {
                    Vector3 wp = _rend.GetPosition(j);
                    loopPoints.Add(transform.InverseTransformPoint(wp));
                }
                loopPoints.Add(intersection); // 閉じる

                // PolygonCollider2D を生成
                if (_poly != null) Destroy(_poly);
                _poly = gameObject.AddComponent<PolygonCollider2D>();
                _poly.isTrigger = true;
                _poly.points = loopPoints.ToArray();

                // 中にいるオブジェクトをチェック
                CheckObjectsInside();

                // ポリゴン削除＆線リセット
                Destroy(_poly);
                ResetLine();
                return;
            }
        }
    }

    /// <summary>
    /// 線分交差判定
    /// </summary>
    private bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;
        float d = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
        if (Mathf.Approximately(d, 0f)) return false; // 平行

        float u = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;
        float v = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;

        if (u >= 0 && u <= 1 && v >= 0 && v <= 1)
        {
            intersection = p1 + v * (p2 - p1);
            return true;
        }
        return false;
    }

    /// <summary>
    /// ポリゴンの中にいるオブジェクトをチェックして回数カウント
    /// </summary>
    private void CheckObjectsInside()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Dog");

        foreach (GameObject t in targets)
        {
            Vector2 localPos = transform.InverseTransformPoint(t.transform.position);
            if (_poly.OverlapPoint(localPos))
            {
                if (!insideCount.ContainsKey(t))
                    insideCount[t] = 0;

                insideCount[t]++;

                if (insideCount[t] == 3)
                {
                    var gb = t.GetComponent<GhostBase>();
                    if (gb != null)
                    {
                        gb.Kill();
                    }
                    Debug.Log($"{t.name} は3回囲まれました！");
                }
                else
                {
                    Debug.Log($"{t.name} は {insideCount[t]} 回囲まれました");
                }
            }
        }
    }
}
