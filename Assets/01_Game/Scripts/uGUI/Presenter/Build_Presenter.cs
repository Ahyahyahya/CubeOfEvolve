using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using MVRP.AT.View;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.AT.Presenter
{
    public class Build_Presenter : MonoBehaviour
    {
        // ----- SerializedField

        // Models
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
        /// <summary>
        /// Awake�̓X�N���v�g�C���X�^���X�����[�h���ꂽ�Ƃ��ɌĂяo����܂��B
        /// �ˑ��֌W�̎擾�Ə����ݒ���s���܂��B
        /// </summary>
        void Awake()
        {
            // �ˑ��֌W�̎擾�ƃ`�F�b�N
            if (_buildView == null) Debug.LogError("build_Presenter: buildView��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_moduleDataStore == null) Debug.LogError("build_Presenter: ModuleDataStore��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            if (_exitButton == null) Debug.LogError("build_Presenter: ExitButton��Inspector�Őݒ肳��Ă��܂���I�J�X�I�I�I", this);

            // �e�ˑ��֌W�������Ă��邩�ŏI�`�F�b�N
            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null)
            {
                Debug.LogError("build_Presenter: �ˑ��֌W���s�����Ă��܂��BInspector�̐ݒ�ƃV�[���̃Z�b�g�A�b�v���m�F���Ă��������B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
                enabled = false;
                return;
            }

            // View����̃��W���[���w�����N�G�X�g���w��
            _buildView.OnModuleChoiceRequested
                .Subscribe(moduleId => HandleModuleChoiceRequested(moduleId))
                .AddTo(_disposables);

            _buildView.OnModuleHovered
                .Subscribe(x => HandleModuleHovered(x))
                .AddTo(this);

            // RuntimeModuleManager���Ǘ����郂�W���[���R���N�V�����S�̂̕ύX���Ď����A�V���b�vUI���X�V����
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ => {
                    Debug.Log("RuntimeModuleData�R���N�V�������ύX����܂����B���W���[���̕ύX�w�ǂ��Đݒ肵�A�V���b�vUI���X�V���܂��B");
                    // �����̃��W���[�����x���E���ʕύX�w�ǂ�S�ĉ���
                    _moduleLevelAndQuantityChangeDisposables.Clear();

                    // ���݂̑S�Ẵ��W���[���ɑ΂��ă��x���E���ʕύX���w��
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd);
                    }
                    ChoiceAndShowBuildUI(); // �V���b�v���ĕ\�����ă��X�g���X�V
                })
                .AddTo(_disposables);

            // �����\���̂��߂ɃV���b�vUI���������ĕ\��
            ChoiceAndShowBuildUI();
        }

        /// <summary>
        /// OnDestroy�̓Q�[���I�u�W�F�N�g���j�������Ƃ��ɌĂяo����܂��B
        /// �S�Ă̍w�ǂ��������A���\�[�X��������܂��B
        /// </summary>
        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // �e���W���[���̃��x���E���ʕύX�w�ǂ�����
        }

        // ----- Private Methods (�v���C�x�[�g���\�b�h)
        /// <summary>
        /// �eRuntimeModuleData�̃��x���Ɛ��ʕύX���w�ǂ���w���p�[���\�b�h�ł��B
        /// </summary>
        /// <param name="runtimeModuleData">�w�ǑΏۂ�RuntimeModuleData�B</param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            // Level�̕ύX���w��
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level => {
                        Debug.Log($"���W���[��ID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) �̃��x���� {level} �ɕύX����܂����B�V���b�vUI���X�V���܂��B");
                        ChoiceAndShowBuildUI(); // ���x�����ύX���ꂽ��V���b�v���ĕ\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} ��Level��ReactiveProperty�Ƃ��Č��J���Ă��܂���B", this);
            }
        }

        /// <summary>
        /// �e���W���[���̍w���{�^���̃C���^���N�g�\��Ԃ��X�V���܂��B
        /// </summary>
        private void UpdateChoiceButtonsInteractability()
        {
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("build_Presenter: �w���{�^���̃C���^���N�g�\�����X�V���邽�߂̕K�v�ȃf�[�^���s�����Ă��܂��B", this);
                return;
            }

            // �r���h��ʂɕ\������Ă��邷�ׂẴ��W���[���i1�ȏ�̂��́j�ɂ��ă`�F�b�N
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                             .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                bool canAfford = runtimeData.CurrentQuantityValue > 0;

                // ���x����1�ȏ�ŃV���b�v�ɕ\������Ă��郂�W���[���́A�������������΍w���\
                // ������w���ł��邽�߁A��ɃC���^���N�g�\�Ƃ���i����������������j�B
                _buildView.SetChoiceButtonInteractable(runtimeData.Id, canAfford);
            }
        }

        /// <summary>
        /// ���W���[���w�����N�G�X�g���󂯎�����ۂ̃n���h���ł��B
        /// </summary>
        /// <param name="moduleId">�w�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void HandleModuleChoiceRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"build_Presenter: ���W���[��ID {moduleId} �̃}�X�^�[�f�[�^��������܂���B�w���������ł��܂���B", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Build_Presenter: ���W���[��ID {moduleId} �̃����^�C���f�[�^��������܂���B����͑S�Ẵv���C���[�Ƀ��W���[��������������Ă���ꍇ�͔������Ȃ��͂��ł��B", this);
                return;
            }

            // ������0�̃��W���[���͍w���ł��Ȃ�
            if (runtimeModule.CurrentQuantityValue == 0)
            {
                Debug.LogWarning($"Build_Presenter: ���W���[��ID {moduleId} ({masterData.ViewName}) �͎����ĂȂ��̂őI���ł��܂���B", this);
                return;
            }

            // �I����ʂ̏���
            _exitButton.onClick.Invoke();

            // �����ӁF�r���h��ʂɈڍs���鏈��
            // �����ӁF�ݒu��ɏ����������炷����


            Debug.Log($"Build_Presenter: �v���C���[�����W���[��ID {moduleId} ({masterData.ViewName}) ��I������", this);

            // �I�𐬌����̐����̃t�B�[�h�o�b�N (UI�X�V�Ȃ�)
            UpdateChoiceButtonsInteractability();

        }

        private void HandleModuleHovered(int EnterModuleId)
        {
            _hoveredModuleInfoText.text = _moduleDataStore.FindWithId(EnterModuleId).Description;
        }

        // ----- Public
        /// <summary>
        /// �r���h��ʂ�\�����鏀�������AView�ɕ\�����˗����܂��B
        /// ���̃��\�b�h�͊O������Ăяo����܂��i��: GameManager��UIController�j�B
        /// �܂��ARuntimeModuleData�̕ύX�ɂ���Ă������I�ɌĂяo����邱�Ƃ�����܂��B
        /// </summary>
        private void ChoiceAndShowBuildUI()
        {
            // �Q��NullCheck
            if (_buildView == null || _moduleDataStore == null || _runtimeModuleManager == null)
            {
                Debug.LogError("build_Presenter: �V���b�vUI���������邽�߂̈ˑ��֌W����������Ă��܂���IAwake�̃��O���m�F���Ă��������B", this);
                return;
            }

            // ������1�ȏ�̃��W���[���݂̂�View�ɓn��
            List<RuntimeModuleData> choiceRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentQuantityValue > 0)
                .ToList();

            _buildView.DisplayBuildModules(choiceRuntimeModules);
            UpdateChoiceButtonsInteractability();
        }
    }
}
