using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using R3;
using R3.Triggers;
using System;

/// <summary>
/// �Q�[���S�̂̃T�E���h�Ǘ����s���V���O���g���N���X�ł��B
/// BGM�ASE�̍Đ��AAudioSource�̃v�[���Ǘ��A�t�F�[�h�����A�V�[�����Ƃ�BGM�؂�ւ��Ȃǂ�S�����܂��B
/// </summary>
public class SoundManager : MonoBehaviour
{
    // ----- Singleton (�V���O���g��)
    /// <summary>
    /// SoundManager �̃V���O���g���C���X�^���X�ł��B
    /// </summary>
    public static SoundManager Instance { get; private set; }

    // ----- Serializable Fields (�V���A���C�Y�t�B�[���h)
    /// <summary>
    /// �V���A���C�Y�\�ȃT�E���h�f�[�^�̃��b�p�[�N���X�ł��B
    /// �C���X�y�N�^�[��Ŗ��O�� SoundData �A�Z�b�g��R�t���邽�߂Ɏg�p���܂��B
    /// </summary>
    [System.Serializable]
    public class SoundDataWrapper
    {
        public string name;
        public SoundData soundData;
    }

    /// <summary>
    /// �C���X�y�N�^�[����ݒ肳���T�E���h�f�[�^�̔z��ł��B
    /// </summary>
    [SerializeField]
    private SoundDataWrapper[] soundDatas;

    /// <summary>
    /// �V���A���C�Y�\�ȃV�[�����Ƃ� BGM �ݒ�N���X�ł��B
    /// </summary>
    [System.Serializable]
    public class SceneBGM
    {
        public string sceneName;
        public string bgmName;
    }
    /// <summary>
    /// �C���X�y�N�^�[����ݒ肳���V�[�����Ƃ� BGM �ݒ�̔z��ł��B
    /// </summary>
    [SerializeField]
    private SceneBGM[] inScenePlay;

    [Header("AudioSource Pooling Settings")]
    [Tooltip("SoundManager�����g�ŕێ�����AudioSource�̏������iUI/System SE�p�j")]
    [SerializeField]
    private int _initialManagerAudioSources = 20;

    [Tooltip("�e�Q�[���I�u�W�F�N�g���ێ��ł���AudioSource�̍ő吔")]
    [SerializeField]
    private int _maxPooledAudioSourcesPerObject = 5;

    // ----- Private Fields (�v���C�x�[�g�t�B�[���h)
    /// <summary>
    /// SoundManager�����g�ōĐ��Ɏg�p���� AudioSource �̃v�[�� (UI/System SE�p)�B
    /// </summary>
    private Stack<AudioSource> _managerAudioSourcePool = new Stack<AudioSource>();
    /// <summary>
    /// �T�E���h���� SoundData �̑Ή���ێ����鎫���ł��B
    /// </summary>
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();
    /// <summary>
    /// �V�[������ BGM ���̑Ή���ێ����鎫���ł��B
    /// </summary>
    private Dictionary<string, string> sceneBGMMapping = new Dictionary<string, string>();
    /// <summary>
    /// ���ݍĐ����� BGM �� AudioSource �ł��B
    /// </summary>
    private AudioSource currentBGMSource;
    /// <summary>
    /// ���ݍĐ����� BGM �� SoundData �ł��B
    /// </summary>
    private SoundData currentSoundData;

    /// <summary>
    /// BGM�̃t�F�[�h�������̍w�ǂ��Ǘ����邽�߂�Disposable�ł��B
    /// </summary>
    private IDisposable _bgmFadeDisposable;

    // --- AudioSource Pooling Fields (AudioSource�v�[���֘A�t�B�[���h)
    /// <summary>
    /// SoundManager���Ǘ�����A�ǂ�GameObject�ɂ��݂��o����Ă��Ȃ�AudioSource�̃O���[�o���v�[���ł��B
    /// </summary>
    private Stack<AudioSource> _globalAudioSourcePool = new Stack<AudioSource>();
    /// <summary>
    /// �eGameObject�ɑ݂��o����Ă���A�܂��͂���GameObject�ɃA�^�b�`����ė��p�\��AudioSource�̃��X�g�ł��B
    /// </summary>
    private Dictionary<GameObject, List<AudioSource>> _objectAudioSourcePools = new Dictionary<GameObject, List<AudioSource>>();
    /// <summary>
    /// AudioSource�Ƃ��̃I�[�i�[GameObject��R�t����}�b�v�ł��B
    /// </summary>
    private Dictionary<AudioSource, GameObject> _audioSourceOwnerMap = new Dictionary<AudioSource, GameObject>();

