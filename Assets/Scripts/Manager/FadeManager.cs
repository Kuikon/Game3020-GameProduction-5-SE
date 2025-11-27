using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    private Image fadeImage;
    private string nextSpawnPointName = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            fadeImage = GetComponentInChildren<Image>();
            var c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    public void StartSceneTransition(string targetSceneName, string spawnPointName, float fadeDuration)
    {
        nextSpawnPointName = spawnPointName;
        StartCoroutine(TransitionRoutine(targetSceneName, fadeDuration));
    }

    private IEnumerator TransitionRoutine(string targetScene, float fadeDuration)
    {
        yield return FadeOut(fadeDuration);
        SceneManager.LoadScene(targetScene);
        yield return null;
        Transform spawn = null;
        while (spawn == null)
        {
            var obj = GameObject.Find(nextSpawnPointName);
            if (obj != null)
                spawn = obj.transform;

            yield return null;
        }
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        PlayerController player = null;
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();
        if (player != null)
        {
            player.enabled = false;
            player.transform.position = spawn.position;
        }
        yield return FadeIn(fadeDuration);
        if (player != null)
            player.enabled = true;
            player.CanMove = true;
    }
    public IEnumerator FadeOut(float duration)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 1f;
        fadeImage.color = c;
    }
    public IEnumerator FadeIn(float duration)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / duration);
            fadeImage.color = c;
            yield return null;
        }

        c.a = 0f;
        fadeImage.color = c;
    }
}
