using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }
    public void OnRetry(string sceneName)
    {
        ClearDontDestroyOnLoad();
        SceneManager.LoadScene(sceneName);
    }
    public static void ClearDontDestroyOnLoad()
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // DontDestroyOnLoad の隠しシーンにいるオブジェクトだけ削除
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
