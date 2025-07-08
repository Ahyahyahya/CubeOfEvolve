using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.View;
using AT.uGUI;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterDropCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [Header("Models")]
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // �����^�C�����W���[���f�[�^���Ǘ�����}�l�[�W���[�B
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���}�X�^�[�f�[�^���Ǘ�����f�[�^�X�g�A�B
        [Header("Views")]
        [SerializeField] private ViewDropCanvas _dropView; // �h���b�vUI��\������View�R���|�[�l���g�B
        [Header("Views_Hovered")]
        [SerializeField] private TextMeshProUGUI _infoText; // ������
        [SerializeField] private TextMeshProUGUI _level; // 
        [SerializeField] private TextMeshProUGUI _levelNext; // 

        // ----- Private Members (�����f�[�^)
        private const int NUMBER_OF_OPTIONS = 3; // �񎦂��郂�W���[���̐��B
        private List<int> _candidateModuleIds = new List<int>();

        // ----- UnityMessage
        private void Start()
        {
            if (_dropView != null)
            {
                _dropView.OnModuleSelected
                    .Subscribe(x => HandleModuleSelected(x))
                    .AddTo(this); // R3 �� AddTo(CompositeDisposable) ���g�p�B
                _dropView.OnModuleHovered
                    .Subscribe(x => HandleModuleHovered(x))
                    .AddTo(this);
            }
        }
        private void Awake()
        {
            // �ˑ��֌W�����ݒ�̏ꍇ�A�V�[������擾�����݂�
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;

            // �K�{�̈ˑ��֌W�������Ă��邩�`�F�b�N
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("Drop_Presenter: RuntimeModuleManager, ModuleDataStore, �܂���Drop_View���ݒ肳��Ă��܂���B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
                enabled = false;
            }
        }

        #region ModelToView

        /// <summary>
        /// �h���b�v�I��UI��\�����鏀�������AView�ɕ\�����˗����܂��B
        /// </summary>
        public void PrepareAndShowDropUI()
        {
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("�ˑ��֌W����������Ă��܂���I");
                return;
            }

            var gameState = GameManager.Instance.CurrentGameState.CurrentValue;
            var displayIds = _runtimeModuleManager.GetDisplayModuleIds(NUMBER_OF_OPTIONS, gameState);
            var candidatePool = _runtimeModuleManager.AllRuntimeModuleData
                                  .Where(m => m.CurrentLevelValue < 5).ToList();

            if (displayIds.Count == 0)
            {
                //debug.log("�S���W���[�����ő僌�x���B�I�����Ȃ��B");
                var Player = FindFirstObjectByType(typeof(PlayerCore));
                Player.GetComponent<PlayerCore>().ReceiveMoney(500); // 500���ǉ�
                return;
            }

            _dropView.DisplayModulesByIdOrRandom(displayIds, candidatePool, _moduleDataStore);
            _dropView.GetComponent<CanvasCtrl>().OnOpenCanvas();
        }

        #endregion


        #region ViewToModel

        /// <summary>
        /// ���[�U�[�����W���[����I�������ۂ̃C�x���g�n���h���B
        /// View����̃C�x���g�iR3�ōw�ǁj�ɂ���ČĂяo����܂��B
        /// </summary>
        /// <param name="selectedModuleId">�I�����ꂽ���W���[����ID�B</param>
        private void HandleModuleSelected(int selectedModuleId)
        {
            if (selectedModuleId == -1) // ���ł��Ȃ����̂�I�������ꍇ
            {
            }
            else
            {
                // RuntimeModuleManager ����ă��W���[���̃��x���A�b�v���������s
                _runtimeModuleManager.LevelUpModule(selectedModuleId);
            }

            _dropView.gameObject.GetComponent<CanvasCtrl>().OnCloseCanvas();
        }

        /// <summary>
        /// ���W���[���Ƀ}�E�X�I�[�o�[�����ۂ̃C�x���g�n���h���B
        /// ���������X�V���܂��B
        /// </summary>
        /// <param name="EnterModuleId"></param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            var module = _moduleDataStore.FindWithId(EnterModuleId);
            var Rruntime = RuntimeModuleManager.Instance.GetRuntimeModuleData(EnterModuleId);

            //_unitName.text = module.ViewName;
            _infoText.text = module.Description;
            _level.text = $"{Rruntime.CurrentLevelValue}";
            _levelNext.text = $"{Rruntime.CurrentLevelValue + 1}";
            //_image.sprite = module.MainSprite;
            //_icon.sprite = module.BlockSprite;
            //_atk.text = $"{module.ModuleState?.Attack ?? 0}";
            //_rpd.text = $"{module.ModuleState?.Interval ?? 0}";
            //_prc.text = $"{module.BasePrice}";
        }


        #endregion

    }
}