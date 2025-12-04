using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MasterVolumeUI : MonoBehaviour
{
    [SerializeField] private GameObject uiRoot;

    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private GameObject backButton;

    [Header("Scene Settings")]
    [SerializeField] private string startSceneName = "StartScene";

    private bool isVisible = false;
    private bool isStartScene = false;
    private void Start()
    {
        uiRoot.SetActive(false);
        volumeSlider.value = SoundManager.Instance.masterVolume;
        volumeSlider.onValueChanged.AddListener(value =>
        {
            SoundManager.Instance.SetMasterVolume(value);
        });

        string scene = SceneManager.GetActiveScene().name;
        isStartScene = (scene == startSceneName);
        backButton.SetActive(!isStartScene);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isVisible = !isVisible;
            uiRoot.SetActive(isVisible);
            if (isStartScene)
                return;
            if (isVisible)
            {
                GameManager.Instance.DisablePlayerControl();
            }
            else
            {
                GameManager.Instance.EnablePlayerControl();
            }
        }
    }

    public void OnClickBackToStart()
    {
        Time.timeScale = 1f;
        SoundManager.Instance.StopBGM();
        ClearDontDestroyOnLoad();
        SceneManager.LoadScene(startSceneName);
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
