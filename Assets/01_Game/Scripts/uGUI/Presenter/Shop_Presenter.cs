// App.GameSystem.Presenters/Shop_Presenter.cs
using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using App.GameSystem.UI;
using R3; // R3��using�f�B���N�e�B�u
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace App.GameSystem.Presenters
{
    /// <summary>
    /// �V���b�v��ʂ̃v���[���^�[��S������N���X�B
    /// View����̃C�x���g��R3�ōw�ǂ��AModel�iRuntimeModuleManager, PlayerCore�j�𑀍삵�A
    /// View�ɕ\���f�[�^��n���B
    /// �܂��ARuntimeModule�̃��x���ύX���Ď����A�V���b�v�\�����X�V����B
    /// </summary>
    public class Shop_Presenter : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private Shop_View _shopView;
        [SerializeField] private ModuleDataStore _moduleDataStore;
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private PlayerCore _playerCore;

        private CompositeDisposable _disposables = new CompositeDisposable();
        // �e���W���[���̃��x���E���ʕύX�w�Ǘp�B�R���N�V�����ύX���ɃN���A���邽��CompositeDisposable���g�p
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable();

        void Awake()
        {
            // �ˑ��֌W�̎擾�ƃ`�F�b�N��Awake�̑����i�K�ōs��
            if (_shopView == null) _shopView = FindObjectOfType<Shop_View>();
            if (_moduleDataStore == null) Debug.LogError("Shop_Presenter: ModuleDataStore is not assigned in Inspector!", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            if (_playerCore == null) _playerCore = FindObjectOfType<PlayerCore>();

            // �e�ˑ��֌W�������Ă��邩�ŏI�`�F�b�N
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: One or more dependencies are missing. Please check Inspector assignments and scene setup. Disabling this component.", this);
                enabled = false;
                return;
            }

            // View����̃��W���[���w�����N�G�X�g���w��
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            // PlayerCore �� Money ���ύX���ꂽ��UI���X�V����w��
            if (_playerCore.Money != null)
            {
                _playerCore.Money.Subscribe(money => UpdateShopUIOnCoinChange(money)).AddTo(_disposables);
            }
            else
            {
                Debug.LogError("Shop_Presenter: PlayerCore.Money ReactiveProperty is null. Cannot subscribe to money changes.", this);
            }

            // RuntimeModuleManager���Ǘ����郂�W���[���R���N�V�����S�̂̕ύX���Ď����A�V���b�vUI���X�V����
            // ���W���[���̒ǉ��A�폜�A�܂��͊������W���[���̃��x���E���ʕύX��RuntimeModuleManager����ʒm���ꂽ�ꍇ�ɔ���
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ => {
                    Debug.Log("RuntimeModuleData collection changed. Re-subscribing to module changes and updating shop UI.");
                    // �����̃��W���[�����x���E���ʕύX�w�ǂ�S�ĉ���
                    _moduleLevelAndQuantityChangeDisposables.Clear();

                    // ���݂̑S�Ẵ��W���[���ɑ΂��ă��x���E���ʕύX���w��
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd);
                    }
                    PrepareAndShowShopUI(); // �V���b�v���ĕ\�����ă��X�g���X�V
                })
                .AddTo(_disposables);

            // �����\���̂��߂ɁA�R���N�V���������������ꂽ��Ɉ�xPrepareAndShowShopUI���Ăяo��
            // OnAllRuntimeModuleDataChanged��Awake��ɔ��΂��邽�߁A���̍s�͕s�v�ȏꍇ������܂����A
            // �m���ɏ����\�����s�����߂Ɏc���Ă����܂��B
            PrepareAndShowShopUI();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // �e���W���[���̃��x���E���ʕύX�w�ǂ�����
        }

        /// <summary>
        /// �eRuntimeModuleData�̃��x���Ɛ��ʕύX���w�ǂ���w���p�[���\�b�h
        /// </summary>
        /// <param name="runtimeModuleData"></param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            // Level�̕ύX���w��
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level => {
                        Debug.Log($"Module {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) level changed to {level}. Updating shop UI.");
                        PrepareAndShowShopUI(); // ���x�����ύX���ꂽ��V���b�v���ĕ\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} does not expose its Level as a ReactiveProperty.", this);
            }

            // Quantity�̕ύX���w�� (�������ʂ̕ω��Ń{�^���̏�Ԃ�ς������ꍇ)
            if (runtimeModuleData.Quantity != null)
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity => {
                        Debug.Log($"Module {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) quantity changed to {quantity}. Updating purchase button interactability.");
                        // ���ʕύX�����Ȃ�V���b�v���X�g�S�̂ł͂Ȃ��A�{�^���̃C���^���N�g���̂ݍX�V
                        UpdatePurchaseButtonsInteractability();
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} does not expose its Quantity as a ReactiveProperty.", this);
            }
        }

        /// <summary>
        /// �V���b�v��ʂ�\�����鏀�������AView�ɕ\�����˗����܂��B
        /// ���̃��\�b�h�͊O������Ăяo����܂��i��: GameManager��UIController�j�B
        /// �܂��ARuntimeModuleData�̕ύX�ɂ���Ă������I�ɌĂяo����邱�Ƃ�����܂��B
        /// </summary>
        public void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter dependencies not met! Cannot prepare shop UI. Check Awake logs.", this);
                return;
            }

            // ���ύX�_: ���x��1�ȏ�̃��W���[���݂̂�View�ɓn����
            // AllRuntimeModuleData��IReadOnlyList<RuntimeModuleData>�^�ɂȂ��Ă���
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0) // CurrentLevelValue���g�p
                .ToList();

            _shopView.DisplayShopModules(shopRuntimeModules);
            _shopView.UpdatePlayerCoins(_playerCore.Money.CurrentValue);
            UpdatePurchaseButtonsInteractability();
        }

        /// <summary>
        /// �v���C���[�̃R�C���ύX���ɃV���b�vUI���X�V����B
        /// </summary>
        /// <param name="newCoins">�V�����R�C���ʁB</param>
        private void UpdateShopUIOnCoinChange(int newCoins)
        {
            if (_shopView == null) return;
            _shopView.UpdatePlayerCoins(newCoins);
            UpdatePurchaseButtonsInteractability(); // �R�C���ʂɉ����ă{�^���̗L��/������؂�ւ���
        }

        /// <summary>
        /// �e���W���[���̍w���{�^���̃C���^���N�g�\��Ԃ��X�V���܂��B
        /// </summary>
        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore == null || _moduleDataStore.DataBase == null || _moduleDataStore.DataBase.ItemList == null)
            {
                Debug.LogError("Shop_Presenter: Required data for updating purchase button interactability is missing.", this);
                return;
            }

            // �V���b�v�ɕ\������Ă��邷�ׂẴ��W���[���i���x��1�ȏ�̂��́j�ɂ��ă`�F�b�N
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                                                            .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                bool canAfford = _playerCore.Money.CurrentValue >= masterData.BasePrice;

                // ���x����1�ȏ�ŃV���b�v�ɕ\������Ă��郂�W���[���́A�������������΍w���\
                // ������w���ł��邽�߁A��ɃC���^���N�g�\�Ƃ���i����������������j�B
                _shopView.SetPurchaseButtonInteractable(runtimeData.Id, canAfford);
            }
        }

        /// <summary>
        /// ���W���[���w�����N�G�X�g���󂯎�����ۂ̃n���h���B
        /// </summary>
        /// <param name="moduleId">�w�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Shop_Presenter: Master data for module ID {moduleId} not found. Cannot process purchase.", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Shop_Presenter: Runtime data for module ID {moduleId} not found. This should not happen if modules are initialized to all players.", this);
                return;
            }

            // ���x����0�̃��W���[���͍w���ł��Ȃ�
            if (runtimeModule.CurrentLevelValue == 0) // CurrentLevelValue���g�p
            {
                Debug.LogWarning($"Shop_Presenter: Module ID {moduleId} ({masterData.ViewName}) is at level 0. Cannot purchase from shop. Please upgrade it first (e.g., via drops).", this);
                return;
            }

            // �w���\������i������������邩�j
            if (_playerCore.Money.CurrentValue >= masterData.BasePrice)
            {
                _playerCore.PayMoney(masterData.BasePrice);
                Debug.Log($"Shop_Presenter: Player purchased module ID {moduleId} ({masterData.ViewName}) for {masterData.BasePrice} coins. Remaining coins: {_playerCore.Money.CurrentValue}.", this);

                // ���W���[�����v���C���[�̃����^�C�����W���[���ɒǉ��i���ʂ�1���₷�j
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);

                // �w�������̃t�B�[�h�o�b�N (UI�X�V�Ȃ�)
                UpdatePurchaseButtonsInteractability();
            }
            else
            {
                Debug.Log($"Shop_Presenter: Not enough coins to purchase module ID {moduleId} ({masterData.ViewName}). Required: {masterData.BasePrice}, Have: {_playerCore.Money.CurrentValue}.", this);
            }
        }
    }
}