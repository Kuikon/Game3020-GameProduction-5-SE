using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Light2DRadiusController : MonoBehaviour
{
    public static Light2DRadiusController Instance { get; private set; }
    [Header("2D Light Settings")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float minRadius = 1f;
    [SerializeField] private float maxRadius = 10f;

    [Header("Brightness Settings")]
    [SerializeField] private float maxIntensity = 1.2f; 
    [SerializeField] private float minIntensity = 0.2f; 
    public  float fadeSpeed = 0.25f;   
    [SerializeField] private float expandAmount = 5f;   
    [SerializeField] public float flashDuration = 0.3f;

    private Coroutine fadeRoutine;
   

    private void Awake()
    {
        Instance = this;
    }
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

        // While the fadeRoutine is not running, it gradually darkens.
        if (fadeRoutine == null)
        {
            light2D.intensity = Mathf.Lerp(light2D.intensity, minIntensity, Time.deltaTime * fadeSpeed);
            light2D.pointLightOuterRadius = Mathf.Lerp(light2D.pointLightOuterRadius, minRadius, Time.deltaTime * fadeSpeed);
        }
    }

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

        // Bright and Spreading
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            light2D.intensity = Mathf.Lerp(light2D.intensity, maxIntensity, t);
            light2D.pointLightOuterRadius = Mathf.Lerp(startRadius, targetRadius, t);
            yield return null;
        }

        // Keep it a little brighter
        yield return new WaitForSeconds(flashDuration);

        // Automatic fade-out
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
