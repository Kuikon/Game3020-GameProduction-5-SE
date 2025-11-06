using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using static UnityEngine.GraphicsBuffer;

public class LineVisualEffectManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject glowPrefab; 
    [SerializeField] private float defaultGlowScale = 0.3f;
    [SerializeField] private Color captureColor = Color.yellow;
    [SerializeField] private Material fadeMaterial;
    [SerializeField] private float afterImageLifetime = 1.5f;
    [SerializeField] private Color afterImageColor = new Color(0f, 1f, 1f, 0.6f);

    private List<GameObject> activeEffects = new();
    private const int maxActiveEffects = 50;
    public static LineVisualEffectManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    // 💥 ライン交差時（例：CrossPoint検出時）
    public void CreateLineAfterImage(LineRenderer sourceLine)
    {
        if (sourceLine == null) return;

        // 残像用の空オブジェクトを作成
        GameObject afterImageObj = new GameObject("LineAfterImage");
        LineRenderer afterLine = afterImageObj.AddComponent<LineRenderer>();

        // Lineの見た目をコピー
        afterLine.positionCount = sourceLine.positionCount;
        Vector3[] positions = new Vector3[sourceLine.positionCount];
        sourceLine.GetPositions(positions);
        afterLine.SetPositions(positions);
        afterLine.material = new Material(fadeMaterial);
        afterLine.startWidth = sourceLine.startWidth;
        afterLine.endWidth = sourceLine.endWidth;
        afterLine.startColor = afterImageColor;
        afterLine.endColor = afterImageColor;
        afterLine.sortingOrder = sourceLine.sortingOrder + 1;
        // フェードアウト処理開始
        StartCoroutine(FadeAndDestroy(afterLine));
    }
    private IEnumerator FadeAndDestroy(LineRenderer line)
    {
        float elapsed = 0f;
        Material mat = line.material;
        Color startColor = afterImageColor;

        while (elapsed < afterImageLifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / afterImageLifetime);
            Color c = new Color(startColor.r, startColor.g, startColor.b, alpha);
            line.startColor = c;
            line.endColor = c;

            if (mat != null && mat.HasProperty("_Color"))
                mat.color = c;

            yield return null;
        }

        if (line != null)
            Destroy(line.gameObject);
    }

    // ✨ 囲み完了時（Loop完成時）
    public void PlayCaptureEffect(LineRenderer line, Transform target, bool reverse = false)
    {
        if (line == null) return;

        Vector3[] points = new Vector3[line.positionCount];
        line.GetPositions(points);
        int step = Mathf.Max(2, line.positionCount / 200);

        // 🔹 逆方向ならループ順を反転
        if (reverse)
        {
            System.Array.Reverse(points);
        }

        for (int i = 0; i < points.Length; i += step)
        {
            GameObject glow = SpawnGlow(points[i], captureColor, defaultGlowScale);
            activeEffects.Add(glow);

            GlowMover mover = glow.GetComponent<GlowMover>();
            if (mover != null)
            {
                if (reverse)
                    mover.SetReverseTarget(target); // 逆モード
                else
                    mover.SetTarget(target);        // 通常モード
            }
        }
    }

    public void ReleaseAllGlowsUpward()
    {
        foreach (var glow in activeEffects)
        {
            if (glow == null) continue;

            var mover = glow.GetComponent<GlowMover>();
            if (mover != null)
            {
                mover.ReleaseUpward(); // ⬆️ 上向きモードへ
            }
        }

        // リストを掃除
        activeEffects.RemoveAll(g => g == null);
    }
    // 🎯 ゴーストヒット時（当たり判定時）
    public void PlayHitEffect(Vector3 position)
    {
        SpawnGlow(position, Color.white, defaultGlowScale * 1.2f);
    }

    // 🌀 汎用エフェクト生成
    private GameObject SpawnGlow(Vector3 position, Color color, float scale)
    {
        GameObject glow = Instantiate(glowPrefab, position, Quaternion.identity);
        var ps = glow.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color;
            main.startSize = scale;
        }

        return glow;
    }

    // 🔄 エフェクト寿命管理（Destroyと同時にリスト除外）
    private System.Collections.IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
        activeEffects.Remove(obj);
    }

    // 🧹 念のため毎数秒にリスト掃除（安全対策）
    private float cleanupTimer = 0f;
    private void LateUpdate()
    {
        cleanupTimer += Time.deltaTime;
        if (cleanupTimer > 3f)
        {
            activeEffects.RemoveAll(e => e == null);
            cleanupTimer = 0f;
        }
    }
}
