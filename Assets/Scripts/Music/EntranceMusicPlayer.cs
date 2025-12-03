using UnityEngine;

public class EntranceMusicPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SoundManager.Instance.PlayBGM(BGMSoundData.BGM.Entrance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
