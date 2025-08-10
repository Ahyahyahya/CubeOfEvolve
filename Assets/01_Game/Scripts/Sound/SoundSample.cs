using UnityEngine;
using System.Collections;
using App.GameSystem.Modules;
using App.BaseSystem.DataStores.ScriptableObjects.Modules;

namespace Assets.AT
{
    public class SoundSample : MonoBehaviour
    {
        [SerializeField] private GameObject _soundSourceObj;
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���}�X�^�[�f�[�^���i�[����f�[�^�X�g�A�B

        private GameSoundManager SM;
        private bool _isPlay = true;

        private void Start()
        {
            SM = GameSoundManager.Instance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && _isPlay) // ���炷
            {
                SM.PlaySE("SampleSE", "SE");
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.F) && _isPlay) // �u�b�s�K���I �w��ӏ�����
            {
                //_soundSourceObj.GetComponent<SFXManagerComponent>().PlaySFX("SampleSE", "SE", false);
                GameSoundManager.Instance.PlaySFX("Hit_Bom", _soundSourceObj.transform, "SE");
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.T) && _isPlay) // bgm�����t�F�[�h
            {
                SM.StopBGMWithFade(1f);
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            if (Input.GetKeyDown(KeyCode.G) && _isPlay) // bgm�炷
            {
                SM.PlayBGM("SampleBGM", "BGM", 3f);
                _isPlay = false;
                StartCoroutine(ResetLogFlag());
            }

            /* L �������ƁA�S���̃��W���[���̐��ʂ�10�ɁA�I�v�V�������������ׂẴ��x����5�ɂ���R�[�h */
            if (Input.GetKeyDown(KeyCode.L) && _isPlay)
            {
                var runtimeModuleManager = RuntimeModuleManager.Instance;
                foreach (var module in runtimeModuleManager.AllRuntimeModuleData)
                {
                    if (_moduleDataStore.FindWithId(module.Id).ModuleType != ModuleData.MODULE_TYPE.Options)
                    {
                        module.SetLevel(5);
                    }
                    else
                    {
                        // �I�v�V�����̓��x����1�ɐݒ�
                        module.SetLevel(1);
                    }
                    module.SetQuantity(10);
                }
                _isPlay = false;
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(ResetLogFlag());
        }

        private IEnumerator ResetLogFlag()
        {
            yield return new WaitForSeconds(0.5f);
            _isPlay = true;
        }
    }
}

