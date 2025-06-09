using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using MVRP.AT.View;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.AT.Presenter
{
    /// <summary>
    /// �r���h��ʂ̃v���[���^�[��S������N���X�B
    /// View����̃C�x���g��R3�ōw�ǂ��AModel�iRuntimeModuleManager, ModuleDataStore�j�𑀍삵�A
    /// View�ɕ\���f�[�^��n���܂��B
    /// </summary>
    public class Build_Presenter : MonoBehaviour
    {
        // ----- SerializedField

        // Models
        [SerializeField] private PlayerBuilder _builder;
        [SerializeField] private Build_View _buildView; // �r���hUI��\������View�R���|�[�l���g�B
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���}�X�^�[�f�[�^���Ǘ�����f�[�^�X�g�A�B
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // �����^�C�����W���[���f�[�^���Ǘ�����}�l�[�W���[�B

        // Views
        [SerializeField] private TextMeshProUGUI _hoveredModuleInfoText;
        [SerializeField] private Button _exitButton;

        // ----- Private Members (�����f�[�^)
        private CompositeDisposable _disposables = new CompositeDisposable(); // �S�̂̍w�ǉ������Ǘ�����CompositeDisposable�B
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // �e���W���[���̃��x���E���ʕύX�w�ǂ��Ǘ�����CompositeDisposable�B

        // ----- UnityMessage
        private void Awake()
        {
            // �ˑ��֌W�̎擾�ƃ`�F�b�N
            if (_builder == null) Debug.LogError("Build_Presenter: PlayerBuilder���A�^�b�`����Ă��܂���I", this);
            if (_buildView == null) Debug.LogError("Build_Presenter: BuildView��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_moduleDataStore == null) Debug.LogError("Build_Presenter: ModuleDataStore��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            if (_exitButton == null) Debug.LogError("Build_Presenter: ExitButton��Inspector�Őݒ肳��Ă��܂���I", this); // ���̃G���[���b�Z�[�W�͓K�؂ɏC�����܂����B

            // �e�ˑ��֌W�������Ă��邩�ŏI�`�F�b�N
            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null || _exitButton == null)
            {
                Debug.LogError("Build_Presenter: �ˑ��֌W���s�����Ă��܂��BInspector�̐ݒ�ƃV�[���̃Z�b�g�A�b�v���m�F���Ă��������B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
                enabled = false;
                return;
            }

            // View����̃��W���[���I�����N�G�X�g���w��
            _buildView.OnModuleChoiceRequested
                .Subscribe(moduleId => HandleModuleChoiceRequested(moduleId))
                .AddTo(_disposables);

            _buildView.OnModuleHovered
                .Subscribe(x => HandleModuleHovered(x))
                .AddTo(this); // ������_disposables�ɒǉ����Ă��ǂ���������܂���B

            // RuntimeModuleManager���Ǘ����郂�W���[���R���N�V�����S�̂̕ύX���Ď����A�r���hUI���X�V����
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ => {
                    Debug.Log("RuntimeModuleData�R���N�V�������ύX����܂����B���W���[���̕ύX�w�ǂ��Đݒ肵�A�r���hUI���X�V���܂��B");
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
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // �e���W���[���̃��x���E���ʕύX�w�ǂ�����
        }

        /// <summary>
        /// ���f���̕ύX���Ď����A�r���[�ɕ\���f�[�^��n�������������ɋL�q���܂��B
        /// </summary>
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
                    .Subscribe(level => {
                        Debug.Log($"���W���[��ID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) �̃��x���� {level} �ɕύX����܂����B�r���hUI���X�V���܂��B");
                        DisplayBuildUI(); // ���x�����ύX���ꂽ��r���h��ʂ��ĕ\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            if (runtimeModuleData.Quantity != null) // ���ʂ̊Ď����d�v�Ȃ̂Œǉ�
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity => {
                        Debug.Log($"���W���[��ID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) �̐��ʂ� {quantity} �ɕύX����܂����B�r���hUI���X�V���܂��B");
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
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Build_Presenter: �I���{�^���̃C���^���N�g�\�����X�V���邽�߂̕K�v�ȃf�[�^���s�����Ă��܂��B", this);
                return;
            }

            // �r���h��ʂɕ\������Ă��邷�ׂẴ��W���[���i1�ȏ�̂��́j�ɂ��ă`�F�b�N
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                             .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                // ��������0�łȂ���ΑI���\
                bool canChoose = runtimeData.CurrentQuantityValue > 0;

                _buildView.SetChoiceButtonInteractable(runtimeData.Id, canChoose);
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

            // �I����ʂ̏���
            _exitButton.onClick.Invoke();

            // �����ӁF�r���h��ʂɈڍs���鏈��
            // �����ӁF�ݒu��ɏ����������炷����

            _builder?.SetModuleData(masterData);

            Debug.Log($"Build_Presenter: �v���C���[�����W���[��ID {moduleId} ({masterData.ViewName}) ��I�����܂����B", this);

            // �I�𐬌����̃t�B�[�h�o�b�N (UI�X�V�Ȃ�)
            UpdateChoiceButtonsInteractability();
        }

        /// <summary>
        /// ���W���[���Ƀ}�E�X�I�[�o�[�����ۂ̃C�x���g�n���h���B
        /// ���������X�V���܂��B
        /// </summary>
        /// <param name="EnterModuleId">�}�E�X�I�[�o�[���ꂽ���W���[����ID�B</param>
        private void HandleModuleHovered(int EnterModuleId)
        {
            _hoveredModuleInfoText.text = _moduleDataStore.FindWithId(EnterModuleId).Description;
        }

        #endregion

    }
}