using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using R3;
using System.Collections.Generic;
using UnityEngine;


public class Drop_Presenter : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Drop_View _dropView; // View�ւ̎Q��
    [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // Model�ւ̎Q��
    [SerializeField] private ModuleDataStore _moduleDataStore; // Model�ւ̎Q�� (�}�X�^�[�f�[�^)

    private const int NUMBER_OF_OPTIONS = 3; // �񎦂��郂�W���[���̐�



    void Awake()
    {
        // �ˑ��֌W�����ݒ�̏ꍇ�A�V�[������擾�����݂�
        if (_dropView == null) _dropView = FindObjectOfType<Drop_View>();
        if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
        // _moduleDataStore �� GameManager �Ȃǂ���n����邩�A���ڎQ��

        // View����̃C�x���g��R3�ōw��
        if (_dropView != null)
        {
            // R3 �� Subscribe ���\�b�h�́A���������ꍇ�̓����_���œn���܂�
            _dropView.OnModuleSelected
                .Subscribe(selectedModuleId => HandleModuleSelected(selectedModuleId)) // ���������C����
                .AddTo(this); // R3 �� AddTo(CompositeDisposable) ���g�p
        }
    }

    /// <summary>
    /// �h���b�v�I��UI��\�����鏀�������AView�ɕ\�����˗����܂��B
    /// ���̃��\�b�h�́A�Ⴆ�΃v���C���[������̃A�C�e�����E�����ۂ�GameManager�Ȃǂ���Ăяo����܂��B
    /// </summary>
    public void PrepareAndShowDropUI()
    {
        if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
        {
            Debug.LogError("Presenter dependencies not met! Cannot show drop UI.");
            return;
        }

        // 1. �񎦂��郂�W���[����I�����郍�W�b�N
        List<int> candidateModuleIds = GetRandomAvailableModuleIds(NUMBER_OF_OPTIONS);

        // 2. View�ɓn�����߂̃f�[�^����
        List<(ModuleData master, RuntimeModuleData runtime)> displayDatas = new List<(ModuleData, RuntimeModuleData)>();
        bool showDefaultOption = false; // ��փI�v�V������\�����邩�ǂ���

        if (candidateModuleIds.Count == 0)
        {
            // �I�������Ȃ��ꍇ�A��փI�v�V������\��
            showDefaultOption = true;
            Debug.Log("No upgradeable modules found. Displaying default option.");
        }
        else
        {
            foreach (int moduleId in candidateModuleIds)
            {
                RuntimeModuleData runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
                ModuleData master = _moduleDataStore.FindWithId(moduleId);

                if (runtime != null && master != null)
                {
                    displayDatas.Add((master, runtime));
                }
                else
                {
                    Debug.LogWarning($"Data inconsistency: Module ID {moduleId} found in candidates but master/runtime data missing.");
                }
            }
        }

        // 3. View�ɕ\�����˗�
        _dropView.Show(displayDatas, showDefaultOption);
    }


    /// <summary>
    /// ���[�U�[�����W���[����I�������ۂ̃C�x���g�n���h���B
    /// View����̃C�x���g�iR3�ōw�ǁj�ɂ���ČĂяo����܂��B
    /// </summary>
    /// <param name="selectedModuleId">�I�����ꂽ���W���[����ID�B</param>
    private void HandleModuleSelected(int selectedModuleId)
    {
        if (selectedModuleId == -1) // ��փI�v�V�������I�����ꂽ�ꍇ
        {
            Debug.Log("Default option was handled.");
            // �����Ōo���l�l����R�C���l���Ȃǂ̃��W�b�N���Ăяo��
            // ��: GameManager.Instance.GainExperience(100);
        }
        else
        {
            Debug.Log($"Module with ID {selectedModuleId} was selected by user.");

            // RuntimeModuleManager ����ă��W���[���̃��x���A�b�v���������s
            _runtimeModuleManager.LevelUpModule(selectedModuleId);
        }

        // �K�v�ł���΁AUI�̍X�V�ȂǁA�Q�[���S�̂̏�Ԃɉ������㏈�����Ăяo��
        // ��: GameManager.Instance.OnPlayerModuleUpgraded();
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
            ModuleData masterData = _moduleDataStore.FindWithId(runtimeModule.Id);
            // �����Ń}�X�^�[�f�[�^�� maxLevel �̂悤�ȏ�񂪂���O��
            // if (runtimeModule.CurrentLevel < masterData.MaxLevel) {
            //     upgradeableModuleIds.Add(runtimeModule.Id);
            // }
            // ���ɁA���x����5�����̃��W���[�����A�b�v�O���[�h�\�Ƃ���
            if (runtimeModule.CurrentLevelValue < 5)
            {
                upgradeableModuleIds.Add(runtimeModule.Id);
            }
        }

        // �I�o���W�b�N
        List<int> selectedIds = new List<int>();
        if (upgradeableModuleIds.Count == 0)
        {
            return selectedIds; // �A�b�v�O���[�h�\�ȃ��W���[�����Ȃ��ꍇ
        }

        // �d���Ȃ��Ń����_���ɑI��
        HashSet<int> uniqueIndices = new HashSet<int>();
        while (uniqueIndices.Count < count && uniqueIndices.Count < upgradeableModuleIds.Count)
        {
            int randomIndex = Random.Range(0, upgradeableModuleIds.Count);
            uniqueIndices.Add(upgradeableModuleIds[randomIndex]);
        }

        selectedIds.AddRange(uniqueIndices); // HashSet���烊�X�g�ɕϊ�

        return selectedIds;
    }
}
