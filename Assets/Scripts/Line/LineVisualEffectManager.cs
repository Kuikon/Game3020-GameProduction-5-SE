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
    [SerializeField] private Color reverseColor = Color.red;
    private List<GameObject> activeEffects = new();
    private const int maxActiveEffects = 50;
    public static LineVisualEffectManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    public void CreateLineAfterImage(LineRenderer sourceLine)
    {
        if (sourceLine == null) return;
        GameObject afterImageObj = new GameObject("LineAfterImage");
        LineRenderer afterLine = afterImageObj.AddComponent<LineRenderer>();
        afterLine.positionCount = sourceLine.positionCount;
        Vector3[] positions = new Vector3[sourceLine.positionCount];
        sourceLine.GetPositions(positions);
        afterLine.SetPositions(positions);
        afterLine.material = new Material(fadeMaterial);
        afterLine.startWidth = sourceLine.startWidth;
        afterLine.endWidth = sourceLine.endWidth;
        afterLine.startColor = afterImageColor;
        afterLine.endColor = afterImageColor;
        afterLine.sortingLayerName = sourceLine.sortingLayerName;
        afterLine.sortingOrder = sourceLine.sortingOrder + 1;
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
    public void PlayCaptureEffect(LineRenderer line, Transform target)
    {
        if (line == null) return;

        Vector3[] points = new Vector3[line.positionCount];
        line.GetPositions(points);

        int step = Mathf.Max(2, line.positionCount / 400);

        for (int i = 0; i < points.Length; i += step)
        {
            GameObject glow = SpawnGlow(points[i], captureColor, defaultGlowScale);
            activeEffects.Add(glow);

            GlowMover mover = glow.GetComponent<GlowMover>();
            if (mover != null)
                mover.SetTarget(target);
        }
    }
    public void PlayReverseCaptureEffect(LineRenderer line, Transform target)
    {
        if (line == null) return;

        Vector3[] points = new Vector3[line.positionCount];
        line.GetPositions(points);
        System.Array.Reverse(points);

        int step = Mathf.Max(2, line.positionCount / 400);

        for (int i = 0; i < points.Length; i += step)
        {
            GameObject glow = SpawnGlow(points[i], reverseColor, defaultGlowScale);
            activeEffects.Add(glow);

            GlowMover mover = glow.GetComponent<GlowMover>();
            if (mover != null)
                mover.SetReverseTarget(target); 
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
                mover.ReleaseUpward(); 
            }
        }
        activeEffects.RemoveAll(g => g == null);
    }
    public void PlayHitEffect(Vector3 position)
    {
        SpawnGlow(position, Color.white, defaultGlowScale * 1.2f);
    }

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
        Destroy(glow, 1.2f);

        return glow;
    }
    private System.Collections.IEnumerator DestroyAfter(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
            Destroy(obj);
        activeEffects.Remove(obj);
    }
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
