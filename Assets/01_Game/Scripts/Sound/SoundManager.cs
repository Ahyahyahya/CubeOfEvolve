// �쐬���F   250509
// �X�V���F   250512
// �쐬�ҁF ���� ���l

// �T�v����(AI�ɂ��쐬)�F

// �g���������F

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using R3;
using R3.Triggers;
using System;

// �Q�[���S�̂̃T�E���h�Ǘ����s���V���O���g���N���X�B
public class SoundManager : MonoBehaviour
{
    // SoundManager �̃V���O���g���C���X�^���X�B
    public static SoundManager Instance { get; private set; }

    // ---------------------------- Serializable

    // �V���A���C�Y�\�ȃT�E���h�f�[�^�̃��b�p�[�N���X�B
    // �C���X�y�N�^�[��Ŗ��O�� SoundData �A�Z�b�g��R�t���邽�߂Ɏg�p�B
    [System.Serializable]
    public class SoundDataWrapper
    {
        public string name;
        public SoundData soundData;
    }

    // �C���X�y�N�^�[����ݒ肳���T�E���h�f�[�^�̔z��B
    [SerializeField]
    private SoundDataWrapper[] soundDatas;

    // �V���A���C�Y�\�ȃV�[�����Ƃ� BGM �ݒ�N���X�B
    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public string bgmName;
    }
    // �C���X�y�N�^�[����ݒ肳���V�[�����Ƃ� BGM �ݒ�̔z��B
    [SerializeField]
    private SceneBGM[] inScenePlay;

    // ---------------------------- Field

    // �Đ��Ɏg�p���� AudioSource �̃��X�g�B
    private AudioSource[] audioSourceList = new AudioSource[20];
    // �T�E���h���� SoundData �̑Ή���ێ����鎫���B
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();
    // �V�[������ BGM ���̑Ή���ێ����鎫���B
    private Dictionary<string, string> sceneBGMMapping = new Dictionary<string, string>();
    // ���ݍĐ����� BGM �� AudioSource�B
    private AudioSource currentBGMSource;
    // ���ݍĐ����� BGM �� SoundData�B
    private SoundData currentSoundData;

    // �t�F�[�h�������̍w�ǂ��Ǘ����邽�߂�Disposable�B
    private IDisposable _fadeDisposable;

    // ---------------------------- UnityMessage

    // Awake ���\�b�h�F�C���X�^���X�̏������A�V���O���g���̐ݒ�AAudioSource �̍쐬�A�T�E���h�f�[�^�̓o�^�Ȃǂ��s���B
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
        foreach (var soundDataWrapper in soundDatas)
        {
            if (soundDataWrapper.soundData != null)
            {
                soundDictionary.Add(soundDataWrapper.name, soundDataWrapper.soundData);
            }
            else
            {
                Debug.LogError($"SoundData '{soundDataWrapper.name}' ���ݒ肳��Ă��܂���B");
            }
        }

        // sceneBGMMapping�ɃZ�b�g
        foreach (var sceneBGM in inScenePlay)
        {
            sceneBGMMapping[sceneBGM.sceneName] = sceneBGM.bgmName;
        }

        // �V�[�����[�h����BGM�ݒ�
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        this.UpdateAsObservable()
            .Where(x => (currentBGMSource != null && currentBGMSource.isPlaying && currentSoundData is LoopSoundData))
            .Subscribe(_ =>
            {
                LoopCheck(currentBGMSource, currentSoundData);
            }
            );
    }

    // OnDestroy ���\�b�h�F�C���X�^���X���j�������ۂ̏����B�V�[�����[�h�C�x���g�̉������s���B
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // ---------------------------- Public

    // Play ���\�b�h�F�w�肳�ꂽ���O�̃T�E���h���Đ�����B�~�L�T�[�O���[�v���w�肷�邱�Ƃ��\�B
    public void Play(string name, string mixerGroupName = null)
    {
        if (soundDictionary.TryGetValue(name, out var soundData))
        {
            AudioClip clipToPlay = soundData.GetAudioClip();
            if (clipToPlay != null)
            {
                AudioSource audioSource = GetUnusedAudioSource();
                if (audioSource != null)
                {
                    audioSource.clip = clipToPlay;
                    if (!string.IsNullOrEmpty(mixerGroupName))
                    {
                        AudioMixerGroup mixerGroup = Resources.FindObjectsOfTypeAll<AudioMixerGroup>().FirstOrDefault(group => group.name == mixerGroupName);
                        if (mixerGroup != null)
                        {
                            audioSource.outputAudioMixerGroup = mixerGroup;
                        }
                        else
                        {
                            Debug.LogWarning($"�~�L�T�[�O���[�v'{mixerGroupName}'��������܂���B");
                        }
                    }
                    audioSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"�I�[�f�B�I�N���b�v��������܂���: {name}");
            }
        }
        else
        {
            Debug.LogWarning($"���̕ʖ��͓o�^����Ă��܂���: {name}");
        }
    }

    /// <summary>
    /// ���ݍĐ�����BGM���t�F�[�h�A�E�g�����Ē�~���܂��B
    /// </summary>
    /// <param name="fadeDuration">�t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j</param>
    public void StopBGMWithFade(float fadeDuration)
    {
        if (currentBGMSource == null || !currentBGMSource.isPlaying)
        {
            Debug.Log("�Đ�����BGM������܂���B");
            return;
        }

        // �����̃t�F�[�h�����𒆎~
        _fadeDisposable?.Dispose();

        float startVolume = currentBGMSource.volume;
        _fadeDisposable = Observable.Interval(TimeSpan.FromSeconds(Time.deltaTime))
            .TakeWhile(_ => currentBGMSource != null && currentBGMSource.volume > 0)
            .Subscribe(
                _ =>
                {
                    if (currentBGMSource != null)
                    {
                        currentBGMSource.volume -= startVolume * (Time.deltaTime / fadeDuration);
                        if (currentBGMSource.volume <= 0)
                        {
                            currentBGMSource.Stop();
                            currentBGMSource.volume = startVolume; // ���̃{�����[���ɖ߂��Ă���
                            _fadeDisposable?.Dispose(); // �w�ǂ�����
                        }
                    }
                    else
                    {
                        _fadeDisposable?.Dispose(); // AudioSource��null�ɂȂ����ꍇ������
                    }
                }
                // R3�ł�onError��onCompleted��Subscribe�̈����Ƃ��Ă͎w�肵�܂���B
                // �G���[��Catch�ȂǂŁA������Finally�Ȃǂň����܂��B
                // ����̂悤�ȃ��W�b�N�ł͒ʏ�G���[�͔������ɂ����ł��B
            );
    }

    // PlayBGMForScene ���\�b�h�F�w�肳�ꂽ�V�[�����ɑΉ����� BGM ���Đ�����B
    public void PlayBGMForScene(string sceneName)
    {
        if (sceneBGMMapping.TryGetValue(sceneName, out var bgmName))
        {
            Debug.Log($"sceneName/{sceneName}\n bgmName/{bgmName}");
            PlayBGM(bgmName, "BGM");
        }
        else
        {
            Debug.LogWarning($"�V�[�����ɑΉ�����BGM��������܂���: {sceneName}");
        }
    }

    // PlayBGM ���\�b�h�F�w�肳�ꂽ BGM ���� BGM ���Đ�����B�~�L�T�[�O���[�v���w�肷�邱�Ƃ��\�B
    public void PlayBGM(string bgmName, string mixerGroupName = null)
    {
        // ���ݍĐ����� BGM ������Β�~����B
        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            currentBGMSource.Stop();
        }

        if (soundDictionary.TryGetValue(bgmName, out var soundData))
        {
            Debug.Log($"bgmName/{bgmName}\n soundData/{soundData}");

            currentSoundData = soundData;
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
                currentBGMSource.clip = soundData.GetAudioClip();
                currentBGMSource.loop = true;
                currentBGMSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"BGM��������܂���: {bgmName}");
        }
    }

    // ---------------------------- Private

    // OnSceneLoaded ���\�b�h�F�V�[�������[�h���ꂽ�ۂɌĂяo����A�V�[���ɑΉ����� BGM ���Đ�����B
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    // GetUnusedAudioSource ���\�b�h�F���ݍĐ����łȂ� AudioSource ���擾����B
    private AudioSource GetUnusedAudioSource()
    {
        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            if (!audioSourceList[i].isPlaying) return audioSourceList[i];
        }
        return null; // �S�Ă� AudioSource ���g�p���̏ꍇ�� null ��Ԃ��B
    }

    // LoopCheck ���\�b�h�FAudioSource �̍Đ��ʒu���Ď����ALoopSoundData �Ɋ�Â��ă��[�v�������s���B
    private void LoopCheck(AudioSource audioSource, SoundData soundData)
    {
        if (soundData is LoopSoundData soundLoopData)
        {
            // �T���v�����O���g�����l�������������p�x���v�Z���郍�[�J���֐��B
            int CorrectFrequency(long n)
            {
                return (int)(n * audioSource.clip.frequency / soundLoopData.frequency);
            }
            // �Đ��ʒu�����[�v�I���ʒu�𒴂����ꍇ�A���[�v�J�n�ʒu�ɖ߂��B
            if (audioSource.timeSamples >= CorrectFrequency(soundLoopData.loopEnd))
            {
                audioSource.timeSamples -= CorrectFrequency(soundLoopData.loopEnd - soundLoopData.loopStart);
            }
        }
    }
}