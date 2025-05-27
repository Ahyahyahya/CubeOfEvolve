// App.GameSystem.Modules/RuntimeModuleManager.cs
using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using System.Collections.Generic;
using UnityEngine;
using R3; // R3��using�f�B���N�e�B�u��ǉ�

namespace App.GameSystem.Modules
{
    public class RuntimeModuleManager : MonoBehaviour
    {
        public static RuntimeModuleManager Instance { get; private set; }

        [SerializeField] private ModuleDataStore _moduleDataStore;

        // �S�Ă�RuntimeModuleData���Ǘ����邽�߂�List
        private readonly List<RuntimeModuleData> _allRuntimeModuleDataInternal = new List<RuntimeModuleData>();

        // �R���N�V�����̕ύX��ʒm���邽�߂�Subject (R3)
        private readonly Subject<Unit> _collectionChangedSubject = new Subject<Unit>();

        // �R���N�V�����̗v�f���̂�ǂݎ���p�Ō��J (������AllRuntimeModuleData�v���p�e�B���ƌ^���ێ��ɋ߂��`)
        // ���̃N���X���R���N�V�����̗v�f�ɃA�N�Z�X���邽�߂Ɏg�p
        public IReadOnlyList<RuntimeModuleData> AllRuntimeModuleData => _allRuntimeModuleDataInternal;

        // �R���N�V�����S�̂̕ύX�C�x���g���w�ǂ��邽�߂�Observable (R3)
        // Subject����Observable<Unit>�ɕϊ����Č��J���܂��B
        // ���̃N���X���R���N�V�����̕ύX�����m���邽�߂Ɏg�p
        public Observable<Unit> OnAllRuntimeModuleDataChanged => _collectionChangedSubject.AsObservable();


        // ���W���[��ID���L�[�Ƃ��������ō����A�N�Z�X
        private Dictionary<int, RuntimeModuleData> _runtimeModuleDictionary = new Dictionary<int, RuntimeModuleData>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (_moduleDataStore == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore is not assigned!", this);
            }

            InitializeAllModules();
        }

        // MonoBehaviour��OnDestroy��Subject��Dispose���邱�Ƃ𐄏�
        void OnDestroy()
        {
            _collectionChangedSubject.Dispose();
        }

        /// <summary>
        /// �S�Ẵ}�X�^�[���W���[���f�[�^�����RuntimeModuleData���������i���x��0, ����0�j
        /// �Q�[���J�n���Ɉ�x�����Ă΂�邱�Ƃ�z��
        /// </summary>
        private void InitializeAllModules()
        {
            if (_moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore data is not available for initialization.", this);
                return;
            }

            foreach (var masterData in _moduleDataStore.DataBase.ItemList)
            {
                if (!_runtimeModuleDictionary.ContainsKey(masterData.Id))
                {
                    RuntimeModuleData newRmd = new RuntimeModuleData(masterData);
                    _runtimeModuleDictionary.Add(masterData.Id, newRmd);
                    _allRuntimeModuleDataInternal.Add(newRmd); // ����List�ɒǉ�
                }
            }
            // �S�Ă̗v�f��ǉ����I������Ɉ�x�����ύX��ʒm
            _collectionChangedSubject.OnNext(Unit.Default);
            Debug.Log($"RuntimeModuleManager: Initialized {_allRuntimeModuleDataInternal.Count} modules to level 0, quantity 0.");

            // �f�o�b�O�p: ����̃��W���[�����������x��1�ɐݒ肵�ăV���b�v�ɕ\������邩�e�X�g
            // if (_runtimeModuleDictionary.TryGetValue(1001, out var debugRmd)) // ����ID
            // {
            //     debugRmd.SetLevel(1);
            //     debugRmd.SetQuantity(1);
            //     Debug.Log($"Debug: Module 1001 set to Level 1, Quantity 1.");
            // }
        }

        /// <summary>
        /// �w�肳�ꂽID��RuntimeModuleData���擾���܂��B
        /// </summary>
        public RuntimeModuleData GetRuntimeModuleData(int moduleId)
        {
            _runtimeModuleDictionary.TryGetValue(moduleId, out var rmd);
            return rmd;
        }

        /// <summary>
        /// ���W���[���̐��ʂ�ύX���܂��B
        /// </summary>
        /// <param name="moduleId">�Ώۃ��W���[����ID�B</param>
        /// <param name="amount">�ύX�ʁB</param>
        public void ChangeModuleQuantity(int moduleId, int amount)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))
            {
                rmd.ChangeQuantity(amount); // RuntimeModuleData����ReactiveProperty���X�V
                // �ʂ�RuntimeModuleData���ύX���ꂽ�ꍇ�A�R���N�V�����̕ύX���ʒm
                _collectionChangedSubject.OnNext(Unit.Default);
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: Module with ID {moduleId} not found. Cannot change quantity.", this);
            }
        }

        /// <summary>
        /// ���W���[���̃��x�����グ�܂��B
        /// </summary>
        /// <param name="moduleId">�Ώۃ��W���[����ID�B</param>
        public void LevelUpModule(int moduleId)
        {
            if (_runtimeModuleDictionary.TryGetValue(moduleId, out RuntimeModuleData rmd))
            {
                rmd.LevelUp(); // RuntimeModuleData����ReactiveProperty���X�V
                // �ʂ�RuntimeModuleData���ύX���ꂽ�ꍇ�A�R���N�V�����̕ύX���ʒm
                _collectionChangedSubject.OnNext(Unit.Default);
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleManager: Module with ID {moduleId} not found. Cannot level up.", this);
            }
        }
    }
}