    /// <summary>
    /// AudioMixerGroup�̃L���b�V���ł��B���O�őf�����Q�Ƃł��܂��B
    /// </summary>
    private Dictionary<string, AudioMixerGroup> _mixerGroupCache = new Dictionary<string, AudioMixerGroup>();

    // ----- Unity Messages (Unity�C�x���g���b�Z�[�W)
    /// <summary>
    /// �X�N���v�g�C���X�^���X�����[�h���ꂽ�Ƃ��ɌĂяo����܂��B
    /// �V���O���g���̏������AAudioSource�v�[���̐ݒ�A�T�E���h�f�[�^�ƃV�[��BGM�̃}�b�s���O�A�~�L�T�[�O���[�v�̃L���b�V�����������s���܂��B
    /// </summary>
    private void Awake()
    {
        // �V���O���g���̐ݒ�
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject); // �V�[�����܂����ł��j������Ȃ��悤�ɂ��܂��B
        }
        else
        {
            Destroy(gameObject); // ���łɃC���X�^���X�����݂���ꍇ�́A�V�����C���X�^���X��j�����܂��B
            return;
        }

        // SoundManager���g���g��AudioSource�̏����� (UI/System SE�p)
        for (var i = 0; i < _initialManagerAudioSources; ++i)
        {
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false; // �����Đ����Ȃ��悤�ɐݒ肵�܂��B
            newSource.enabled = false; // ������Ԃł͖����ɂ��Ă����܂��B
            _managerAudioSourcePool.Push(newSource); // �v�[���ɒǉ����܂��B
        }

        // soundDictionary�ɃT�E���h�f�[�^���Z�b�g
        foreach (var soundDataWrapper in soundDatas)
        {
            if (soundDataWrapper.soundData != null)
            {
                // �d���o�^������܂��B
                if (!soundDictionary.ContainsKey(soundDataWrapper.name))
                {
                    soundDictionary.Add(soundDataWrapper.name, soundDataWrapper.soundData);
                }
                else
                {
                    Debug.LogWarning($"[SoundManager] �T�E���h�� '{soundDataWrapper.name}' �͊��ɓo�^����Ă��܂��B�㏑���͍s���܂���B");
                }
            }
            else
            {
                Debug.LogError($"[SoundManager] SoundData '{soundDataWrapper.name}' ���ݒ肳��Ă��܂���B");
            }
        }

        // sceneBGMMapping�ɃV�[��BGM�f�[�^���Z�b�g
        foreach (var sceneBGM in inScenePlay)
        {
            sceneBGMMapping[sceneBGM.sceneName] = sceneBGM.bgmName;
        }

        // �I�[�f�B�I�~�L�T�[�O���[�v�̃L���b�V��������
        InitMixerGroupCache();

        // �V�[�����[�h����BGM�ݒ�C�x���g�ɓo�^
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// ����Update�̑O�ɌĂяo����܂��B
    /// BGM���[�v�`�F�b�N�̍w�ǂ�ݒ肵�܂��B
    /// </summary>
    private void Start()
    {
        // BGM���[�v�`�F�b�N�̍w��
        this.UpdateAsObservable()
            .Where(x => (currentBGMSource != null && currentBGMSource.isPlaying && currentSoundData is LoopSoundData))
            .Subscribe(_ =>
            {
                LoopCheck(currentBGMSource, currentSoundData);
            })
            .AddTo(this); // SoundManager���j�����ꂽ�玩���I�ɍw�ǉ�������܂��B
    }

    /// <summary>
    /// ���̃X�N���v�g���A�^�b�`���ꂽ�Q�[���I�u�W�F�N�g���j�������Ƃ��ɌĂяo����܂��B
    /// �V�[�����[�h�C�x���g�̉����A�t�F�[�h�������̍w�ǉ����A����ёS�Ă�AudioSource�v�[���̃N���[���A�b�v���s���܂��B
    /// </summary>
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded; // �C�x���g�̉���
            _bgmFadeDisposable?.Dispose(); // �t�F�[�h�������̍w�ǂ�����

            // �O���[�o���v�[������AudioSource��S�Ĕj��
            foreach (var audioSource in _globalAudioSourcePool)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
            _globalAudioSourcePool.Clear();

            // SoundManager���g��AudioSource�v�[������AudioSource��S�Ĕj��
            foreach (var audioSource in _managerAudioSourcePool)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
            _managerAudioSourcePool.Clear();

            // �e�I�u�W�F�N�g�v�[������AudioSource��S�Ĕj��
            foreach (var pair in _objectAudioSourcePools)
            {
                foreach (var audioSource in pair.Value)
                {
                    if (audioSource != null) Destroy(audioSource.gameObject);
                }
            }
            _objectAudioSourcePools.Clear();
            _audioSourceOwnerMap.Clear();
        }
    }

    // ----- Public Methods (���J���\�b�h)
    /// <summary>
    /// ����̋�Ԉʒu�ɕR�Â��Ȃ��T�E���h�iUI���A�V�X�e��SE�Ȃǁj���Đ����܂��B
    /// SoundManager���g��AudioSource�v�[�����痘�p�\�Ȃ��̂��擾���čĐ����܂��B
    /// </summary>
    /// <param name="name">�Đ�����T�E���h�̕ʖ�</param>
    /// <param name="mixerGroupName">���蓖�Ă�AudioMixerGroup�̖��O�i�I�v�V�����j</param>
    public void Play(string name, string mixerGroupName = null)
    {
        if (!soundDictionary.TryGetValue(name, out var soundData))
        {
            Debug.LogWarning($"[SoundManager] �T�E���h'{name}'�͓o�^����Ă��܂���B");
            return;
        }

        AudioClip clipToPlay = soundData.GetAudioClip();
        if (clipToPlay == null)
        {
            Debug.LogWarning($"[SoundManager] �I�[�f�B�I�N���b�v��������܂���: {name}");
            return;
        }

        AudioSource audioSource = GetUnusedManagerAudioSource();
        if (audioSource == null)
        {
            Debug.LogWarning("[SoundManager] SoundManager�����p�\��AudioSource�������Ă��܂���BSE���Đ��ł��܂���ł����B");
            return;
        }

        ResetAudioSourceProperties(audioSource); // �v���p�e�B��������ԂɃ��Z�b�g���܂��B
        audioSource.clip = clipToPlay;
        audioSource.loop = false; // �ʏ�ASE�̓��[�v���܂���B
        audioSource.spatialBlend = 0f; // 2D�T�E���h�Ƃ��čĐ����܂��B

        SetAudioMixerGroup(audioSource, mixerGroupName);
        audioSource.Play();

        // Play���\�b�h�ōĐ����ꂽ�P��SE�������ŉ������悤�ɁA�N���b�v�̍Đ����Ԍ�Ƀv�[���ɖ߂��܂��B
        Observable.Timer(TimeSpan.FromSeconds(clipToPlay.length))
            .Subscribe(_ =>
            {
                ReleaseManagerAudioSource(audioSource);
            })
            .AddTo(this); // SoundManager���j�����ꂽ�玩���w�ǉ�������܂��B
    }

    /// <summary>
    /// �w�肳�ꂽGameObject�̈ʒu�����ԓI�ȃT�E���h�iSFX�j���Đ����܂��B
    /// �I�u�W�F�N�g�v�[������AudioSource���擾���A�Đ��I����Ƀv�[���ɖ߂��܂��B
    /// </summary>
    /// <param name="name">�Đ�����T�E���h�̕ʖ�</param>
    /// <param name="sourceObject">�T�E���h�̔������ƂȂ�GameObject</param>
    /// <param name="mixerGroupName">���蓖�Ă�AudioMixerGroup�̖��O�i�I�v�V�����j</param>
    public void PlaySFXAt(string name, GameObject sourceObject, string mixerGroupName = null)
    {
        if (sourceObject == null)
        {
            Debug.LogWarning($"[SoundManager] �T�E���h'{name}'�̔������ƂȂ�GameObject��null�ł��B�Ăяo����: {GetCallingMethodName()}");
            return;
        }
        if (!soundDictionary.TryGetValue(name, out var soundData))
        {
            Debug.LogWarning($"[SoundManager] �T�E���h'{name}'�͓o�^����Ă��܂���B�Ăяo�����I�u�W�F�N�g: {sourceObject.name}");
            return;
        }

        AudioClip clipToPlay = soundData.GetAudioClip();
        if (clipToPlay == null)
        {
            Debug.LogWarning($"[SoundManager] �I�[�f�B�I�N���b�v��������܂���: {name}");
            return;
        }

        AudioSource audioSource = GetAudioSource(sourceObject);
        if (audioSource == null)
        {
            Debug.LogWarning($"[SoundManager] �I�u�W�F�N�g'{sourceObject.name}'�ɗ��p�\��AudioSource������܂���B�T�E���h'{name}'���Đ��ł��܂���ł����B");
            return;
        }

        ResetAudioSourceProperties(audioSource);
        audioSource.clip = clipToPlay;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f; // 3D�T�E���h�Ƃ��čĐ����܂��B

        SetAudioMixerGroup(audioSource, mixerGroupName);
        audioSource.Play();

        // SFX�̍Đ��I�������m���AAudioSource���v�[���ɖ߂�
        audioSource.gameObject.UpdateAsObservable() // AudioSource���A�^�b�`����Ă���GameObject��Update���w�ǂ��܂��B
            .Where(_ => !audioSource.isPlaying) // �Đ����łȂ����Ƃ����m���܂��B
            .Take(1) // ��x�������s���܂��B
            .Subscribe(_ =>
            {
                ReleaseAudioSource(audioSource);
            })
            .AddTo(audioSource.gameObject); // audioSource���A�^�b�`����Ă���GameObject���j�����ꂽ�玩���w�ǉ�������܂��B
    }

    /// <summary>
    /// ���ݍĐ�����BGM���t�F�[�h�A�E�g�����Ē�~���܂��B
    /// </summary>
    /// <param name="fadeDuration">�t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j</param>
    public void StopBGMWithFade(float fadeDuration)
    {
        if (currentBGMSource == null || !currentBGMSource.isPlaying)
        {
            Debug.Log("[SoundManager] �Đ�����BGM������܂���B��~�����͕s�v�ł��B");
            return;
        }

        _bgmFadeDisposable?.Dispose(); // �����̃t�F�[�h�����𒆎~���܂��B

        float startVolume = currentBGMSource.volume;
        _bgmFadeDisposable = Observable.Interval(TimeSpan.FromSeconds(Time.deltaTime))
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
                            currentBGMSource.volume = startVolume; // ���̃{�����[���ɖ߂��Ă����܂��B
                            _bgmFadeDisposable?.Dispose(); // �w�ǂ��������܂��B
                            Debug.Log("[SoundManager] BGM�̃t�F�[�h�A�E�g���������A��~���܂����B");
                        }
                    }
                    else
                    {
                        _bgmFadeDisposable?.Dispose(); // AudioSource��null�ɂȂ����ꍇ���������܂��B
                        Debug.LogWarning("[SoundManager] BGM��AudioSource���t�F�[�h���ɔj������܂����B");
                    }
                }
            )
            .AddTo(this); // SoundManager���j�����ꂽ�玩���I�ɍw�ǉ�������܂��B
    }

    /// <summary>
    /// �w�肳�ꂽBGM�����t�F�[�h�A�E�g�����Ē�~���܂��B
    /// ���ݍĐ�����BGM���w�肳�ꂽBGM���ƈ�v����ꍇ�̂ݏ������܂��B
    /// </summary>
    /// <param name="bgmName">��~����BGM�̕ʖ�</param>
    /// <param name="fadeDuration">�t�F�[�h�A�E�g�ɂ����鎞�ԁi�b�j</param>
    public void StopBGMByNameWithFade(string bgmName, float fadeDuration)
    {
        if (currentBGMSource == null || !currentBGMSource.isPlaying)
        {
            Debug.Log("[SoundManager] �Đ�����BGM������܂���B��~�����͕s�v�ł��B");
            return;
        }

        string playingBGMName = soundDictionary.FirstOrDefault(x => x.Value == currentSoundData).Key;
        if (playingBGMName != bgmName)
        {
            Debug.LogWarning($"[SoundManager] ���ݍĐ�����BGM ('{playingBGMName}') �́A�w�肳�ꂽBGM�� ('{bgmName}') �ƈقȂ�܂��B��~���܂���B");
            return;
        }

        StopBGMWithFade(fadeDuration);
    }

    /// <summary>
    /// �V�[�������[�h���ꂽ�ۂɌĂяo����A�V�[���ɑΉ�����BGM���Đ����܂��B
    /// </summary>
    /// <param name="scene">���[�h���ꂽ�V�[���̏��</param>
    /// <param name="mode">�V�[���̃��[�h���[�h</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    /// <summary>
    /// �w�肳�ꂽ�V�[�����ɑΉ����� BGM ���Đ����܂��B
    /// </summary>
    /// <param name="sceneName">BGM���Đ�����V�[���̖��O</param>
    /// <param name="fadeInDuration">BGM�̃t�F�[�h�C���ɂ����鎞�ԁi�b�j�B0�̏ꍇ�͑����Đ��B</param>
    public void PlayBGMForScene(string sceneName, float fadeInDuration = 0f)
    {
        if (sceneBGMMapping.TryGetValue(sceneName, out var bgmName))
        {
            Debug.Log($"[SoundManager] �V�[��'{sceneName}'�ɑΉ�����BGM '{bgmName}' ���Đ����܂��B");
            PlayBGM(bgmName, "BGM", fadeInDuration);
        }
        else
        {
            Debug.LogWarning($"[SoundManager] �V�[����'{sceneName}'�ɑΉ�����BGM��������܂���B");
        }
    }

    /// <summary>
    /// �w�肳�ꂽBGM����BGM���Đ����܂��B�~�L�T�[�O���[�v���w�肷�邱�Ƃ��\�B
    /// ����BGM���Đ�����Ă���ꍇ�́A����BGM���~���Ă���V����BGM���Đ����܂��B
    /// </summary>
    /// <param name="bgmName">�Đ�����BGM�̕ʖ�</param>
    /// <param name="mixerGroupName">���蓖�Ă�AudioMixerGroup�̖��O�i�I�v�V�����j</param>
    /// <param name="fadeInDuration">BGM�̃t�F�[�h�C���ɂ����鎞�ԁi�b�j�B0�̏ꍇ�͑����Đ��B</param>
    public void PlayBGM(string bgmName, string mixerGroupName = null, float fadeInDuration = 0f)
    {
        _bgmFadeDisposable?.Dispose(); // �����̃t�F�[�h�����𒆎~���܂��B

        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            currentBGMSource.Stop();
        }

        if (!soundDictionary.TryGetValue(bgmName, out var soundData))
        {
            Debug.LogWarning($"[SoundManager] BGM'{bgmName}'��������܂���B�Đ��ł��܂���B");
            return;
        }

        currentSoundData = soundData;
        currentBGMSource = GetUnusedManagerAudioSource();
        if (currentBGMSource == null)
        {
            Debug.LogWarning("[SoundManager] SoundManager��BGM�Đ��ɗ��p�\��AudioSource�������Ă��܂���BBGM���Đ��ł��܂���ł����B");
            return;
        }

        ResetAudioSourceProperties(currentBGMSource); // �v���p�e�B��������ԂɃ��Z�b�g���܂��B
        currentBGMSource.clip = soundData.GetAudioClip();
        currentBGMSource.loop = true;
        currentBGMSource.spatialBlend = 0f; // BGM�͒ʏ�2D�T�E���h�ł��B

        SetAudioMixerGroup(currentBGMSource, mixerGroupName);

        if (fadeInDuration > 0f)
        {
            currentBGMSource.volume = 0f; // �t�F�[�h�C���J�n���̓{�����[����0�ɂ��܂��B
            currentBGMSource.Play();

            _bgmFadeDisposable = Observable.Interval(TimeSpan.FromSeconds(Time.deltaTime))
                .TakeWhile(_ => currentBGMSource != null && currentBGMSource.volume < 1f)
                .Subscribe(
                    _ =>
                    {
                        if (currentBGMSource != null)
                        {
                            currentBGMSource.volume += (1f / fadeInDuration) * Time.deltaTime;
                            if (currentBGMSource.volume >= 1f)
                            {
                                currentBGMSource.volume = 1f; // �ő�{�����[���ɌŒ肵�܂��B
                                _bgmFadeDisposable?.Dispose();
                                Debug.Log("[SoundManager] BGM�̃t�F�[�h�C�����������܂����B");
                            }
                        }
                        else
                        {
                            _bgmFadeDisposable?.Dispose();
                            Debug.LogWarning("[SoundManager] BGM��AudioSource���t�F�[�h�C�����ɔj������܂����B");
                        }
                    }
                )
                .AddTo(this); // SoundManager���j�����ꂽ�玩���w�ǉ�������܂��B
        }
        else
        {
            currentBGMSource.volume = 1f; // �t�F�[�h�C�����Ȃ��ꍇ�͑����ɍő�{�����[����ݒ肵�܂��B
            currentBGMSource.Play();
        }
    }

    // ----- Private Methods (�v���C�x�[�g���\�b�h)
    /// <summary>
    /// ���݃V�[���ɂ���S�Ă�AudioMixerGroup���L���b�V���ɓo�^���܂��B
    /// </summary>
    private void InitMixerGroupCache()
    {
        foreach (var group in Resources.FindObjectsOfTypeAll<AudioMixerGroup>())
        {
            if (!_mixerGroupCache.ContainsKey(group.name))
            {
                _mixerGroupCache.Add(group.name, group);
            }
            else
            {
                Debug.LogWarning($"[SoundManager] �I�[�f�B�I�~�L�T�[�O���[�v'{group.name}'���������݂��܂��B�ŏ��̂��̂��g�p���܂��B");
            }
        }
    }

    /// <summary>
    /// SoundManager���g������AudioSource�v�[������A���ݍĐ����łȂ�AudioSource���擾���܂��B
    /// ���UI��V�X�e��SE�ABGM�̍Đ��Ɏg�p���܂��B
    /// </summary>
    /// <returns>���p�\��AudioSource�A�܂��̓v�[�����͊������ꍇ��null�B</returns>
    private AudioSource GetUnusedManagerAudioSource()
    {
        if (_managerAudioSourcePool.Count > 0)
        {
            AudioSource source = _managerAudioSourcePool.Pop();
            if (source != null)
            {
                ResetAudioSourceProperties(source); // �v���p�e�B��������ԂɃ��Z�b�g���܂��B
                source.enabled = true; // �L���ɂ��܂��B
                source.gameObject.SetActive(true); // GameObject���A�N�e�B�u�ɂ��܂� (�O�̂���)�B
                return source;
            }
        }
        Debug.LogWarning("[SoundManager] SoundManager��AudioSource�v�[�����͊����Ă��܂��B�V����AudioSource�͐�������܂���B");
        return null;
    }

    /// <summary>
    /// SoundManager���g��AudioSource�v�[���ɖ߂��܂��B
    /// ���UI��V�X�e��SE�ABGM�̍Đ��Ɏg�p���ꂽAudioSource���ė��p�̂��߂Ƀv�[�����܂��B
    /// </summary>
    /// <param name="audioSource">�v�[���ɖ߂�AudioSource�B</param>
    private void ReleaseManagerAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;
        audioSource.Stop();
        audioSource.clip = null; // �N���b�v���N���A���܂��B
        audioSource.enabled = false; // AudioSource�R���|�[�l���g�𖳌��ɂ��܂��B
        audioSource.gameObject.SetActive(false); // GameObject���A�N�e�B�u�ɂ��܂��B
        _managerAudioSourcePool.Push(audioSource); // �v�[���ɖ߂��܂��B
    }

    /// <summary>
    /// �T�E���h�̔������ƂȂ�GameObject�ɃA�^�b�`���ꂽAudioSource���擾���܂��B
    /// �v�[������ė��p�A�܂��͕K�v�ɉ����ē��I�ɐ������܂��B
    /// </summary>
    /// <param name="owner">AudioSource���K�v��GameObject�B</param>
    /// <returns>���p�\��AudioSource�A�܂��̓v�[�����͊������ꍇ��null�B</returns>
    private AudioSource GetAudioSource(GameObject owner)
    {
        // �I�[�i�[�I�u�W�F�N�g��null�܂��͔�A�N�e�B�u�̏ꍇ�A�V����AudioSource�͊��蓖�Ă܂���B
        if (owner == null || !owner.activeInHierarchy)
        {
            Debug.LogWarning($"[SoundManager] �I�[�i�[�I�u�W�F�N�g���L���ł͂���܂���BAudioSource���擾�ł��܂���ł����B");
            return null;
        }

        List<AudioSource> ownerSources;
        if (!_objectAudioSourcePools.TryGetValue(owner, out ownerSources))
        {
            ownerSources = new List<AudioSource>();
            _objectAudioSourcePools[owner] = ownerSources;
        }

        // 1. owner�Ɋ����̖��g�pAudioSource������΂���𗘗p
        foreach (var source in ownerSources)
        {
            if (source != null && !source.isPlaying && !source.enabled) // �����Ȃ��̂��ΏۂƂ��܂��B
            {
                ResetAudioSourceProperties(source); // �v���p�e�B��������ԂɃ��Z�b�g���܂��B
                source.enabled = true; // �L���ɂ��܂��B
                // GameObject����A�N�e�B�u�ȏꍇ�̓A�N�e�B�u�ɂ��܂��i�I�[�i�[���A�N�e�B�u�ł�AudioSource���g��GameObject����A�N�e�B�u�ȃP�[�X���l���j�B
                if (!source.gameObject.activeInHierarchy) source.gameObject.SetActive(true);
                return source;
            }
        }

        // 2. owner��AudioSource��������ɒB���Ă��Ȃ����m�F
        if (ownerSources.Count >= _maxPooledAudioSourcesPerObject)
        {
            Debug.LogWarning($"[SoundManager] �I�u�W�F�N�g'{owner.name}'��AudioSource�������({_maxPooledAudioSourcesPerObject})�ɒB���Ă��܂��B�V����AudioSource�𐶐��ł��܂���B");
            return null; // ����ɒB���Ă��邽�߁A�V����AudioSource�͐������܂���B
        }

        // 3. �O���[�o���v�[������擾
        if (_globalAudioSourcePool.Count > 0)
        {
            AudioSource pooledSource = _globalAudioSourcePool.Pop();
            if (pooledSource != null)
            {
                // ����GameObject��ύX
                pooledSource.transform.SetParent(owner.transform, false); // ���[�J�����W���ێ����Ȃ��悤�ɂ��܂��B
                pooledSource.gameObject.SetActive(true); // GameObject���A�N�e�B�u�ɂ��܂��B
                ResetAudioSourceProperties(pooledSource); // �v���p�e�B��������ԂɃ��Z�b�g���܂��B
                pooledSource.enabled = true; // AudioSource�R���|�[�l���g���̂��L���ɂ��܂��B

                ownerSources.Add(pooledSource);
                _audioSourceOwnerMap.Add(pooledSource, owner); // �I�[�i�[�}�b�v�ɓo�^���܂��B
                return pooledSource;
            }
        }

        // 4. �v�[���ɂȂ���ΐV�K����
        AudioSource newSource = owner.AddComponent<AudioSource>();
        newSource.playOnAwake = false; // �����Đ����Ȃ��悤�ɐݒ肵�܂��B
        ResetAudioSourceProperties(newSource); // �v���p�e�B��������ԂɃ��Z�b�g���܂� (�V�K�����������Z�b�g)�B
        newSource.enabled = true; // �V�K�������͎����I�ɗL���ɂȂ�܂��B

        ownerSources.Add(newSource);
        _audioSourceOwnerMap.Add(newSource, owner); // �I�[�i�[�}�b�v�ɓo�^���܂��B
        return newSource;
    }

    /// <summary>
    /// �g�p�ς݂�AudioSource���v�[���ɖ߂��܂��B
    /// ���̃��\�b�h�́AAudioSource��������GameObject���j�������\��������ꍇ�ɂ����S�ł��B
    /// </summary>
    /// <param name="audioSource">�v�[���ɖ߂�AudioSource�B</param>
    private void ReleaseAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        // AudioSource���A�N�e�B�u�ɂ��ASoundManager�̎q�ɂ��܂��B
        audioSource.Stop();
        audioSource.clip = null; // �N���b�v���N���A���܂��B
        audioSource.enabled = false; // AudioSource�R���|�[�l���g���̂𖳌��ɂ��܂��B
        audioSource.transform.SetParent(this.transform, false); // SoundManager�̎q�ɂ��܂��B
        audioSource.gameObject.SetActive(false); // GameObject���A�N�e�B�u�ɂ��܂��B

        _globalAudioSourcePool.Push(audioSource); // �O���[�o���v�[���ɖ߂��܂��B

        // �I�u�W�F�N�g�̃v�[��������Q�Ƃ��폜�i_audioSourceOwnerMap����owner�����j
        if (_audioSourceOwnerMap.TryGetValue(audioSource, out GameObject owner))
        {
            if (owner != null && _objectAudioSourcePools.TryGetValue(owner, out List<AudioSource> ownerSources))
            {
                ownerSources.Remove(audioSource);
                // �I�u�W�F�N�g�̃v�[������ɂȂ�����G���g�����폜�i�������ߖ�̂��߁j
                if (ownerSources.Count == 0)
                {
                    _objectAudioSourcePools.Remove(owner);
                }
            }
            _audioSourceOwnerMap.Remove(audioSource); // �}�b�v������폜���܂��B
        }
    }

    /// <summary>
    /// AudioSource��AudioMixerGroup��ݒ肵�܂��B
    /// </summary>
    /// <param name="audioSource">�ݒ�Ώۂ�AudioSource�B</param>
    /// <param name="mixerGroupName">���蓖�Ă�AudioMixerGroup�̖��O�B</param>
    private void SetAudioMixerGroup(AudioSource audioSource, string mixerGroupName)
    {
        if (!string.IsNullOrEmpty(mixerGroupName))
        {
            if (_mixerGroupCache.TryGetValue(mixerGroupName, out AudioMixerGroup mixerGroup))
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }
            else
            {
                Debug.LogWarning($"[SoundManager] �~�L�T�[�O���[�v'{mixerGroupName}'���L���b�V���Ɍ�����܂���BAudioMixerGroup�͐ݒ肳��܂���B");
                audioSource.outputAudioMixerGroup = null; // ������Ȃ��ꍇ�̓f�t�H���g�ɖ߂��܂��B
            }
        }
        else
        {
            audioSource.outputAudioMixerGroup = null; // �~�L�T�[�O���[�v���w�肳��Ȃ��ꍇ�̓f�t�H���g�ɖ߂��܂��B
        }
    }

    /// <summary>
    /// AudioSource�̃v���p�e�B��������ԂɃ��Z�b�g���܂��B
    /// ����́AAudioSource���v�[������ė��p����ۂɁA�ȑO�̐ݒ肪�c��Ȃ��悤�ɂ��邽�߂ɍs���܂��B
    /// </summary>
    /// <param name="audioSource">���Z�b�g����AudioSource�B</param>
    private void ResetAudioSourceProperties(AudioSource audioSource)
    {
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.panStereo = 0f;
        audioSource.spatialBlend = 0f; // �f�t�H���g��2D�BPlaySFXAt��3D�ɏ㏑�������\��������܂��B
        audioSource.outputAudioMixerGroup = null; // �f�t�H���g��None�i�~�L�T�[�O���[�v�Ȃ��j
        audioSource.loop = false; // �f�t�H���g�͔񃋁[�v
        audioSource.clip = null; // �N���b�v���N���A���܂��B
    }

    /// <summary>
    /// LoopSoundData ��BGM�Đ��ʒu���Ď����A�V�[�����X�ȃ��[�v�������s���܂��B
    /// </summary>
    /// <param name="audioSource">���[�v�������s��AudioSource�B</param>
    /// <param name="soundData">���[�v�����܂�SoundData (LoopSoundData�ł���K�v������܂�)�B</param>
    private void LoopCheck(AudioSource audioSource, SoundData soundData)
    {
        if (soundData is LoopSoundData soundLoopData)
        {
            // �T�E���h�f�[�^�̎��g����AudioSource�̃N���b�v���g���Ɋ�Â��āA�T���v�����O�ʒu�𒲐�����w���p�[�֐�
            int CorrectFrequency(long n)
            {
                return (int)(n * audioSource.clip.frequency / soundLoopData.frequency);
            }
            // ���݂̍Đ��ʒu�����[�v�I���n�_�𒴂����ꍇ�A���[�v�J�n�n�_�ɖ߂��܂��B
            if (audioSource.timeSamples >= CorrectFrequency(soundLoopData.loopEnd))
            {
                audioSource.timeSamples -= CorrectFrequency(soundLoopData.loopEnd - soundLoopData.loopStart);
            }
        }
    }

    /// <summary>
    /// �Ăяo�����̃��\�b�h�����ȈՓI�Ɏ擾���܂��B��Ƀf�o�b�O�p�r�ŁA�p�t�H�[�}���X�ɉe������\��������܂��B
    /// </summary>
    /// <returns>�Ăяo�����̃��\�b�h����\��������B</returns>
    private string GetCallingMethodName()
    {
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        // 0: GetCallingMethodName, 1: PlaySFXAt, 2: �Ăяo�������\�b�h
        if (stackTrace.FrameCount > 2)
        {
            return stackTrace.GetFrame(2).GetMethod().Name;
        }
        return "�s���ȃ��\�b�h";
    }
}