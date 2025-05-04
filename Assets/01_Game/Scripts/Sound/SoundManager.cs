using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } // �V���O���g���C���X�^���X

    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip audioClip;
    }

    [SerializeField]
    private SoundData[] soundDatas;

    private AudioSource[] audioSourceList = new AudioSource[20];
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public string bgmName;
    }

    [SerializeField]
    private SceneBGM[] inScenePlay;
    private Dictionary<string, string> sceneBGMMapping = new Dictionary<string, string>();

    private AudioSource currentBGMSource;

    private void Awake()
    {
        // �V���O���g���̐ݒ�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // �V�[�����܂����ł��j������Ȃ��悤�ɂ���
        }
        else
        {
            Destroy(gameObject); // ���łɃC���X�^���X�����݂���ꍇ�͐V�����C���X�^���X��j��
            return;
        }

        // AudioSource�̏�����
        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            audioSourceList[i] = gameObject.AddComponent<AudioSource>();
        }

        // soundDictionary�ɃZ�b�g
        foreach (var soundData in soundDatas)
        {
            soundDictionary.Add(soundData.name, soundData);
        }

        // sceneBGMMapping�ɃZ�b�g
        foreach (var sceneBGM in inScenePlay)
        {
            sceneBGMMapping[sceneBGM.sceneName] = sceneBGM.bgmName;
        }

        // �V�[�����[�h����BGM�ݒ�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    //public void Play(AudioClip clip)
    //{
    //    var audioSource = GetUnusedAudioSource();
    //    if (audioSource == null) return;
    //    audioSource.clip = clip;
    //    audioSource.Play();
    //}

    //public void Play(string name)
    //{
    //    if (soundDictionary.TryGetValue(name, out var soundData))
    //    {
    //        Play(soundData.audioClip);
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"���̕ʖ��͓o�^����Ă��܂���: {name}");
    //    }
    //}

    public void Play(AudioClip clip, string mixerGroupName = null)
    {
        var audioSource = GetUnusedAudioSource();
        if (audioSource == null) return;

        if (!string.IsNullOrEmpty(mixerGroupName))
        {
            AudioMixerGroup mixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group => group.name == mixerGroupName);
            if (mixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void Play(string name, string mixerGroupName = null)
    {
        if (soundDictionary.TryGetValue(name, out var soundData))
        {
            Play(soundData.audioClip, mixerGroupName);
        }
        else
        {
            Debug.LogWarning($"���̕ʖ��͓o�^����Ă��܂���: {name}");
        }
    }

    public void PlayBGMForScene(string sceneName)
    {
        if (sceneBGMMapping.TryGetValue(sceneName, out var bgmName))
        {
            PlayBGM(bgmName, "BGM");
        }
        else
        {
            Debug.LogWarning($"�V�[�����ɑΉ�����BGM��������܂���: {sceneName}");
        }
    }

    //public void PlayBGM(string bgmName)
    //{
    //    if (currentBGMSource != null && currentBGMSource.isPlaying)
    //    {
    //        currentBGMSource.Stop();
    //    }

    //    if (soundDictionary.TryGetValue(bgmName, out var soundData))
    //    {
    //        currentBGMSource = GetUnusedAudioSource();
    //        if (currentBGMSource != null)
    //        {
    //            currentBGMSource.clip = soundData.audioClip;
    //            currentBGMSource.loop = true;
    //            currentBGMSource.Play();
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"BGM��������܂���: {bgmName}");
    //    }
    //}

    public void PlayBGM(string bgmName, string mixerGroupName = null)
    {
        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            currentBGMSource.Stop();
        }

        if (soundDictionary.TryGetValue(bgmName, out var soundData))
        {
            currentBGMSource = GetUnusedAudioSource();
            if (currentBGMSource != null)
            {
                if (!string.IsNullOrEmpty(mixerGroupName))
                {
                    AudioMixerGroup mixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group => group.name == mixerGroupName);
                    if (mixerGroup != null)
                    {
                        currentBGMSource.outputAudioMixerGroup = mixerGroup;
                    }
                    else
                    {
                        Debug.LogWarning($"�~�L�T�[�O���[�v'{mixerGroupName}'��������܂���B");
                    }
                }
                currentBGMSource.clip = soundData.audioClip;
                currentBGMSource.loop = true;
                currentBGMSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"BGM��������܂���: {bgmName}");
        }
    }

    private AudioSource GetUnusedAudioSource()
    {
        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            if (!audioSourceList[i].isPlaying) return audioSourceList[i];
        }
        return null;
    }
}
