using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraveManager : MonoBehaviour
{
    [Header("Grave References")]
    [Tooltip("シーン上に最初から配置されている墓（8個）をここに登録")]
    [SerializeField] private List<GameObject> gravesInScene = new();  // ← ここがメイン
    [SerializeField] private GameObject brokenGravePrefab;

    [Header("Spawn Settings (optional, fallback)")]
    [SerializeField] private GameObject gravePrefab;
    [SerializeField] private Boundry xBoundary;
    [SerializeField] private Boundry yBoundary;
    [SerializeField] private int initialCount = 8;

    private List<GameObject> graveList = new();
    private int lastGraveCount;

    // =========================================================
    // Initialization
    // =========================================================
    public void InitializeGraves()
    {
        graveList.Clear();

        // 🟢 シーン上に手動配置された墓がある場合
        if (gravesInScene != null && gravesInScene.Count > 0)
        {
            foreach (var g in gravesInScene)
            {
                if (g != null)
                {
                    g.SetActive(true);
                    graveList.Add(g);
                }
            }

            lastGraveCount = graveList.Count;
            Debug.Log($"🪦 Initialized {graveList.Count} graves from scene.");
        }
        else
        {
            // ⚙️ フォールバック：Prefabをランダム生成
            lastGraveCount = initialCount;
            SpawnGraves(initialCount);
            Debug.Log($"⚙️ Spawned {initialCount} graves randomly.");
        }
    }

    // =========================================================
    // Patrol Points
    // =========================================================
    public List<Vector3> GetPatrolPoints()
    {
        List<Vector3> points = new();

        foreach (var g in graveList)
        {
            if (g != null && g.activeInHierarchy && !g.CompareTag("BrokenGrave"))
                points.Add(g.transform.position);
        }

        return points;
    }

    // =========================================================
    // Replace / Rebuild Logic
    // =========================================================
    public void ReplaceCapturedGraves(List<GameObject> captured)
    {
        foreach (var g in captured)
        {
            if (g == null) continue;

            Vector3 pos = g.transform.position;
            graveList.Remove(g);
            g.SetActive(false);  // 🟢 破壊された墓は非アクティブにする

            // 壊れた墓の見た目だけ差し替え
            if (brokenGravePrefab != null)
            {
                GameObject broken = Instantiate(brokenGravePrefab, pos, Quaternion.identity);
                broken.tag = "BrokenGrave";
            }
        }
    }

    public IEnumerator RebuildGravesRoutine()
    {
        yield return new WaitForSeconds(2f);

        RemoveAllBroken();

        // 🟢 既存の墓を再アクティブ化
        foreach (var g in gravesInScene)
        {
            if (g != null)
                g.SetActive(true);
        }

        // 🟢 管理リストを更新
        graveList.Clear();
        graveList.AddRange(gravesInScene);

        Debug.Log($"🧱 Graves rebuilt and reactivated ({graveList.Count}).");
    }

    public bool AllGravesBroken()
    {
        foreach (var g in graveList)
        {
            if (g != null && g.activeInHierarchy && !g.CompareTag("BrokenGrave"))
                return false;
        }
        return true;
    }

    private void RemoveAllBroken()
    {
        foreach (var g in GameObject.FindGameObjectsWithTag("BrokenGrave"))
            Destroy(g);
    }

    // =========================================================
    // Optional random placement fallback
    // =========================================================
    private void SpawnGraves(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetValidPosition(2f);
            GameObject g = Instantiate(gravePrefab, pos, Quaternion.identity);
            graveList.Add(g);
        }
    }

    private Vector3 GetValidPosition(float minDist)
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 pos = new(
                Random.Range(xBoundary.min, xBoundary.max),
                Random.Range(yBoundary.min, yBoundary.max),
                0
            );

            bool valid = true;
            foreach (var existing in graveList)
            {
                if (existing != null && Vector3.Distance(pos, existing.transform.position) < minDist)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return pos;
        }
        return Vector3.zero;
    }
}
