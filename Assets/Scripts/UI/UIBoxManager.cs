using UnityEngine;
using System.Collections.Generic;

public class UIBoxManager : MonoBehaviour
{
    [Header("UIBox Prefab")]
    [SerializeField] private GameObject uiBoxPrefab;

    [Header("Layout Settings")]
    [SerializeField] private float radius = 2.5f;   // 配置半径
    [SerializeField] private int count = 4;         // 配置数（例：上下左右）
    [SerializeField] private bool circular = false; // trueなら円形配置

    private List<GameObject> activeBoxes = new();

    // ======= 生成 =======
    public void SpawnBoxes(Vector3 center)
    {
        if (uiBoxPrefab == null)
        {
            Debug.LogWarning("⚠️ UIBox Prefab not assigned!");
            return;
        }

        if (activeBoxes.Count > 0)
        {
            Debug.Log("UIBoxes already exist — skipping spawn.");
            return;
        }

        if (circular)
        {
            SpawnCircular(center);
        }
        else
        {
            SpawnCross(center);
        }
    }

    // ======= 消去 =======
    public void ClearBoxes()
    {
        foreach (var box in activeBoxes)
        {
            if (box != null)
                Destroy(box);
        }
        activeBoxes.Clear();
    }

    // ───────────────────────────────
    private void SpawnCross(Vector3 center)
    {
        Vector3[] offsets = {
            new Vector3( radius, 0, 0),  // 右
            new Vector3(-radius, 0, 0),  // 左
            new Vector3(0,  radius, 0),  // 上
            new Vector3(0, -radius, 0)   // 下
        };

        GhostType[] types = {
            GhostType.Normal,
            GhostType.Suicide,
            GhostType.Quick,
            GhostType.Tank
        };

        for (int i = 0; i < Mathf.Min(count, offsets.Length); i++)
        {
            GameObject box = Instantiate(uiBoxPrefab, center + offsets[i], Quaternion.identity);
            var uibox = box.GetComponent<UIBox>();
            if (uibox != null)
                uibox.boxType = types[i];

            activeBoxes.Add(box);
        }

        Debug.Log($"🟩 Spawned {activeBoxes.Count} UIBoxes around base.");
    }

    private void SpawnCircular(Vector3 center)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2 / count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            GameObject box = Instantiate(uiBoxPrefab, center + offset, Quaternion.identity);
            activeBoxes.Add(box);
        }
    }
}
