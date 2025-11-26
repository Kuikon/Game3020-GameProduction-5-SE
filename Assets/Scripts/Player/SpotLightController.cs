using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Light2DRadiusController : MonoBehaviour
{
    [Header("2D Light Settings")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float minRadius = 1f;
    [SerializeField] private float maxRadius = 10f;

    [Header("Brightness Settings")]
    [SerializeField] private float maxIntensity = 1.2f; // 明るくなったときの最大
    [SerializeField] private float minIntensity = 0.2f; // 暗くなったときの最小
    public  float fadeSpeed = 0.25f;   // 自動で暗く戻る速度
    [SerializeField] private float expandAmount = 5f;   // 光の広がる量
    [SerializeField] public float flashDuration = 0.3f;// 明るい状態の維持時間

    private Coroutine fadeRoutine;

    private void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        light2D.lightType = Light2D.LightType.Point;
        light2D.intensity = minIntensity;
    }

    private void Update()
    {
        if (light2D == null) return;

        // 💫 fadeRoutineが動いていない間は、徐々に暗くなる
        if (fadeRoutine == null)
        {
            light2D.intensity = Mathf.Lerp(light2D.intensity, minIntensity, Time.deltaTime * fadeSpeed);
            light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, minRadius, Time.deltaTime * fadeSpeed);
        }
    }

    // 💥 囲み（交差）が発生したときに呼ぶ
    public void FlashRadius()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FlashAndFadeRoutine());
    }

    private IEnumerator FlashAndFadeRoutine()
    {
        float startRadius = light2D.pointLightOuterRadius;
        float targetRadius = Mathf.Min(startRadius + expandAmount, maxRadius);
        float t = 0f;

        // 🔆 明るく広がる
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            light2D.intensity = Mathf.Lerp(light2D.intensity, maxIntensity, t);
            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, targetRadius, t);
            yield return null;
        }

        // 明るい状態を少しキープ
        yield return new WaitForSeconds(flashDuration);

        // 🌙 自動で暗くフェードアウト
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            light2D.intensity = Mathf.Lerp(maxIntensity, minIntensity, t);
            light2D.pointLightOuterRadius = Mathf.Lerp(targetRadius, startRadius, t);
            yield return null;
        }

        fadeRoutine = null;
    }
}
