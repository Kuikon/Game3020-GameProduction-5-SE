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
            FadeManager.Instance.StartSceneTransition(targetSceneName, spawnPointName, fadeDuration);
        }
    }

}
