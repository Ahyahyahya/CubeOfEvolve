using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Handler;
using Assets.IGC2025.Scripts.GameManagers;
using ObservableCollections;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace App.GameSystem.Modules
{
    /// <summary>
    /// �Q�[�����s���̃��W���[���f�[�^���ꌳ�I�ɊǗ�����}�l�[�W���[�B
    /// �V���O���g���p�^�[���Ŏ�������Ă���A�e�탂�W���[���̏�ԕω����Ď��E���삵�܂��B
    /// </summary>
    [RequireComponent(typeof(SceneClassReferenceHandler))]
    public class RuntimeModuleManager : MonoBehaviour
    {
        // ----- Singleton
        public static RuntimeModuleManager Instance { get; private set; }

        // ----- SerializedField
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���}�X�^�[�f�[�^���i�[����f�[�^�X�g�A�B

        // ----- Private Members (�����f�[�^)
        private readonly List<RuntimeModuleData> _allRuntimeModuleDataInternal = new List<RuntimeModuleData>(); // �S�Ă�RuntimeModuleData���Ǘ�����������X�g�B
        private readonly Subject<Unit> _collectionChangedSubject = new Subject<Unit>(); // ���W���[���R���N�V�����̕ύX��ʒm���邽�߂�Subject�B
        private Dictionary<int, RuntimeModuleData> _runtimeModuleDictionary = new Dictionary<int, RuntimeModuleData>(); // ���W���[��ID���L�[�Ƃ���RuntimeModuleData�̍����A�N�Z�X�p�����B

        private ObservableList<StatusEffectData> _currentStatusEffectList = new(); // �o�t�E�f�o�t���܂Ƃ߂�

        private SceneClassReferenceHandler Handler;
        // ----- Public Properties (���J�v���p�e�B)
        public IReadOnlyList<RuntimeModuleData> AllRuntimeModuleData => _allRuntimeModuleDataInternal;
        public Observable<Unit> OnAllRuntimeModuleDataChanged => _collectionChangedSubject.AsObservable();// �ύX���������Ƃ��̖ڈ�
        public IReadOnlyObservableList<StatusEffectData> CurrentCurrentStatusEffectList => _currentStatusEffectList;

        // ----- UnityMessage
        private void Awake()
        {
            // �V���O���g��������
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            // �Q��NullCheck
            if (_moduleDataStore == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore���ݒ肳��Ă��܂���I", this);
            }

            Handler = GetComponent<SceneClassReferenceHandler>();

            // ������
            InitializeAllModules();
        }

        private void OnDestroy()
        {
            _collectionChangedSubject.Dispose(); // ���\�[�X�J��
        }

        // ----- Private Methods (�v���C�x�[�g���\�b�h)
        /// <summary>
        /// �S�Ẵ}�X�^�[���W���[���f�[�^�����RuntimeModuleData�����������܂��B
        /// �Q�[���J�n���Ɉ�x�����Ă΂�邱�Ƃ�z�肵�Ă��܂��B
        /// </summary>
        private void InitializeAllModules()
        {
            // �Q��NullCheck
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("RuntimeModuleManager: ���W���[���̏������ɕK�v��ModuleDataStore�f�[�^�����p�ł��܂���B", this);
                return;
            }

            foreach (var masterData in _moduleDataStore.DataBase.ItemList)
            {
                if (!_runtimeModuleDictionary.ContainsKey(masterData.Id))
                {
                    RuntimeModuleData newRmd = new RuntimeModuleData(masterData);
                    _runtimeModuleDictionary.Add(masterData.Id, newRmd);
                    _allRuntimeModuleDataInternal.Add(newRmd); // �������X�g�ɒǉ��B
                }
            }

            // �S�Ă̗v�f��ǉ����I������Ɉ�x�����ύX��ʒm�B
            _collectionChangedSubject.OnNext(Unit.Default);
            //debug.log($"RuntimeModuleManager: {_allRuntimeModuleDataInternal.Count}�̃��W���[�������������܂����B");
        }

        // ----- Public Methods (���J���\�b�h)
        /// <summary>
        /// �w�肳�ꂽID��RuntimeModuleData���擾���܂��B
        /// </summary>
        /// <param name="moduleId">�擾���郂�W���[����ID�B</param>
        /// <returns>�w�肳�ꂽID��RuntimeModuleData�B������Ȃ��ꍇ��null�B</returns>
        public RuntimeModuleData GetRuntimeModuleData(int moduleId)
        {
            _runtimeModuleDictionary.TryGetValue(moduleId, out var rmd);
            return rmd;
        }

        /// <summary>
        /// ���W���[���̐��ʂ�ύX���܂��B
        /// </summary>
        /// <param name="moduleId">�Ώۃ��W���[����ID�B</param>
        /// <param name="amount">���ʂ̕ύX�ʁB</param>
        public void ChangeModuleQuantity(int moduleId, int amount)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))
            {
                rmd.ChangeQuantity(amount); // RuntimeModuleData����ReactiveProperty���X�V�B
                // �ʂ�RuntimeModuleData���ύX���ꂽ�ꍇ�A�R���N�V�����̕ύX���ʒm�B
                _collectionChangedSubject.OnNext(Unit.Default);
                //debug.log($"RuntimeModuleManager: ���W���[��ID {moduleId} �̐��ʂ� {amount} �ύX���܂����B���݂̐���: {rmd.CurrentQuantityValue}");
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: ID {moduleId} �̃��W���[����������܂���B���ʂ�ύX�ł��܂���B", this);
            }
        }

        /// <summary>
        /// �I�v�V������ǉ�
        /// </summary>
        public void AddOption(StatusEffectData optionData)
        {
            _currentStatusEffectList.Add(optionData);
        }

        /// <summary>
        /// �I�v�V���������炷
        /// </summary>
        public void RemoveOption(StatusEffectData optionData)
        {
            if (_currentStatusEffectList.Contains(optionData))
            {
                _currentStatusEffectList.Remove(optionData);
            }
        }

        /// <summary>
        /// ���W���[���̃��x�����グ��֐��B
        /// </summary>
        /// <param name="moduleId">�Ώۃ��W���[����ID�B</param>
        public void LevelUpModule(int moduleId)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))// �����A�N�Z�X
            {
                rmd.LevelUp(); // RuntimeModuleData����ReactiveProperty���X�V�B
                // �ʂ�RuntimeModuleData���ύX���ꂽ�ꍇ�A�R���N�V�����̕ύX���ʒm�B
                _collectionChangedSubject.OnNext(Unit.Default);
                //debug.log($"RuntimeModuleManager: ���W���[��ID {moduleId} �̃��x�����グ�܂����B���݂̃��x��: {rmd.CurrentLevelValue}");
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: ID {moduleId} �̃��W���[����������܂���B���x���A�b�v�ł��܂���B", this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numberOfOptions"></param>
        /// <param name="currentGameState"></param>
        /// <returns></returns>
        public List<int> GetDisplayModuleIds(int numberOfOptions, GameState currentGameState)
        {
            var candidatePool = new List<RuntimeModuleData>();
            foreach (var runtime in AllRuntimeModuleData)
            {
                if (runtime.CurrentLevelValue < 5)
                    candidatePool.Add(runtime);
            }

            if (candidatePool.Count == 0)
                return new List<int>(); // �S���W���[�����ő僌�x��

            if (currentGameState == GameState.TUTORIAL)
                return new List<int> { 0, 1, 2 };

            if (candidatePool.Count <= numberOfOptions)
                return candidatePool.Select(m => m.Id).ToList();

            // �����_�����
            HashSet<int> selected = new();
            while (selected.Count < numberOfOptions)
            {
                int randIndex = Random.Range(0, candidatePool.Count);
                selected.Add(candidatePool[randIndex].Id);
            }

            return selected.ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        public void TriggerDropUI()
        {
            Handler.PresenterDropCanvas.PrepareAndShowDropUI();
        }
    }
}