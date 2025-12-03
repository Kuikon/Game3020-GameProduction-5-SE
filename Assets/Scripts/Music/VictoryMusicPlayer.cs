using UnityEngine;

public class VictoryMusicPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM(BGMSoundData.BGM.Win);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
