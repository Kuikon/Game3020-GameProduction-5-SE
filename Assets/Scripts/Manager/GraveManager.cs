using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraveManager : MonoBehaviour
{
    [Header("Grave References")]
    [SerializeField] private List<GameObject> gravesInScene = new();
    [SerializeField] private GameObject brokenGravePrefab;

    private readonly List<GameObject> graveList = new();

    // =========================================================
    // Initialization
    // =========================================================
    public void InitializeGraves()
    {
        graveList.Clear();

        foreach (var g in gravesInScene)
        {
            if (g != null)
            {
                g.SetActive(true);
                graveList.Add(g);
                MiniMapManager.Instance?.RegisterGrave(g);
            }
        }

        Debug.Log($"🪦 Initialized {graveList.Count} graves.");
    }

    // =========================================================
    // Patrol Points
    // =========================================================
    public List<Vector3> GetPatrolPoints()
    {
        List<Vector3> points = new();

        foreach (var g in graveList)
        {
            if (g != null && g.activeInHierarchy)
                points.Add(g.transform.position);
        }

        return points;
    }

    // =========================================================
    // Replace destroyed graves (called from LineDraw)
    // =========================================================
    public void ReplaceCapturedGraves(List<GameObject> captured)
    {
        foreach (var g in captured)
        {
            if (g == null) continue;

            Vector3 pos = g.transform.position;
            g.SetActive(false);

            // 壊れた墓を配置
            if (brokenGravePrefab != null)
            {
                GameObject broken = Instantiate(brokenGravePrefab, pos, Quaternion.identity);
                broken.tag = "BrokenGrave";
            }

            Debug.Log($"💀 Grave replaced at {pos}");
        }
    }

    // =========================================================
    // Check if a broken grave exists at a position
    // =========================================================
    public GameObject GetBrokenGraveAt(Vector3 pos, float radius = 0.2f)
    {
        foreach (var b in GameObject.FindGameObjectsWithTag("BrokenGrave"))
        {
            if (Vector3.Distance(b.transform.position, pos) < radius)
                return b;
        }
        return null;
    }

    // =========================================================
    // Repair a broken grave
    // =========================================================
    public void RepairBrokenGrave(GameObject broken)
    {
        if (broken == null) return;

        Vector3 pos = broken.transform.position;
        Destroy(broken);

        // 通常の墓を復活
        foreach (var normal in gravesInScene)
        {
            if (Vector3.Distance(normal.transform.position, pos) < 0.1f)
            {
                normal.SetActive(true);
                break;
            }
        }

        Debug.Log($"🧱 Repaired grave at {pos}");
    }
}
