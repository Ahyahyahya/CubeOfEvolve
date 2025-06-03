using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using MVRP.AT.View;
using R3;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.AT.Presenter
{
    /// <summary>
    /// �h���b�v�I����ʂ̃v���[���^�[��S������N���X�B
    /// View����̃C�x���g��R3�ōw�ǂ��AModel�iRuntimeModuleManager, ModuleDataStore�j�𑀍삵�A
    /// View�ɕ\���f�[�^��n���܂��B
    /// </summary>
    public class Drop_Presenter : MonoBehaviour
    {
        // ----- SerializedField
        // -----Models
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // �����^�C�����W���[���f�[�^���Ǘ�����}�l�[�W���[�B
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���}�X�^�[�f�[�^���Ǘ�����f�[�^�X�g�A�B
        // -----Views
        [SerializeField] private Drop_View _dropView; // �h���b�vUI��\������View�R���|�[�l���g�B
        [SerializeField] private TextMeshProUGUI _hoveredModuleInfoText; // ������

        // ----- Private Members (�����f�[�^)
        private const int NUMBER_OF_OPTIONS = 3; // �񎦂��郂�W���[���̐��B
        private List<int> _candidateModuleIds = new List<int>();

        // ----- UnityMessage
        
        private void Awake()
        {
            // �ˑ��֌W�����ݒ�̏ꍇ�A�V�[������擾�����݂�
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;

            // View����̃C�x���g��R3�ōw��
            if (_dropView != null)
            {
                _dropView.OnModuleSelected
                    .Subscribe(x => HandleModuleSelected(x))
                    .AddTo(this); // R3 �� AddTo(CompositeDisposable) ���g�p�B
                _dropView.OnModuleHovered
                    .Subscribe(x => HandleModuleHovered(x))
                    .AddTo(this);
            }
            else
            {
                Debug.LogError("Drop_Presenter: Drop_View���ݒ肳��Ă��܂���B�h���b�v�I���C�x���g���w�ǂł��܂���B", this);
            }

            // �K�{�̈ˑ��֌W�������Ă��邩�`�F�b�N
            if (_runtimeModuleManager == null || _moduleDataStore == null)
            {
                Debug.LogError("Drop_Presenter: RuntimeModuleManager�܂���ModuleDataStore���ݒ肳��Ă��܂���B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
                enabled = false;
            }
        }

        // -----Public
        /// <summary>
        /// �h���b�v�I��UI��\�����鏀�������AView�ɕ\�����˗����܂��B
        /// ���̃��\�b�h�́A�Ⴆ�΃v���C���[������̃A�C�e�����E�����ۂ�GameManager�Ȃǂ���Ăяo����܂��B
        /// </summary>
        public void PrepareAndShowDropUI()
        {
            // �ˑ�NullCheck
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("Drop_Presenter: �v���[���^�[�̈ˑ��֌W����������Ă��܂���I�h���b�vUI��\���ł��܂���B", this);
                return;
            }

            // 1. �񎦂��郂�W���[����I�����郍�W�b�N
            _candidateModuleIds = GetRandomAvailableModuleIds(NUMBER_OF_OPTIONS);

            // 2. View�ɓn�����߂̃f�[�^����
            List<(ModuleData master, RuntimeModuleData runtime)> displayDatas = new List<(ModuleData, RuntimeModuleData)>();

            if (_candidateModuleIds.Count == 0)
            {
                // �I�������Ȃ��ꍇ�B�S���̃��W���[�������x��5�̏ꍇ
                Debug.Log("Drop_Presenter: �S���̃��W���[�������x��5�ɂȂ���������݂���");
            }
            else
            {
                foreach (int moduleId in _candidateModuleIds)
                {
                    RuntimeModuleData runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
                    ModuleData master = _moduleDataStore.FindWithId(moduleId);

                    if (runtime != null && master != null)
                    {
                        displayDatas.Add((master, runtime));
                    }
                    else
                    {
                        Debug.LogWarning($"Drop_Presenter: �f�[�^�s����: ���Ƀ��W���[��ID {moduleId} ��������܂������A�}�X�^�[�f�[�^�܂��̓����^�C���f�[�^���s�����Ă��܂��B");
                    }
                }
            }

            // 3. View�ɕ\�����˗�
            _dropView.UpdateModuleView(displayDatas);
        }

        // -----Private
        /// <summary>
        /// ���[�U�[�����W���[����I�������ۂ̃C�x���g�n���h���B
        /// View����̃C�x���g�iR3�ōw�ǁj�ɂ���ČĂяo����܂��B
        /// </summary>
        /// <param name="selectedModuleId">�I�����ꂽ���W���[����ID�B</param>
        private void HandleModuleSelected(int selectedModuleId)
        {
            if (selectedModuleId == -1) // ���ł��Ȃ����̂�I�������ꍇ
            {
                Debug.Log("Drop_Presenter: ��փI�v�V�������I������܂����B");
                // �����Ōo���l�l����R�C���l���Ȃǂ̃��W�b�N���Ăяo��
                // ��: GameManager.Instance.GainExperience(100);
            }
            else
            {
                Debug.Log($"Drop_Presenter: ���[�U�[�ɂ���ă��W���[��ID {selectedModuleId} ���I������܂����B");

                // RuntimeModuleManager ����ă��W���[���̃��x���A�b�v���������s
                _runtimeModuleManager.LevelUpModule(selectedModuleId);
            }

            // �K�v�ł���΁AUI�̍X�V�ȂǁA�Q�[���S�̂̏�Ԃɉ������㏈�����Ăяo��
            // ��: GameManager.Instance.OnPlayerModuleUpgraded();
        }

        private void HandleModuleHovered(int EnterModuleId)
        {
            _hoveredModuleInfoText.text = _moduleDataStore.FindWithId(EnterModuleId).Description;
        }

        /// <summary>
        /// �����_���ɃA�b�v�O���[�h�\�ȃ��W���[��ID��I�����郍�W�b�N�B
        /// </summary>
        /// <param name="count">�I�o���郂�W���[���̐��B</param>
        /// <returns>�I�o���ꂽ���W���[����ID���X�g�B</returns>
        private List<int> GetRandomAvailableModuleIds(int count)
        {
            List<int> upgradeableModuleIds = new List<int>();

            // ���݃v���C���[���������Ă��郂�W���[���̒�����A
            // �܂����x������ɒB���Ă��Ȃ����W���[���𒊏o���郍�W�b�N
            foreach (var runtimeModule in _runtimeModuleManager.AllRuntimeModuleData)
            {
                // ���x����5�����̃��W���[�����A�b�v�O���[�h�\�Ƃ���
                if (runtimeModule.CurrentLevelValue < 5)
                {
                    upgradeableModuleIds.Add(runtimeModule.Id);
                }
            }

            // �I�o���W�b�N
            List<int> selectedIds = new List<int>();
            if (upgradeableModuleIds.Count == 0)
            {
                return selectedIds; // �A�b�v�O���[�h�\�ȃ��W���[�����Ȃ��ꍇ�B
            }

            // �d���Ȃ��Ń����_���ɑI��
            HashSet<int> uniqueIndices = new HashSet<int>();
            while (uniqueIndices.Count < count && uniqueIndices.Count < upgradeableModuleIds.Count)
            {
                int randomIndex = Random.Range(0, upgradeableModuleIds.Count);
                uniqueIndices.Add(upgradeableModuleIds[randomIndex]);
            }

            selectedIds.AddRange(uniqueIndices); // HashSet���烊�X�g�ɕϊ��B

            return selectedIds;
        }
    }
}