using UnityEngine;

public class Portal : MonoBehaviour
{
    public string targetSceneName;
    public string spawnPointName;
    public float fadeDuration = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ★ まず即停止
                player.StopImmediately();

                // ★ 入力も止めたいなら
                player.CanMove = false;

                // Updateの処理ごと止めたいなら
                player.enabled = false;
            }

            FadeManager.Instance.StartSceneTransition(targetSceneName, spawnPointName, fadeDuration);
        }
    }

}
