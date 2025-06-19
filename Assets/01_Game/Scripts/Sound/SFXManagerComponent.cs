// �쐬���F250618
// �쐬�ҁFAT
//   �T�v �F��ԂɕR�Â�SE(SFX)�̍Đ��Ǘ��p�J�v�Z���B�q��������ɂŁA��p�I�u�W�F�N�g�ɃA�^�b�`�����B
// �g�����F�C�ӂ̃Q�[���I�u�W�F�N�g�ɃA�^�b�`���āA�g�p����������o�^���܂��傤�B

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Assets.AT
{
    [System.Serializable]
    public class SFXDataWrapper
    {
        public string name;
        public SoundData soundData;
    }

    public class SFXManagerComponent : MonoBehaviour
    {
        // -----SerializeField

        [SerializeField, Tooltip("����SFXManagerComponent���Ǘ�����AudioSource�̏����v�[����\n����ȏ�̓����Đ��ł����v�B�ǉ��Ő�������")]
        private int _initialAudioSourcePoolSize = 2;

        [SerializeField, Tooltip("���̃R���|�[�l���g�ōĐ�����SFX�̃��X�g")]
        private SFXDataWrapper[] _sfxDataArray; // ���̃R���|�[�l���g������SoundData��ێ�

        // -----Field

        // ����SFXManagerComponent���Ǘ�����AudioSource�̃v�[��
        private Stack<AudioSource> _sfxAudioSourcePool = new Stack<AudioSource>();

        // �݂��o������AudioSource�Ƃ��̊Ď��R���|�[�l���g�̊Ǘ�
        private Dictionary<AudioSource, OnAudioSourceFinished> _borrowedAudioSources = new Dictionary<AudioSource, OnAudioSourceFinished>();

        // SFX����SoundData�̑Ή���ێ����鎫��
        private Dictionary<string, SoundData> _sfxDictionary = new Dictionary<string, SoundData>();

        // -----UnityMessage

        private void Awake()
        {
            // SFX�f�[�^�������ɃZ�b�g
            foreach (var sfxDataWrapper in _sfxDataArray)
            {
                if (sfxDataWrapper.soundData != null)
                {
                    if (!_sfxDictionary.ContainsKey(sfxDataWrapper.name))
                    {
                        _sfxDictionary.Add(sfxDataWrapper.name, sfxDataWrapper.soundData);
                    }
                    else
                    {
                        Debug.LogWarning($"[SFXManagerComponent] SFX�� '{sfxDataWrapper.name}' �͊��ɓo�^����Ă��܂��B�㏑���͍s���܂���B�I�u�W�F�N�g: {gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogError($"[SFXManagerComponent] SFX '{sfxDataWrapper.name}' ���ݒ肳��Ă��܂���B�I�u�W�F�N�g: {gameObject.name}");
                }
            }

            // ���g��GameObject�̎q�Ƃ���AudioSource�v�[����������
            for (int i = 0; i < _initialAudioSourcePoolSize; i++)
            {
                GameObject audioSourceGO = new GameObject($"_SFXAudioSource_Pooled_{i}");
                audioSourceGO.transform.SetParent(this.transform); // ���g�̎q���ɂ���
                AudioSource newSource = audioSourceGO.AddComponent<AudioSource>();
                newSource.playOnAwake = false; // �����Đ����Ȃ�
                newSource.enabled = false;     // ������Ԃł͖���
                _sfxAudioSourcePool.Push(newSource);
            }
        }

        private void OnDestroy()
        {
            // �؂�Ă���AudioSource��S�Ē�~���A�Q�Ƃ��N���A
            foreach (var pair in _borrowedAudioSources.ToList()) // ToList()�ŃR�s�[���쐬
            {
                AudioSource audioSource = pair.Key;
                OnAudioSourceFinished monitor = pair.Value;

                if (monitor != null)
                {
                    monitor.StopMonitoring();
                }
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.clip = null;
                    audioSource.enabled = false;
                    // GameObject���̂��e�ƈꏏ�ɔj������邽�߁A�v�[���ɖ߂��K�v�͂Ȃ�
                }
            }
            _borrowedAudioSources.Clear();

            // �v�[������AudioSource���S�Ĕj���i�e���j������邽�߁A�����I�Ɏq���j������邪�����I�Ɂj
            foreach (var audioSource in _sfxAudioSourcePool)
            {
                if (audioSource != null) Destroy(audioSource.gameObject);
            }
            _sfxAudioSourcePool.Clear();
        }

        // -----Public

        /// <summary>
        /// ���̃I�u�W�F�N�g����SFX���Đ����܂��B���g�̃v�[������AudioSource���؂�܂��B
        /// </summary>
        /// <param name="sfxName">�Đ�����SFX�̕ʖ�</param>
        /// <param name="mixerGroupName">���蓖�Ă�AudioMixerGroup�̖��O�i�I�v�V�����j</param>
        /// <param name="loop">���[�v�Đ����邩�ǂ����i�f�t�H���g: false�j</param>
        public void PlaySFX(string sfxName, string mixerGroupName = null, bool loop = false)
        {
            if (!_sfxDictionary.TryGetValue(sfxName, out var soundData)) // ���g�̎�������擾
            {
                Debug.LogWarning($"[SFXManagerComponent] SFX '{sfxName}' ��SoundData�����̃R���|�[�l���g�ɓo�^����Ă��܂���B�I�u�W�F�N�g: {gameObject.name}");
                return;
            }

            AudioClip clipToPlay = soundData.GetAudioClip();
            if (clipToPlay == null)
            {
                Debug.LogWarning($"[SFXManagerComponent] SFX�I�[�f�B�I�N���b�v��������܂���: {sfxName}�B�I�u�W�F�N�g: {gameObject.name}");
                return;
            }

            AudioSource audioSource = GetUnusedAudioSource();
            if (audioSource == null)
            {
                Debug.LogWarning($"[SFXManagerComponent] SFX '{sfxName}' �p��AudioSource���擾�ł��܂���ł����B�v�[�����͊����Ă���\��������܂��B�I�u�W�F�N�g: {gameObject.name}");
                return;
            }

            // AudioSource�̐ݒ�
            ResetAudioSourceProperties(audioSource); // �v���p�e�B��������ԂɃ��Z�b�g���܂�
            audioSource.clip = clipToPlay;
            audioSource.loop = loop;
            audioSource.spatialBlend = 1f; // SFX�͒ʏ�3D�T�E���h

            // �~�L�T�[�O���[�v�ݒ� (GameSoundManager����擾)
            if (GameSoundManager.Instance != null) // GameSoundManager�̃C���X�^���X���Ȃ��ꍇ�͐ݒ肵�Ȃ�
            {
                GameSoundManager.Instance.SetAudioMixerGroup(audioSource, mixerGroupName);
            }
            else
            {
                Debug.LogWarning("[SFXManagerComponent] GameSoundManager��������Ȃ����߁AAudioMixerGroup��ݒ�ł��܂���B");
            }

            // AudioSource�̍Đ��I���Ď��ƃv�[���ԋp����
            OnAudioSourceFinished finishedMonitor = audioSource.gameObject.GetOrAddComponent<OnAudioSourceFinished>();

            Action onFinished = () =>
            {
                if (_borrowedAudioSources.ContainsKey(audioSource))
                {
                    _borrowedAudioSources.Remove(audioSource);
                    ReturnAudioSource(audioSource);
                }
            };

            // �Ď����J�n���A�Đ�
            finishedMonitor.Monitor(audioSource, onFinished);
            audioSource.Play(); // �����ōĐ����J�n
            _borrowedAudioSources.Add(audioSource, finishedMonitor);
        }

        /// <summary>
        /// �����SFX�̍Đ����~���܂��B
        /// </summary>
        /// <param name="sfxName">��~����SFX�̕ʖ�</param>
        public void StopSFX(string sfxName)
        {
            // SoundData�����̃R���|�[�l���g�ɑ��݂��邩�m�F
            if (!_sfxDictionary.TryGetValue(sfxName, out var soundData))
            {
                Debug.LogWarning($"[SFXManagerComponent] ��~������SFX '{sfxName}' ��SoundData�����̃R���|�[�l���g�ɓo�^����Ă��܂���B�I�u�W�F�N�g: {gameObject.name}");
                return;
            }

            AudioClip clipToStop = soundData.GetAudioClip();
            if (clipToStop == null) return;

            foreach (var pair in _borrowedAudioSources.ToList())
            {
                AudioSource audioSource = pair.Key;
                OnAudioSourceFinished monitor = pair.Value;

                if (audioSource != null && audioSource.clip == clipToStop && audioSource.isPlaying)
                {
                    if (monitor != null) monitor.StopMonitoring();
                    ReturnAudioSource(audioSource);
                    _borrowedAudioSources.Remove(audioSource);
                    return; // �ŏ��̌����������̂��~���ďI��
                }
            }
        }

        /// <summary>
        /// ����SFXManagerComponent���Đ����Ă���S�Ă�SFX���~���܂��B
        /// </summary>
        public void StopAllSFX()
        {
            foreach (var pair in _borrowedAudioSources.ToList())
            {
                AudioSource audioSource = pair.Key;
                OnAudioSourceFinished monitor = pair.Value;

                if (monitor != null) monitor.StopMonitoring();
                ReturnAudioSource(audioSource);
            }
            _borrowedAudioSources.Clear();
        }

        // -----Private

        /// <summary>
        /// ���g�̎q�ɂ���AudioSource�v�[������A���ݍĐ����łȂ�AudioSource���擾���܂��B
        /// �v�[�����͊������ꍇ�́A�V���Ɏ��g�̎q���Ƃ��Đ������܂��B
        /// </summary>
        /// <returns>���p�\��AudioSource�A�܂���null�i�G���[�������j�B</returns>
        private AudioSource GetUnusedAudioSource()
        {
            if (_sfxAudioSourcePool.Count > 0)
            {
                AudioSource source = _sfxAudioSourcePool.Pop();
                if (source != null)
                {
                    ResetAudioSourceProperties(source);
                    source.enabled = true;
                    return source;
                }
            }

            Debug.LogWarning($"[SFXManagerComponent] AudioSource�v�[�����͊����Ă��܂��i�I�u�W�F�N�g: {gameObject.name}�j�B�V����AudioSource�𐶐����܂��B");
            GameObject audioSourceGO = new GameObject($"_SFXAudioSource_New_{Guid.NewGuid().ToString().Substring(0, 8)}");
            audioSourceGO.transform.SetParent(this.transform); // ���g�̎q���ɂ���
            AudioSource newSource = audioSourceGO.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            ResetAudioSourceProperties(newSource);
            return newSource;
        }

        /// <summary>
        /// �g�p�ς݂�AudioSource���v�[���ɖ߂��܂��B
        /// </summary>
        /// <param name="audioSource">�v�[���ɖ߂�AudioSource�B</param>
        private void ReturnAudioSource(AudioSource audioSource)
        {
            if (audioSource == null) return;
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.enabled = false;
            // �e�͊��ɂ���SFXManagerComponent��GameObject�Ȃ̂ŕύX�s�v
            _sfxAudioSourcePool.Push(audioSource);
        }

        /// <summary>
        /// AudioSource�̃v���p�e�B��������ԂɃ��Z�b�g���܂��B
        /// </summary>
        /// <param name="audioSource">���Z�b�g����AudioSource�B</param>
        private void ResetAudioSourceProperties(AudioSource audioSource)
        {
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.panStereo = 0f;
            audioSource.spatialBlend = 0f; // �f�t�H���g��2D�BPlaySFX��3D�ɏ㏑������܂��B
            audioSource.outputAudioMixerGroup = null;
            audioSource.loop = false;
            audioSource.clip = null;
        }
    }
}