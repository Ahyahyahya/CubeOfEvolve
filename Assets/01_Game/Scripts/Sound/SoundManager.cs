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

// �T�E���h�f�[�^�̊��N���X�iScriptableObject�j�B
// �e�T�E���h�f�[�^�͂��̃N���X���p�����ċ�̓I�ȃf�[�^�����B
public abstract class SoundData : ScriptableObject
{
    // �T�E���h�̖��O�i���ʎq�j�B
    public string name;

    // �T�E���h�̃I�[�f�B�I�N���b�v���擾���钊�ۃ��\�b�h�B
    public abstract AudioClip GetAudioClip();

    // �T�E���h�̖��O���擾���钊�ۃ��\�b�h�B
    public abstract string GetName();
}

// �P��̃I�[�f�B�I�N���b�v�����T�E���h�f�[�^�B
[CreateAssetMenu(fileName = "AudioClipData", menuName = "Sound/AudioClipData")]
public class AudioClipData : SoundData
{
    // �Đ�����I�[�f�B�I�N���b�v�B
    public AudioClip audioClip;

    // �I�[�f�B�I�N���b�v��Ԃ������B
    public override AudioClip GetAudioClip()
    {
        return audioClip;
    }

    // �T�E���h�̖��O��Ԃ������B
    public override string GetName()
    {
        return name;
    }
}

// ���[�v�Đ��ɑΉ������T�E���h�f�[�^�B
[CreateAssetMenu(fileName = "SoundLoopData", menuName = "Sound/SoundLoopData")]
public class LoopSoundData : SoundData
{
    // �Đ�����I�[�f�B�I�N���b�v�B
    public AudioClip audioClip;

    // ���[�v�J�n�ʒu�i�T���v���P�ʁj�B
    public int loopStart;

    // ���[�v�I���ʒu�i�T���v���P�ʁj�B
    public int loopEnd;

    // �I�[�f�B�I�N���b�v�̃T���v�����O���g���B
    public int frequency = 44100;

    // �I�[�f�B�I�N���b�v��Ԃ������B
    public override AudioClip GetAudioClip()
    {
        return audioClip;
    }

    // �T�E���h�̖��O��Ԃ������B
    public override string GetName()
    {
        return name;
    }
}

// �Q�[���S�̂̃T�E���h�Ǘ����s���V���O���g���N���X�B
public class SoundManager : MonoBehaviour
{
    // SoundManager �̃V���O���g���C���X�^���X�B
    public static SoundManager Instance { get; private set; }

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

    // �Đ��Ɏg�p���� AudioSource �̃��X�g�B
    private AudioSource[] audioSourceList = new AudioSource[20];

    // �T�E���h���� SoundData �̑Ή���ێ����鎫���B
    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

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

    // �V�[������ BGM ���̑Ή���ێ����鎫���B
    private Dictionary<string, string> sceneBGMMapping = new Dictionary<string, string>();

    // ���ݍĐ����� BGM �� AudioSource�B
    private AudioSource currentBGMSource;

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

    // OnDestroy ���\�b�h�F�C���X�^���X���j�������ۂ̏����B�V�[�����[�h�C�x���g�̉������s���B
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // OnSceneLoaded ���\�b�h�F�V�[�������[�h���ꂽ�ۂɌĂяo����A�V�[���ɑΉ����� BGM ���Đ�����B
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    // Update ���\�b�h�F���t���[���Ăяo����A���ݍĐ����� BGM �̃��[�v�������s���B
    private void Update()
    {
        if (currentBGMSource != null && currentBGMSource.isPlaying)
        {
            LoopCheck(currentBGMSource);
        }
    }

    // LoopCheck ���\�b�h�FAudioSource �̍Đ��ʒu���Ď����ALoopSoundData �Ɋ�Â��ă��[�v�������s���B
    private void LoopCheck(AudioSource audioSource)
    {
        if (soundDictionary.TryGetValue(audioSource.clip.name, out var soundData))
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

    // PlayBGMForScene ���\�b�h�F�w�肳�ꂽ�V�[�����ɑΉ����� BGM ���Đ�����B
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

    // GetUnusedAudioSource ���\�b�h�F���ݍĐ����łȂ� AudioSource ���擾����B
    private AudioSource GetUnusedAudioSource()
    {
        for (var i = 0; i < audioSourceList.Length; ++i)
        {
            if (!audioSourceList[i].isPlaying) return audioSourceList[i];
        }
        return null; // �S�Ă� AudioSource ���g�p���̏ꍇ�� null ��Ԃ��B
    }
}