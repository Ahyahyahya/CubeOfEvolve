// RuntimeModuleManager.cs
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // LINQ ���\�b�h (Select, ToDictionary �Ȃ�) �̂��߂ɕK�v
using App.BaseSystem.DataStores.ScriptableObjects.Modules; // ModuleDataStore �̖��O���

namespace App.GameSystem.Modules
{
    /// <summary>
    /// �v���C���[�����L���郂�W���[���̃����^�C���f�[�^���Ǘ�����N���X�B
    /// �������A�f�[�^�擾�A����A�Z�[�u/���[�h�f�[�^�̐����E�K�p��S������B
    /// </summary>
    public class RuntimeModuleManager : MonoBehaviour
    {
        // ------------------ �ˑ��֌W
        // ModuleDataStore �ւ̎Q�Ɓi�}�X�^�[�f�[�^���擾���邽�߁j
        [SerializeField] private ModuleDataStore _moduleDataStore;

        // ------------------ �����^�C���f�[�^�̕ێ�
        // ���W���[��ID���L�[�ɁARuntimeModuleData �C���X�^���X���Ǘ�
        private Dictionary<int, RuntimeModuleData> _runtimeModules = new Dictionary<int, RuntimeModuleData>();

        // ------------------ �V���O���g���p�^�[���i��j
        // GameManager�̂悤�ȏ�ʃN���X��������Ǘ�����ꍇ�A�V���O���g���͕s�v�Ȃ��Ƃ�����܂�
        public static RuntimeModuleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Initialize();
                Instance = this;
                // �V�[���J�ڂ��Ă��f�[�^��ێ��������ꍇ�̓R�����g����
                //DontDestroyOnLoad(gameObject);
            }
        }
        // ------------------ �V���O���g���p�^�[���I���

        // ------------------ ������
       
        /// �����^�C�����W���[���f�[�^�����������܂��B
        /// ModuleDataStore�Ɋ܂܂��S�Ẵ��W���[����������ԂŃv���C���[�����悤�ɐݒ肵�܂��B
        /// �ʏ�A�Q�[���J�n����V�[�����[�h���ɌĂяo����܂��B
        /// </summary>
        public void Initialize()
        {
            if (_moduleDataStore == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore is not assigned in Inspector! Cannot initialize modules.");
                return;
            }
            if (_moduleDataStore.DaraBase == null)
            {
                Debug.LogError("RuntimeModuleManager: ModuleDataStore.DaraBase is NULL! Master data is not loaded. Cannot initialize modules.");
                return;
            }
            if (_moduleDataStore.DaraBase.ItemList == null || _moduleDataStore.DaraBase.ItemList.Count == 0)
            {
                Debug.LogWarning("RuntimeModuleManager: ModuleDataStore.DaraBase.ItemList is EMPTY. No master ModuleData to initialize with.");
                return;
            }

            _runtimeModules.Clear(); // �����̃����^�C���f�[�^���N���A

            // (�ύX�_: Store����S�Ẵ��W���[�����擾���A�����^�C���f�[�^�Ƃ��Ēǉ�)
            foreach (ModuleData masterModuleData in _moduleDataStore.DaraBase.ItemList)
            {
                if (masterModuleData != null)
                {
                    RuntimeModuleData runtimeModule = new RuntimeModuleData(masterModuleData);
                    // ���ɓ���ID�����݂��Ȃ����`�F�b�N (�G���[�h�~�A�ʏ�͔������Ȃ��͂������O�̂���)
                    if (_runtimeModules.ContainsKey(runtimeModule.Id))
                    {
                        Debug.LogWarning($"RuntimeModuleManager: Duplicate module ID {runtimeModule.Id} found in ModuleDataStore.DaraBase.ItemList. Skipping duplicate.");
                    }
                    else
                    {
                        _runtimeModules.Add(runtimeModule.Id, runtimeModule);
                    }
                }
                else
                {
                    Debug.LogWarning("RuntimeModuleManager: Null ModuleData found in ModuleDataStore.DaraBase.ItemList. Skipping.");
                }
            }

            Debug.Log($"RuntimeModuleManager Initialized. Managing {_runtimeModules.Count} player runtime modules, taken from ModuleDataStore.");
        }

        // ------------------ �����^�C���f�[�^�ւ̃A�N�Z�X
        /// <summary>
        /// �w�肳�ꂽID�̃����^�C�����W���[���f�[�^���擾���܂��B
        /// </summary>
        /// <param name="id">�擾���������W���[����ID�B</param>
        /// <returns>�Ή�����RuntimeModuleData�C���X�^���X�B������Ȃ��ꍇ��null�B</returns>
        public RuntimeModuleData GetRuntimeModuleData(int id)
        {
            _runtimeModules.TryGetValue(id, out RuntimeModuleData module);
            return module;
        }

        /// <summary>
        /// �S�Ẵ����^�C�����W���[���f�[�^��ǂݎ���p�̃R���N�V�����Ƃ��Ď擾���܂��B
        /// </summary>
        public IReadOnlyCollection<RuntimeModuleData> AllRuntimeModuleData => _runtimeModules.Values;

        // ------------------ �����^�C���f�[�^�̑���i�v���C���[���W���[�����L�̃r�W�l�X���W�b�N�j
        /// <summary>
        /// �w�肳�ꂽ���W���[���̃��x����1�グ�܂��B
        /// </summary>
        /// <param name="id">���x���A�b�v�����郂�W���[����ID�B</param>
        public void LevelUpModule(int id)
        {
            RuntimeModuleData module = GetRuntimeModuleData(id);
            if (module != null)
            {
                module.CurrentLevel++; // �����^�C���f�[�^���X�V

                // �}�X�^�[�f�[�^���Q�Ƃ��āA���x���A�b�v��̉e�����v�Z����Ȃ�
                ModuleData masterData = _moduleDataStore.FindWithId(id);
                string moduleName = masterData?.Name ?? "Unknown Module";
                Debug.Log($"Player's Module {moduleName} (ID:{id}) level up to {module.CurrentLevel}!");
            }
            else
            {
                Debug.LogWarning($"Attempted to level up non-existent player module with ID: {id}");
            }
        }

        /// <summary>
        /// �w�肳�ꂽ���W���[���̏�������ύX���܂��B
        /// </summary>
        /// <param name="id">��������ύX���郂�W���[����ID�B</param>
        /// <param name="changeAmount">�ύX�ʁi���Z�܂��͌��Z�j�B</param>
        public void ChangeModuleQuantity(int id, int changeAmount)
        {
            RuntimeModuleData module = GetRuntimeModuleData(id);
            if (module != null)
            {
                module.Quantity += changeAmount;
                if (module.Quantity < 0) module.Quantity = 0; // �����������ɂȂ�Ȃ��悤��

                ModuleData masterData = _moduleDataStore.FindWithId(id);
                string moduleName = masterData?.Name ?? "Unknown Module";
                Debug.Log($"Player's Module {moduleName} (ID:{id}) quantity changed by {changeAmount}. Current: {module.Quantity}");

                if (module.Quantity == 0)
                {
                    Debug.Log($"Player's Module {moduleName} (ID:{id}) ran out.");
                    // �K�v�ł���΁A�����Ń��W���[�������S�ɍ폜���郍�W�b�N���Ăяo��
                    // RemoveModule(id);
                }
            }
            else
            {
                Debug.LogWarning($"Attempted to change quantity of non-existent player module with ID: {id}");
            }
        }

        // ���W���[�������S�ɍ폜����� (��������0�ɂȂ����ꍇ�Ȃ�)
        public void RemoveModule(int id)
        {
            if (_runtimeModules.Remove(id))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(id);
                string moduleName = masterData?.Name ?? "Unknown Module";
                Debug.Log($"Player's Module {moduleName} (ID:{id}) has been removed.");
            }
            else
            {
                Debug.LogWarning($"Attempted to remove non-existent player module with ID: {id}");
            }
        }


        

        // ------------------ �Z�[�u/���[�h�����̂��߂̃f�[�^��/�K�p
        /// <summary>
        /// ���݂̑S�Ẵv���C���[���W���[���f�[�^���Z�[�u�f�[�^�`���ɕϊ����A�񋟂��܂��B
        /// </summary>
        /// <returns>�ۑ����ׂ����W���[���̏�Ԃ̃��X�g�B</returns>
        public List<RuntimeModuleData.ModuleSaveState> GeneratePlayerModuleSaveData()
        {
            return _runtimeModules.Values
                .Select(m => new RuntimeModuleData.ModuleSaveState
                {
                    id = m.Id,
                    level = m.CurrentLevel,
                    quantity = m.Quantity // Quantity ���Z�[�u�Ώۂɒǉ�
                })
                .ToList();
        }

        /// <summary>
        /// ���[�h���ꂽ�Z�[�u�f�[�^���󂯎��A�v���C���[���W���[���f�[�^���X�V���܂��B
        /// </summary>
        /// <param name="savedStates">���[�h���ꂽ���W���[���̃Z�[�u�f�[�^�̃��X�g�B</param>
        public void ApplyPlayerModuleSaveData(List<RuntimeModuleData.ModuleSaveState> savedStates)
        {
            // �܂��͊����̃����^�C���f�[�^���N���A���A�Z�[�u�f�[�^�ŏ㏑�����鏀��
            _runtimeModules.Clear();

            // ���[�h���ꂽ�Z�[�u�f�[�^�̓��e�ŁA�Ή����郉���^�C�����W���[���𐶐��E�X�V
            foreach (var state in savedStates)
            {
                // �Z�[�u�f�[�^���畜������ہA�}�X�^�[�f�[�^�����݂��邩�m�F
                ModuleData masterModuleData = _moduleDataStore.FindWithId(state.id);
                if (masterModuleData != null)
                {
                    // �Z�[�u�f�[�^����RuntimeModuleData�𐶐�
                    RuntimeModuleData runtimeModule = new RuntimeModuleData(state);
                    // ������Name�v���p�e�B���}�X�^�[�f�[�^����ݒ�
                    runtimeModule.Name = masterModuleData.Name;
                    _runtimeModules.Add(runtimeModule.Id, runtimeModule);
                }
                else
                {
                    // �Z�[�u�f�[�^�ɑ��݂��邪�A�}�X�^�[�f�[�^�ɂ͑��݂��Ȃ����W���[���̏ꍇ
                    // �i��: �Q�[���X�V�ō폜���ꂽ���W���[���Ȃǁj
                    Debug.LogWarning($"Saved data for player module ID {state.id} found but corresponding master module data not found. Skipping.");
                }
            }
            Debug.Log($"Applied save data to {_runtimeModules.Count} player runtime modules.");
        }
    }
}