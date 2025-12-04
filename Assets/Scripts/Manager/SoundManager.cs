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
    private int sePlayCountThisFrame = 0;
    private const int SE_HARD_LIMIT = 5;
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
    void Update()
    {
        sePlayCountThisFrame = 0;
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
        if (data == null) return;
        sePlayCountThisFrame++;
        float overlapFactor = Mathf.Clamp01(1f - (sePlayCountThisFrame - 1) * 0.15f);
        float volume = data.volume * seMasterVolume * masterVolume * overlapFactor;
        seAudioSource.volume = volume;
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
    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        bgmAudioSource.volume = bgmMasterVolume * masterVolume;
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
        SuicideFire,
        CaptureEmpty,
        CaptureFail,
        CaptureProgress

    }

    public SE se;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}
//SoundManager.Instance.PlaySE(SESoundData.SE.Select);