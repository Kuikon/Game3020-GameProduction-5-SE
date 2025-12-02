using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform ghostPointsRoot;   
    private Transform[] spawnPoints;
        [Header(" Spawn Settings")]
    [SerializeField] GameObject[] ghostPrefabs;
    [SerializeField] GameObject spawnEffectPrefab;
    [SerializeField] float delayBeforeSpawn = 0.5f;
    [SerializeField] private int initialSpawnCount = 10;
    [SerializeField] float spawnInterval = 2f;
    [SerializeField] private GhostType targetType = GhostType.Normal;

    public void BeginSpawnFromGhostPoints()
    {
        LoadGhostPoints();
        StartCoroutine(SpawnGhostsFromPointsRoutine());
    }
    private void LoadGhostPoints()
    {
        if (ghostPointsRoot == null)
        {
            GameObject obj = GameObject.Find("GhostPoints");
            if (obj != null)
                ghostPointsRoot = obj.transform;
            else
            {
                Debug.LogError("❌ GhostPoints object not found in scene!");
                return;
            }
        }

        // 子だけ取得（ルート自身は除外）
        spawnPoints = ghostPointsRoot.GetComponentsInChildren<Transform>();

        // 先頭は root 自身 → スキップする
        if (spawnPoints.Length > 0)
        {
            List<Transform> list = new List<Transform>(spawnPoints);
            list.RemoveAt(0); 

            spawnPoints = list.ToArray();
        }

        Debug.Log($"📌 Loaded {spawnPoints.Length} GhostPoints!");
    }
    IEnumerator SpawnGhostsFromPointsRoutine()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (spawnPoints.Length > 0)
            {
                // ランダムなポイントを1つ選ぶ
                Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

                if (point != null)
                {
                    GameObject prefab = GetPrefabByType(targetType);
                    if (prefab != null)
                        StartCoroutine(SpawnSequence(point.position, prefab));
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }


    private GameObject GetPrefabByType(GhostType type)
    {
        foreach (var p in ghostPrefabs)
        {
            GhostBase g = p.GetComponent<GhostBase>();
            if (g != null && g.data.type == type)
                return p;
        }
        return null;
    }
    private IEnumerator SpawnSequence(Vector3 pos, GameObject prefab)
    {
        if (spawnEffectPrefab != null)
        {
            Vector3 effectPos = pos + new Vector3(0f, -.5f, 0f);
            GameObject effect = Instantiate(spawnEffectPrefab, effectPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        yield return new WaitForSeconds(delayBeforeSpawn);

        Instantiate(prefab, pos, Quaternion.identity);
    }
    public void SpawnAroundPointsGradually(
        Transform point,
        int count = 3,
        float radius = 1.5f,
        float interval = 0.3f,
        float fadeDuration = 1f)
    {
        StartCoroutine(SpawnGraduallyRoutine(point, count, radius, interval, fadeDuration));
    }
    public void SpawnAtPosition(Vector3 pos)
    {
        if (ghostPrefabs.Length == 0) return;

        GameObject ghostPrefab = ghostPrefabs[Random.Range(0, ghostPrefabs.Length)];
        StartCoroutine(SpawnSequence(pos, ghostPrefab));
    }
    IEnumerator SpawnGraduallyRoutine(
        Transform point,
        int count,
        float radius,
        float interval,
        float fadeDuration)
    {
        GameObject normalPrefab = GetPrefabByType(GhostType.Normal);
        if (normalPrefab == null) yield break;

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 pos = point.position + new Vector3(offset.x, offset.y, 0);

            GameObject ghost = Instantiate(normalPrefab, pos, Quaternion.identity);
            SpriteRenderer sr = ghost.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0;
                sr.color = c;

                StartCoroutine(FadeIn(sr, fadeDuration));
            }

            yield return new WaitForSeconds(interval);
        }
    }
    IEnumerator FadeIn(SpriteRenderer sr, float duration)
    {
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / duration);
            Color c = sr.color;
            c.a = a;
            sr.color = c;
            yield return null;
        }
    }
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-5f, 5f);
        float y = Random.Range(-3f, 3f);
        return new Vector3(x, y, 0f);
    }
}
