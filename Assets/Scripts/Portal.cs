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
                // Stop player coroutine and animation
                player.StopImmediately();

                // ★ Stop player input
                player.CanMove = false;

                // stop the entire update process
                player.enabled = false;
            }

            FadeManager.Instance.StartSceneTransition(targetSceneName, spawnPointName, fadeDuration);
        }
    }

}
