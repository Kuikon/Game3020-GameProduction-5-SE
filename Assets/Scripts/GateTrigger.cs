using System.Collections;
using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public CameraFollow2D cam;
    public GameObject fence;      // Sprite fore closing fence
    public float fenceDelay = 0.1f;
    public EnemySpawner spawner;

    private bool triggered = false;
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
                count: 5,
                radius: 2.0f,
                interval: 0.25f,
                fadeDuration: 1f
            );
        };
        if (fence != null)
            StartCoroutine(CloseFence());
        cam.StartPatrol();
    }
    IEnumerator CloseFence()
    {
        yield return new WaitForSeconds(fenceDelay);
        fence.SetActive(true);
    }
}
