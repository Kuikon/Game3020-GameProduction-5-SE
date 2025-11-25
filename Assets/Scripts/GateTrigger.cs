using System.Collections;
using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    public CameraFollow2D cam;
    public GameObject fence;      // ← 閉めるフェンスのスプライト
    public float fenceDelay = 0.1f; // 少し遅らせたい場合用（任意）
    public EnemySpawner spawner;

    private bool triggered = false;
    private void Start()
    {
        if (cam == null)
        {
            CameraFollow2D[] cams = FindObjectsOfType<CameraFollow2D>(true);

            foreach (var c in cams)
            {
                // ★ DontDestroyOnLoad のカメラだけ選ぶ！
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

        // GhostPointsオブジェクトを取得
        GameObject root = GameObject.Find("GhostPoints");
        if (root != null)
        {
            // 子オブジェクトだけ取得
            Transform[] children = new Transform[root.transform.childCount];
            for (int i = 0; i < root.transform.childCount; i++)
            {
                children[i] = root.transform.GetChild(i);
            }

            // カメラにパトロールポイントを渡す
            cam.SetGhostPoints(children);
        }
        else
        {
            Debug.LogError("GhostPoints object not found in this scene!");
        }

        // イベント登録
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

        // フェンス
        if (fence != null)
            StartCoroutine(CloseFence());

        // パトロール開始
        cam.StartPatrol();
    }



    IEnumerator CloseFence()
    {
        yield return new WaitForSeconds(fenceDelay);
        fence.SetActive(true);  // ← 扉を閉める
    }
}
