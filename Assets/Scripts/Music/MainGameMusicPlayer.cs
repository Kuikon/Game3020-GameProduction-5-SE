using UnityEngine;

public class MainGameMusicPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM(BGMSoundData.BGM.BeforeMain);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
