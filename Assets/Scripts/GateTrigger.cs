using System.Collections;
using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public CameraFollow2D cam;
    public GameObject fence;      // Sprite fore closing fence
    public float fenceDelay = 0.1f;
    public EnemySpawner spawner;
    private Light2DRadiusController lightController;
    private Transform originalLightParent;
    private bool triggered = false;
    void Awake()
    {
        lightController = FindAnyObjectByType<Light2DRadiusController>();

        if (lightController == null)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll<Light2DRadiusController>())
            {
                if (obj.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    lightController = obj;
                    break;
                }
            }
        }

        if (lightController == null)
            Debug.LogError("❌ Light2DRadiusController NOT FOUND");
        else
            Debug.Log("✅ Light2DRadiusController FOUND");
    }

    private void Start()
    {
        if (cam == null)
        {
            CameraFollow2D[] cams = Object.FindObjectsByType<CameraFollow2D>(FindObjectsSortMode.None);

            foreach (var c in cams)
            {
                // pick the camera on DontDestroyOnLoad 
                if (c.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    cam = c;
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        if (lightController != null)
        {
            lightController.isForcedLight = true;
            lightController.StopAllCoroutines();
            originalLightParent = lightController.transform.parent;
            lightController.transform.SetParent(cam.transform);
            var light2d = lightController.GetComponent<UnityEngine.Rendering.Universal.Light2D>();
            if (light2d != null)
            {
                light2d.intensity = lightController.fixedIntensity;       
                light2d.pointLightOuterRadius = lightController.fixedRadius;
            }

            lightController.transform.localPosition = new Vector3(0, 0, 1);
        }
        cam.OnPatrolEnd += ReturnLightToPlayer;
        // Get GhostPoint objects
        GameObject root = GameObject.Find("GhostPoints");
        if (root != null)
        {
            // Get only child objects
            Transform[] children = new Transform[root.transform.childCount];
            for (int i = 0; i < root.transform.childCount; i++)
            {
                children[i] = root.transform.GetChild(i);
            }

            // pass the patrol points to the camera
            cam.SetGhostPoints(children);
        }
        else
        {
            Debug.LogError("GhostPoints object not found in this scene!");
        }

        
        cam.OnReachedPoint += (Transform p) =>
        {
            spawner.SpawnAroundPointsGradually(
                p,
                count: 1,
                radius: 2.0f,
                interval: 0.25f,
                fadeDuration: 1f
            );
        };
        if (fence != null)
            StartCoroutine(CloseFence());
        cam.StartPatrol();
    }
    private void ReturnLightToPlayer()
    {
        if (lightController == null || originalLightParent == null) return;
        lightController.isForcedLight = false;
        lightController.transform.SetParent(originalLightParent);
        lightController.transform.localPosition = Vector3.zero;

        Debug.Log("💡 Light returned to player.");
    }
    IEnumerator CloseFence()
    {
        yield return new WaitForSeconds(fenceDelay);
        fence.SetActive(true);
    }
}
