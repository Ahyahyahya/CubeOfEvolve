using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using App.GameSystem.UI;
using R3;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace App.GameSystem.Presenters
{
    /// <summary>
    /// �V���b�v��ʂ̃v���[���^�[��S������N���X�B
    /// View����̃C�x���g��R3�ōw�ǂ��AModel�iRuntimeModuleManager, PlayerCore�j�𑀍삵�A
    /// View�ɕ\���f�[�^��n���܂��B�܂��ARuntimeModule�̃��x���ύX���Ď����A�V���b�v�\�����X�V���܂��B
    /// </summary>
    public class Shop_Presenter : MonoBehaviour
    {
        // ----- SerializedField (Unity Inspector�Őݒ�)
        [Header("Dependencies")]
        [SerializeField] private Shop_View _shopView; // �V���b�vUI��\������View�R���|�[�l���g�B
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���}�X�^�[�f�[�^���Ǘ�����f�[�^�X�g�A�B
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // �����^�C�����W���[���f�[�^���Ǘ�����}�l�[�W���[�B
        [SerializeField] private PlayerCore _playerCore; // �v���C���[�̃R�A�f�[�^�i�������Ȃǁj���Ǘ�����R���|�[�l���g�B

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
            if (_shopView == null) _shopView = FindObjectOfType<Shop_View>();
            if (_moduleDataStore == null) Debug.LogError("Shop_Presenter: ModuleDataStore��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
            if (_playerCore == null) _playerCore = FindObjectOfType<PlayerCore>();

            // �e�ˑ��֌W�������Ă��邩�ŏI�`�F�b�N
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: �ˑ��֌W���s�����Ă��܂��BInspector�̐ݒ�ƃV�[���̃Z�b�g�A�b�v���m�F���Ă��������B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
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
                Debug.LogError("Shop_Presenter: PlayerCore.Money ReactiveProperty��null�ł��B�������̕ύX���w�ǂł��܂���B", this);
            }

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
                    PrepareAndShowShopUI(); // �V���b�v���ĕ\�����ă��X�g���X�V
                })
                .AddTo(_disposables);

            // �����\���̂��߂ɃV���b�vUI���������ĕ\��
            PrepareAndShowShopUI();
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
                        PrepareAndShowShopUI(); // ���x�����ύX���ꂽ��V���b�v���ĕ\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} ��Level��ReactiveProperty�Ƃ��Č��J���Ă��܂���B", this);
            }

            // Quantity�̕ύX���w��
            if (runtimeModuleData.Quantity != null)
            {
                runtimeModuleData.Quantity
                    .Subscribe(quantity => {
                        Debug.Log($"���W���[��ID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) �̐��ʂ� {quantity} �ɕύX����܂����B�w���{�^���̃C���^���N�g�����X�V���܂��B");
                        UpdatePurchaseButtonsInteractability(); // ���ʕύX�����Ȃ�V���b�v���X�g�S�̂ł͂Ȃ��A�{�^���̃C���^���N�g���̂ݍX�V
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // �ʃ��W���[���̍w�ǂ͐�p��DisposableBag�ɒǉ�
            }
            else
            {
                Debug.LogWarning($"RuntimeModuleData ID {runtimeModuleData.Id} ��Quantity��ReactiveProperty�Ƃ��Č��J���Ă��܂���B", this);
            }
        }

        /// <summary>
        /// �v���C���[�̃R�C���ύX���ɃV���b�vUI���X�V���܂��B
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
                Debug.LogError("Shop_Presenter: �w���{�^���̃C���^���N�g�\�����X�V���邽�߂̕K�v�ȃf�[�^���s�����Ă��܂��B", this);
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
        /// ���W���[���w�����N�G�X�g���󂯎�����ۂ̃n���h���ł��B
        /// </summary>
        /// <param name="moduleId">�w�������N�G�X�g���ꂽ���W���[����ID�B</param>
        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Shop_Presenter: ���W���[��ID {moduleId} �̃}�X�^�[�f�[�^��������܂���B�w���������ł��܂���B", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Shop_Presenter: ���W���[��ID {moduleId} �̃����^�C���f�[�^��������܂���B����͑S�Ẵv���C���[�Ƀ��W���[��������������Ă���ꍇ�͔������Ȃ��͂��ł��B", this);
                return;
            }

            // ���x����0�̃��W���[���͍w���ł��Ȃ�
            if (runtimeModule.CurrentLevelValue == 0)
            {
                Debug.LogWarning($"Shop_Presenter: ���W���[��ID {moduleId} ({masterData.ViewName}) �̓��x��0�ł��B�V���b�v����w���ł��܂���B�܂��A�b�v�O���[�h���Ă��������i��: �h���b�v�o�R�Łj�B", this);
                return;
            }

            // �w���\������i������������邩�j
            if (_playerCore.Money.CurrentValue >= masterData.BasePrice)
            {
                _playerCore.PayMoney(masterData.BasePrice);
                Debug.Log($"Shop_Presenter: �v���C���[�����W���[��ID {moduleId} ({masterData.ViewName}) �� {masterData.BasePrice} �R�C���ōw�����܂����B�c��R�C��: {_playerCore.Money.CurrentValue}�B", this);

                // ���W���[�����v���C���[�̃����^�C�����W���[���ɒǉ��i���ʂ�1���₷�j
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);

                // �w�������̃t�B�[�h�o�b�N (UI�X�V�Ȃ�)
                UpdatePurchaseButtonsInteractability();
            }
            else
            {
                Debug.Log($"Shop_Presenter: ���W���[��ID {moduleId} ({masterData.ViewName}) ���w������̂ɃR�C�����s�����Ă��܂��B�K�v: {masterData.BasePrice}�A����: {_playerCore.Money.CurrentValue}�B", this);
            }
        }

        // ----- Public Methods (���J���\�b�h)
        /// <summary>
        /// �V���b�v��ʂ�\�����鏀�������AView�ɕ\�����˗����܂��B
        /// ���̃��\�b�h�͊O������Ăяo����܂��i��: GameManager��UIController�j�B
        /// �܂��ARuntimeModuleData�̕ύX�ɂ���Ă������I�ɌĂяo����邱�Ƃ�����܂��B
        /// </summary>
        public void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: �V���b�vUI���������邽�߂̈ˑ��֌W����������Ă��܂���IAwake�̃��O���m�F���Ă��������B", this);
                return;
            }

            // ���x��1�ȏ�̃��W���[���݂̂�View�ɓn��
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList();

            _shopView.DisplayShopModules(shopRuntimeModules);
            _shopView.UpdatePlayerCoins(_playerCore.Money.CurrentValue);
            UpdatePurchaseButtonsInteractability();
        }
    }
}