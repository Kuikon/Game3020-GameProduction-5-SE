using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header(" Spawn Settings")]
    [SerializeField] GameObject[] ghostPrefabs;
    [SerializeField] GameObject spawnEffectPrefab;
    [SerializeField] float delayBeforeSpawn = 0.5f;
    [SerializeField] private int initialSpawnCount = 10;
    [SerializeField] private GhostType targetType = GhostType.Normal;

    private void Start()
    {
        SpawnInitialGhosts();
    }

    //---------------------------------------------------
    // 初期スポーン
    //---------------------------------------------------
    public void SpawnInitialGhosts()
    {
        int count = 0;
        for (int i = 0; i < initialSpawnCount; i++)
        {
            Vector3 pos = GetRandomPosition();
            GameObject prefab = GetPrefabByType(targetType);
            if (prefab != null)
            {
                StartCoroutine(SpawnSequence(pos, prefab));
                count++;
            }
        }
        Debug.Log($"👻 Spawned {count} {targetType} ghosts at start!");
    }

    //---------------------------------------------------
    // 指定タイプのPrefab取得
    //---------------------------------------------------
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

    //---------------------------------------------------
    // 通常スポーン（エフェクトあり）
    //---------------------------------------------------
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

    //---------------------------------------------------
    // 🔥 カメラがポイントに到達した時に呼ぶ
    // ランダム位置に徐々にスポーン（フェードイン付き）
    //---------------------------------------------------
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

            // 🔵 透明度0で開始
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

    //---------------------------------------------------
    // Sprite を透明 → 不透明にフェードイン
    //---------------------------------------------------
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

    //---------------------------------------------------
    // 画面の適当なランダム位置
    //---------------------------------------------------
    private Vector3 GetRandomPosition()
    {
        float x = Random.Range(-5f, 5f);
        float y = Random.Range(-3f, 3f);
        return new Vector3(x, y, 0f);
    }
}
