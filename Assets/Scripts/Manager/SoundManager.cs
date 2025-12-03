using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] AudioSource seAudioSource;

    [SerializeField] List<BGMSoundData> bgmSoundDatas;
    [SerializeField] List<SESoundData> seSoundDatas;

    public float masterVolume = 1;
    public float bgmMasterVolume = 1;
    public float seMasterVolume = 1;

    public static SoundManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(BGMSoundData.BGM bgm)
    {
        BGMSoundData data = bgmSoundDatas.Find(data => data.bgm == bgm);
        bgmAudioSource.clip = data.audioClip;
        bgmAudioSource.volume = data.volume * bgmMasterVolume * masterVolume;
        bgmAudioSource.Play();
    }


    public void PlaySE(SESoundData.SE se)
    {
        SESoundData data = seSoundDatas.Find(data => data.se == se);
        seAudioSource.volume = data.volume * seMasterVolume * masterVolume;
        seAudioSource.PlayOneShot(data.audioClip);
    }
    public void StopBGM()
    {
        if (bgmAudioSource.isPlaying)
            bgmAudioSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmAudioSource.isPlaying)
            bgmAudioSource.Pause();
    }

    public void ResumeBGM()
    {
        if (!bgmAudioSource.isPlaying)
            bgmAudioSource.UnPause();
    }
}

[System.Serializable]
public class BGMSoundData
{
    public enum BGM
    {
        Title,
        Entrance,
        BeforeMain,
        Main,
        Win,
        Lose,
    }

    public BGM bgm;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}
//SoundManager.Instance.PlayBGM(BGMSoundData.BGM.Title);
[System.Serializable]
public class SESoundData
{
    public enum SE
    {
        Select,
        RewardSelect,
        TakeDamage,
        GhostSpawn,
        EXPGet,
        LevelUp,
        GhostCapture,
        Reaction,
        Audience,
        GraveBroken,
        DragonHatch,
        DragonStart,
        HatchExplosion,
        SuicideFire
    }

    public SE se;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}
//SoundManager.Instance.PlaySE(SESoundData.SE.Select);