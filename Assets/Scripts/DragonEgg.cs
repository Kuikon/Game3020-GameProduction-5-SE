using UnityEngine;
using System.Collections;

public class DragonEgg : MonoBehaviour
{
    [Header("Prefabs & Animation")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject hatchEffectPrefab;
    [SerializeField] private Animator animator;

    [Header("Hatching Settings")]
    [SerializeField] private int requiredCaptureCount = 10;
    [SerializeField] private float shakeAmplitude = 0.05f; // 🔹揺れの強さ
    [SerializeField] private float shakeSpeed = 2f;        // 🔹揺れの速さ

    private int currentCaptured = 0;
    private bool hatched = false;
    private bool hatching = false; // 孵化中かどうか
    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
    }

    private void OnEnable() => GhostEvents.OnGhostCaptured += OnGhostCaptured;
    private void OnDisable() => GhostEvents.OnGhostCaptured -= OnGhostCaptured;

    private void Update()
    {
        if (!hatched)
        {
            float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmplitude;
            float offsetY = Mathf.Cos(Time.time * shakeSpeed * 0.5f) * (shakeAmplitude * 0.5f);
            transform.position = initialPos + new Vector3(offsetX, offsetY, 0f);
        }
    }

    private void OnGhostCaptured(GhostType type, Vector3 pos)
    {
        if (hatched) return;
        if (type != GhostType.Normal) return;

        currentCaptured++;
        Debug.Log($"✅ Captured {currentCaptured}/{requiredCaptureCount} Normal ghosts");

        // ゴーストを捕獲するたびに少し揺れを強く
        shakeAmplitude = Mathf.Lerp(shakeAmplitude, 0.1f, 0.3f);

        if (currentCaptured >= requiredCaptureCount && !hatching)
        {
            StartCoroutine(HatchSequence());
        }
    }

    private IEnumerator HatchSequence()
    {
        hatching = true;
        Debug.Log("🐣 Dragon egg is hatching...");

        // アニメーション再生
        if (animator != null)
            animator.SetTrigger("Hatch");

       

        // 孵化中は激しく揺れる
        float hatchTime = 2f;
        float elapsed = 0f;
        while (elapsed < hatchTime)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(Time.time * 25f) * 0.15f;
            transform.position = initialPos + new Vector3(shake, 0f, 0f);
            yield return null;
        }
        // 光や爆発エフェクト
        if (hatchEffectPrefab != null)
        {
            GameObject fx = Instantiate(hatchEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 0.5f);
        }
        // 卵削除
        hatched = true;
        transform.position = initialPos;
        Destroy(gameObject);

        // ドラゴン生成
        if (bossPrefab != null)
        {
            Instantiate(bossPrefab, initialPos, Quaternion.identity);
            Debug.Log("🐉 Dragon has hatched!");
        }
    }
}
