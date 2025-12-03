using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SoundManager.Instance.PlaySE(SESoundData.SE.Select);
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
            if (obj.scene.name == "DontDestroyOnLoad")
            {
                GameObject.Destroy(obj);
            }
        }
    }
}
