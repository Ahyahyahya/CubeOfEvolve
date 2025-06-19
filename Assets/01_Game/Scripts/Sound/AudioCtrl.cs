using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Assets.AT
{
    public class AudioCtrl : MonoBehaviour
    {
        [Serializable]
        public struct AudioSliderPair
        {
            public string GroupName;
            public Slider Slider;
        }

        // ---------------------------- SerializeField
        [SerializeField] private AudioMixer _audioMixer;

        [SerializeField] private List<AudioSliderPair> _audioSliderList = new List<AudioSliderPair>();

        // AudioMixer�̉��ʂ̍ŏ��l�idB�j�ƍő�l�idB�j
        [SerializeField] private float _minVolumeDB = -79f;
        [SerializeField] private float _maxVolumeDB = 1f;

        // ---------------------------- Field


        // ---------------------------- UnityMessage

        private void Start()
        {
            SetSliderCtrl();
        }

        // ---------------------------- PublicMethod

        public void SetSliderCtrl()
        {
            if (_audioMixer == null)
                return;

            foreach (var group in _audioSliderList)
            {
                string groupName = group.GroupName;
                Slider slider = group.Slider;

                // AudioMixer���猻�݂̉��ʂ��擾�idB�j
                if (slider != null)
                {
                    // AudioMixer���猻�݂̉��ʂ��擾�idB�j
                    if (_audioMixer.GetFloat(groupName, out float volumeDB))
                    {
                        // dB�l��0�`1��Slider�l�ɕϊ�
                        float sliderValue = Mathf.InverseLerp(_minVolumeDB, _maxVolumeDB, volumeDB);
                        slider.value = sliderValue;

                        // �����̃��X�i�[���폜���ĐV�������X�i�[��ǉ�
                        //slider.onValueChanged.RemoveAllListeners();
                        slider.onValueChanged.AddListener(value => OnValueChangedGroup(groupName, value));
                    }
                    else
                    {
                        Debug.LogWarning($"AudioMixer��Exposed Parameter '{groupName}' ��������܂���B");
                    }
                }
            }
        }

        // ---------------------------- PrivateMethod

        private void OnValueChangedGroup(string GroupName, float Value)
        {
            // 0�`1��Slider�l��dB�ɕϊ�
            float dBValue = Mathf.Lerp(_minVolumeDB, _maxVolumeDB, Value);
            _audioMixer.SetFloat(GroupName, dBValue); // SE�O���[�v�̉��ʂ�ݒ�
        }
    }
}