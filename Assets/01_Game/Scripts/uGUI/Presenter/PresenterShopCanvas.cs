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
    /// <summary>
    /// �V���b�v��ʂ̃v���[���^�[�N���X�ł��B
    /// �V���b�v�̕\�����W�b�N�A�v���C���[�̏������⃂�W���[���f�[�^�Ƃ̘A�g�A�w�������Ȃǂ�S�����܂��B
    /// </summary>
    public class PresenterShopCanvas : MonoBehaviour
    {
        // ----- Serializable Fields (�V���A���C�Y�t�B�[���h)
        [Header("Models")]
        [SerializeField] private ViewShopCanvas _shopView; // �V���b�v��UI�\�����Ǘ�����View�ւ̎Q�ƁB
        [SerializeField] private ModuleDataStore _moduleDataStore; // ���W���[���̃}�X�^�[�f�[�^��ێ�����f�[�^�X�g�A�ւ̎Q�ƁB
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager; // �����^�C�����W���[���f�[�^���Ǘ�����}�l�[�W���[�ւ̎Q�ƁB
        [SerializeField] private PlayerCore _playerCore; // �v���C���[�̃R�A�f�[�^�i�������Ȃǁj�ւ̎Q�ƁB

        [Header("Views")]
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation; // �������\���̃e�L�X�g�A�j���[�V�����R���|�[�l���g�B
        [SerializeField] private TextMeshProUGUI _hoveredModuleInfoText; // �z�o�[���̃��W���[�����e�L�X�g�B

        // ----- Private Fields (�v���C�x�[�g�t�B�[���h)
        private CompositeDisposable _disposables = new CompositeDisposable(); // �I�u�W�F�N�g�j�����ɍw�ǂ��܂Ƃ߂ĉ������邽�߂�Disposable�B
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable(); // ���W���[�����x���␔�ʕύX�̍w�ǂ��Ǘ����邽�߂�Disposable�B

        // ----- Unity Messages (Unity�C�x���g���b�Z�[�W)

        private void Awake()
        {
            // �ˑ��֌W�̃`�F�b�N�ƃG���[���O
            if (_shopView == null) Debug.LogError("Shop_Presenter: ShopView��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_moduleDataStore == null) Debug.LogError("Shop_Presenter: ModuleDataStore��Inspector�Őݒ肳��Ă��܂���I", this);
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance; // �C���X�^���X�������擾
            if (_playerCore == null) Debug.LogError("Shop_Presenter: PlayerCore��Inspector�Őݒ肳��Ă��܂���I", this);

            // �K�{�̈ˑ��֌W����ł��s�����Ă���ꍇ�A�R���|�[�l���g�𖳌��ɂ��܂��B
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: �ˑ��֌W���s�����Ă��܂��BInspector�̐ݒ�ƃV�[���̃Z�b�g�A�b�v���m�F���Ă��������B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
                enabled = false;
                return;
            }

            // �v���C���[�̏��������ύX���ꂽ�ۂɁA�e�L�X�g�A�j���[�V�������X�V���܂��B
            _playerCore.Money
                .Subscribe(x => _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f))
                .AddTo(_disposables);

            // �����^�C�����W���[���f�[�^�S�̂��ύX���ꂽ�ۂɁA���W���[���̕ύX�w�ǂ��Đݒ肵�A�V���b�vUI���X�V���܂��B
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {
                    Debug.Log("Shop_Presenter: �����^�C�����W���[���f�[�^�R���N�V�������ύX����܂����B���W���[���̕ύX�w�ǂ��Đݒ肵�A�V���b�vUI���X�V���܂��B");
                    _moduleLevelAndQuantityChangeDisposables.Clear(); // �����̍w�ǂ��N���A
                    foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                    {
                        SubscribeToModuleChanges(rmd); // �e���W���[���̕ύX���w��
                    }
                    DisplayShopContent(); // �V���b�v���e���ĕ\��
                })
                .AddTo(_disposables);

            // �V���b�vUI�̏��������ƕ\�����s���܂��B
            PrepareAndShowShopUI();
        }


        private void Start()
        {
            // View����̃��W���[���w���v���C�x���g���w�ǂ��A�w���������Ăяo���܂��B
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            // View����̃��W���[���z�o�[�C�x���g���w�ǂ��A�z�o�[����\�����܂��B
            _shopView.OnModuleHovered
                .Subscribe(x => HandleModuleHovered(x))
                .AddTo(this); // ����GameObject���j�����ꂽ�玩���I�ɍw�ǉ���
        }


        private void OnDestroy()
        {
            _disposables.Dispose(); // ���C���̍w�ǂ�����
            _moduleLevelAndQuantityChangeDisposables.Dispose(); // ���W���[���ύX�Ɋւ���w�ǂ�����
        }

        // ----- Private Methods (�v���C�x�[�g���\�b�h)
        /// <summary>
        /// �w�肳�ꂽ�����^�C�����W���[���f�[�^�̕ύX�i���x���Ȃǁj���w�ǂ��A�V���b�vUI���X�V���܂��B
        /// </summary>
        /// <param name="runtimeModuleData">�w�ǑΏۂ̃����^�C�����W���[���f�[�^�B</param>
        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            if (runtimeModuleData.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(level =>
                    {
                        Debug.Log($"Shop_Presenter: ���W���[��ID {runtimeModuleData.Id} ({_moduleDataStore.FindWithId(runtimeModuleData.Id)?.ViewName}) �̃��x���� {level} �ɕύX����܂����B�V���b�vUI���X�V���܂��B");
                        PrepareAndShowShopUI(); // ���x���ύX���ɃV���b�vUI���ď����E�\��
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables); // ���W���[�����x���ύX�w�Ǘp��Disposable�ɒǉ�
            }
            else
            {
                Debug.LogWarning($"Shop_Presenter: �����^�C�����W���[���f�[�^ID {runtimeModuleData.Id} ��Level��ReactiveProperty�Ƃ��Č��J���Ă��܂���B", this);
            }
        }

        /// <summary>
        /// �V���b�vUI�̏����ƕ\�����s���܂��B
        /// ��ɃV���b�v�ɕ\�����郂�W���[���̃f�[�^�擾��View�ւ̈����n�����s���܂��B
        /// </summary>
        private void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
            {
                Debug.LogError("Shop_Presenter: �V���b�vUI���������邽�߂̈ˑ��֌W����������Ă��܂���IAwake�̃��O���m�F���Ă��������B", this);
                return;
            }

            DisplayShopContent(); // �V���b�v�̓��e��\��
        }

        /// <summary>
        /// ���݂̃����^�C�����W���[���f�[�^�Ɋ�Â��ăV���b�v�̃R���e���c��\�����܂��B
        /// </summary>
        private void DisplayShopContent()
        {
            // �V���b�v�ɕ\�����郉���^�C�����W���[���f�[�^���t�B���^�����O���܂��B�i��: ���x����0���傫�����W���[���j
            List<RuntimeModuleData> shopRuntimeModules = _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList();

            // View�Ƀ��W���[���\�����˗����܂��B
            _shopView.DisplayShopModules(shopRuntimeModules, _moduleDataStore);
            // �w���{�^���̃C���^���N�g�\�����X�V���܂��B
            UpdatePurchaseButtonsInteractability();
        }

        /// <summary>
        /// �v���C���[�̏������Ɗe���W���[���̉��i�Ɋ�Â��āA�w���{�^���̃C���^���N�g�\�����X�V���܂��B
        /// </summary>
        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore == null || _moduleDataStore.DataBase?.ItemList == null)
            {
                Debug.LogError("Shop_Presenter: �w���{�^���̃C���^���N�g�\�����X�V���邽�߂̕K�v�ȃf�[�^���s�����Ă��܂��B", this);
                return;
            }

            // ���݃v���C���[���������Ă���i���x����1�ȏ�́j���W���[���ɂ��āA�w���{�^���̏�Ԃ��X�V���܂��B
            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0))
            {
                ModuleData masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                // �v���C���[���w���ł��邩�ǂ����𔻒f���܂��B
                bool canAfford = _playerCore.Money.CurrentValue >= masterData.BasePrice; // �����ł͊ȗ����̂��ߒ艿���g�p�B��q�̊����v�Z��K�p���邱�Ƃ��\�B
                _shopView.SetPurchaseButtonInteractable(runtimeData.Id, canAfford); // View�Ƀ{�^���̏�ԍX�V���˗����܂��B
            }
        }

        /// <summary>
        /// ���W���[���̍w���v����View���炠�����ۂɏ������܂��B
        /// �v���C���[�̏������`�F�b�N�A���i�v�Z�A�w�������A�����UI�X�V���s���܂��B
        /// </summary>
        /// <param name="moduleId">�w�����v�����ꂽ���W���[����ID�B</param>
        private void HandleModulePurchaseRequested(int moduleId)
        {
            ModuleData masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null)
            {
                Debug.LogError($"Shop_Presenter: ���W���[��ID {moduleId} �̃}�X�^�[�f�[�^��������܂���B�w���ł��܂���B", this);
                return;
            }

            RuntimeModuleData runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null)
            {
                Debug.LogError($"Shop_Presenter: ���W���[��ID {moduleId} �̃����^�C���f�[�^��������܂���B�w���ł��܂���B", this);
                return;
            }

            // ���x��0�̃��W���[���͍w���ł��Ȃ��Ƃ������W�b�N (��: ���J���̃��W���[���̓V���b�v�ɕ\������Ȃ��A���邢�͍w���ł��Ȃ�)
            if (runtimeModule.CurrentLevelValue == 0)
            {
                Debug.LogWarning($"Shop_Presenter: ���W���[��ID {moduleId} ({masterData.ViewName}) �̓��x��0�ł��B�w���ł��܂���B", this);
                return;
            }

            /// <summary>
            /// ���W���[���̍w�����i���v�Z���܂��B
            /// ���x���ɉ����Ċ�����K�p���郍�W�b�N�̗�ł��B
            /// </summary>
            /// <param name="maxDiscountRate">�ő劄���� (��: 0.5f��50%����)�B</param>
            /// <returns>�v�Z���ꂽ�w�����i�B</returns>
            float CalculatePrice(float maxDiscountRate)
            {
                // ���x��1�̏ꍇ�͒艿
                if (runtimeModule.CurrentLevelValue <= 1) return masterData.BasePrice;
                // ���x��5�ȏ�̏ꍇ�͍ő劄��
                if (runtimeModule.CurrentLevelValue >= 5) return masterData.BasePrice * (1f - maxDiscountRate);

                // ���x��2����4�̊ԂŊ���������`���
                float discountProgress = (runtimeModule.CurrentLevelValue - 1) / 4f; // 1 (Lv2) ���� 4 (Lv5) �܂Ői��
                float currentDiscountRate = maxDiscountRate * discountProgress;
                return masterData.BasePrice * (1f - currentDiscountRate);
            }

            var payPrice = CalculatePrice(0.5f); // �ő�50%�̊�����K�p���ĉ��i���v�Z

            // �v���C���[�����W���[�����w���ł��邩�`�F�b�N���܂��B
            if (_playerCore.Money.CurrentValue >= payPrice)
            {
                _playerCore.PayMoney((int)payPrice); // �v���C���[���������x�����܂��B
                Debug.Log($"Shop_Presenter: �v���C���[�����W���[��ID {moduleId} ({masterData.ViewName}) �� {payPrice:F0} ���ōw�����܂����B�c���: {_playerCore.Money.CurrentValue}�B", this);
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1); // ���W���[���̐��ʂ𑝂₵�܂��B
                UpdatePurchaseButtonsInteractability(); // �w���{�^���̃C���^���N�g�\�����X�V���܂��B
            }
            else
            {
                Debug.Log($"Shop_Presenter: ���W���[��ID {moduleId} ({masterData.ViewName}) ���w������̂ɋ����s�����Ă��܂��B���݂̏�����: {_playerCore.Money.CurrentValue}�A�K�v���z: {payPrice:F0}�B", this);
            }
        }

        /// <summary>
        /// ���W���[���Ƀ}�E�X���z�o�[���ꂽ�ۂɁA���̃��W���[���̏ڍ׏����e�L�X�g�ŕ\�����܂��B
        /// </summary>
        /// <param name="hoveredModuleId">�z�o�[���ꂽ���W���[����ID�B</param>
        private void HandleModuleHovered(int hoveredModuleId)
        {
            // �z�o�[���e�L�X�g���ݒ肳��Ă���΁A���W���[���̐�������\�����܂��B
            if (_hoveredModuleInfoText != null)
            {
                ModuleData hoveredMasterData = _moduleDataStore.FindWithId(hoveredModuleId);
                _hoveredModuleInfoText.text = hoveredMasterData?.Description ?? "���Ȃ�"; // �������Ȃ���΁u���Ȃ��v�ƕ\��
            }
        }
    }
}