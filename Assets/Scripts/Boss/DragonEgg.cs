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
    [SerializeField] private float shakeAmplitude = 0.05f; 
    [SerializeField] private float shakeSpeed = 2f;       

    private int currentCaptured = 0;
    private bool hatched = false;
    private bool hatching = false; 
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
        shakeAmplitude = Mathf.Lerp(shakeAmplitude, 0.1f, 0.3f);

        if (currentCaptured >= requiredCaptureCount && !hatching)
        {
            StartCoroutine(HatchSequence());
        }
    }

    private IEnumerator HatchSequence()
    {
        hatching = true;
        if (animator != null)
            animator.SetTrigger("Hatch");
        float hatchTime = 2f;
        float elapsed = 0f;
        while (elapsed < hatchTime)
        {
            elapsed += Time.deltaTime;
            float shake = Mathf.Sin(Time.time * 25f) * 0.15f;
            transform.position = initialPos + new Vector3(shake, 0f, 0f);
            yield return null;
        }
        if (hatchEffectPrefab != null)
        {
            GameObject fx = Instantiate(hatchEffectPrefab, transform.position, Quaternion.identity);
            SoundManager.Instance.PlaySE(SESoundData.SE.HatchExplosion);
            Destroy(fx, 0.5f);
        }
        hatched = true;
        transform.position = initialPos;
      
        GameObject boss = null;
        if (bossPrefab != null)
        {
            boss = Instantiate(bossPrefab, transform.position, Quaternion.identity);
            BossBehaviour bossBehaviour = boss.GetComponent<BossBehaviour>();
            GraveManager gm = FindFirstObjectByType<GraveManager>();
            SoundManager.Instance.PlaySE(SESoundData.SE.DragonHatch);
            bossBehaviour.InitializeAfterHatch(gm);
            Destroy(gameObject);
            Debug.Log("🐉 Dragon has hatched!");
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDragonUI(true);
            Debug.Log("🎯 Dragon UI Activated!");
        }
        if (MiniMapManager.Instance != null && boss != null)
            MiniMapManager.Instance.RegisterDragon(boss);
    }
}
