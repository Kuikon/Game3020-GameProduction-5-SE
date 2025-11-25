using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance;

    private Image fadeImage;

    // 🔵 PortalSpawnData を使わず、この変数に保存する
    private string nextSpawnPointName = null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            fadeImage = GetComponentInChildren<Image>();

            // 初期透明
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

    // ------------------------------------------------------
    // 🔵 シーン遷移開始
    // ------------------------------------------------------
    public void StartSceneTransition(string targetSceneName, string spawnPointName, float fadeDuration)
    {
        // ここで保存
        nextSpawnPointName = spawnPointName;

        StartCoroutine(TransitionRoutine(targetSceneName, fadeDuration));
    }

    private IEnumerator TransitionRoutine(string targetScene, float fadeDuration)
    {
        // フェードアウト
        yield return FadeOut(fadeDuration);

        // シーンロード
        SceneManager.LoadScene(targetScene);
        yield return null;

        // 🔵 スポーン位置が見つかるまで待つ
        Transform spawn = null;
        while (spawn == null)
        {
            var obj = GameObject.Find(nextSpawnPointName);
            if (obj != null)
                spawn = obj.transform;

            yield return null;
        }

        // プレイヤー取得
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        PlayerController player = null;
        if (playerObj != null)
            player = playerObj.GetComponent<PlayerController>();

        // プレイヤー移動
        if (player != null)
        {
            player.enabled = false;
            player.transform.position = spawn.position;
        }

        // フェードイン
        yield return FadeIn(fadeDuration);

        // 操作復帰
        if (player != null)
            player.enabled = true;
            player.CanMove = true;
    }

    // ----------------------------------------------
    // フェードアウト（透明→黒）
    // ----------------------------------------------
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

    // ----------------------------------------------
    // フェードイン（黒→透明）
    // ----------------------------------------------
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
