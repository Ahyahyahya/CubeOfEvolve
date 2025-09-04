using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using Assets.IGC2025.Scripts.View;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public sealed class PresenterBuildCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializedField

        [Header("Models")]
        [SerializeField] private PlayerBuilder _builder;
        [SerializeField] private ViewBuildCanvas _buildView;
        [SerializeField] private ModuleDataStore _moduleDataStore;
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private PlayerCore _playerCore;

        [Header("Views")]
        [SerializeField] private TextMeshProUGUI _cubeQuantityText;

        // ----- Private Members (�����f�[�^)
        private CompositeDisposable _disposables = new CompositeDisposable(); // �S�̂̍w�ǉ������Ǘ�����CompositeDisposable�B
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // �e���W���[���̃��x���E���ʕύX�w�ǂ��Ǘ�����CompositeDisposable�B

        // -----Field
        public bool IsInitialized { get; private set; } = false;

        // -----UnityMessages

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // �e���W���[���̃��x���E���ʕύX�w�ǂ�����
        }

        #region ModelToView

        /// <summary>
        /// �eRuntimeModuleData�̃��x���Ɛ��ʕύX���w�ǂ���w���p�[���\�b�h�ł��B
        /// </summary>
        /// <param name="runtimeModuleData">�w�ǑΏۂ�RuntimeModuleData�B</param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            // Level�܂���Quantity�̕ύX���w��
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level =>
                    {
                        DisplayBuildUI(); // ���x�����ύX���ꂽ��r���h��ʂ��ĕ\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            if (runtimeModuleData.Quantity != null) // ���ʂ̊Ď����d�v�Ȃ̂Œǉ�
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity =>
                    {
                        DisplayBuildUI(); // ���ʂ��ύX���ꂽ��r���h��ʂ��ĕ\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables);
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} ��Level�܂���Quantity��ReactiveProperty�Ƃ��Č��J���Ă��܂���B", this);
            }
        }

        /// <summary>
        /// �r���h��ʂ�\�����鏀�������AView�ɕ\�����˗����܂��B
        /// ���̃��\�b�h�͊O������Ăяo����܂��i��: GameManager��UIController�j�B
        /// �܂��ARuntimeModuleData�̕ύX�ɂ���Ă������I�ɌĂяo����邱�Ƃ�����܂��B
        /// </summary>
        private void DisplayBuildUI()
        {
            // �Q��NullCheck
            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null)
            {
                Debug.LogError("Build_Presenter: �r���hUI���������邽�߂̈ˑ��֌W����������Ă��܂���IAwake�̃��O���m�F���Ă��������B", this);
                return;
            }

            // ������1�ȏ�̃��W���[���݂̂�View�ɓn��
            List<RuntimeModuleData> choiceRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0)
                .ToList();

            _buildView.DisplayBuildModules(choiceRuntimeModules, _moduleDataStore);
            UpdateChoiceButtonsInteractability();
        }

        /// <summary>
        /// �e���W���[���̑I���{�^���̃C���^���N�g�\��Ԃ��X�V���܂��B
        /// </summary>
        private void UpdateChoiceButtonsInteractability()
        {
            if (_runtimeModuleManager == null ||
                _moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Build_Presenter: �I���{�^���̍X�V�ɕK�v�Ȉˑ����s�����Ă��܂��B", this);
                return;
            }

            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                             .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0))
            {
                var masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;
                _buildView.SetChoiceButtonInteractable(runtimeData.Id, runtimeData.CurrentQuantityValue > 0);
            }
        }

        #endregion

        #region ViewToModel

        /// <summary>
        /// ���W���[���I�����N�G�X�g���󂯎�����ۂ̃n���h���ł��B
        /// </summary>
        /// <param name="moduleId">�I�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void HandleModuleChoiceRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Build_Presenter: ���W���[��ID {moduleId} �̃}�X�^�[�f�[�^��������܂���B�I���������ł��܂���B", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Build_Presenter: ���W���[��ID {moduleId} �̃����^�C���f�[�^��������܂���B����͑S�Ẵv���C���[�Ƀ��W���[��������������Ă���ꍇ�͔������Ȃ��͂��ł��B", this);
                return;
            }

            // ������0�̃��W���[���͑I���ł��Ȃ�
            if (runtimeModule.CurrentQuantityValue == 0)
            {
                Debug.LogWarning($"Build_Presenter: ���W���[��ID {moduleId} ({masterData.ViewName}) �͎����Ă��Ȃ����ߑI���ł��܂���B", this);
                return;
            }

            _builder?.SetModuleData(masterData);

            // �I�𐬌����̃t�B�[�h�o�b�N (UI�X�V�Ȃ�)
            UpdateChoiceButtonsInteractability();
        }

        /// <summary>
        /// ���W���[���Ƀ}�E�X�I�[�o�[�����ۂ̃C�x���g�n���h��
        /// </summary>
        /// <param name="EnterModuleId">�}�E�X�I�[�o�[���ꂽ���W���[����ID�B</param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            var module = _moduleDataStore.FindWithId(EnterModuleId);
            var Rruntime = RuntimeModuleManager.Instance.GetRuntimeModuleData(EnterModuleId);
        }

        #endregion

        // -----PublicMethod
        public void InInventory()
        {
            GameSoundManager.Instance.PlaySE("inv_in", "SE");
        }
        public void OutInventory()
        {
            GameSoundManager.Instance.PlaySE("inv_out", "SE");
        }

        public void Initialize()
        {
            if (IsInitialized) return;

            // ������
            _playerCore.CubeCount
                .CombineLatest(_playerCore.MaxCubeCount, (cube, maxCube) => new { cube, maxCube })
                .Subscribe(x =>
                {
                    _cubeQuantityText.text = $"{_playerCore.MaxCubeCount.CurrentValue - x.cube}";
                }).AddTo(_disposables);

            // �ˑ��֌W�̎擾�ƃ`�F�b�N
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;

            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null)
            {
                Debug.LogError($"{nameof(PresenterBuildCanvas)}: �ˑ��s���ŏ������𒆒f���܂��B", this);
                enabled = false;
                return;
            }

            // View����̃��W���[���I�����N�G�X�g���w��
            _buildView.OnModuleChoiceRequested
                .Subscribe(moduleId => HandleModuleChoiceRequested(moduleId))
                .AddTo(_disposables);

            // RuntimeModuleManager���Ǘ����郂�W���[���R���N�V�����S�̂̕ύX���Ď����A�r���hUI���X�V����
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {

                    // �����̃��W���[�����x���E���ʕύX�w�ǂ�S�ĉ���
                    _moduleLevelAndQuantityChangeDisposables.Clear();

                    // ���݂̑S�Ẵ��W���[���ɑ΂��ă��x���E���ʕύX���w��
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd);
                    }
                    DisplayBuildUI(); // �r���h��ʂ��ĕ\�����ă��X�g���X�V
                })
                .AddTo(_disposables);

            // �����\���̂��߂Ƀr���hUI���������ĕ\��
            DisplayBuildUI();

            // ����
            IsInitialized = true;
#if UNITY_EDITOR
            Debug.Log($"{nameof(PresenterBuildCanvas)} initialized.", this);
#endif
        }
    }
}