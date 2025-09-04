using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.AT;
using Assets.IGC2025.Scripts.View;
using Game.Utils;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterShopCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private ViewShopCanvas _shopView;
        [SerializeField] private ModuleDataStore _moduleDataStore;
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private PlayerCore _playerCore;

        [Header("Views")]
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;

        [Header("Views_Hovered")]
        [SerializeField] private TextMeshProUGUI _unitName;
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private Image _image;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _atk;
        [SerializeField] private TextMeshProUGUI _rpd;
        [SerializeField] private TextMeshProUGUI _rng;
        [SerializeField] private TextMeshProUGUI _prc;
        [SerializeField] private Button _confirmPurchaseButton;

        // -----Field
        private CompositeDisposable _disposables = new CompositeDisposable();
        private CompositeDisposable _moduleLevelAndQuantityChangeDisposables = new CompositeDisposable();

        private int _currentSelectedModuleId = -1;
        public bool IsInitialized { get; private set; } = false;

        // -----UnityMessage

        private void OnDestroy()
        {
            _disposables.Dispose();
            _moduleLevelAndQuantityChangeDisposables.Dispose();
        }

        // -----PublicMethod
        public void Initialize()
        {
            if (IsInitialized) return;

            if (_runtimeModuleManager == null)
                _runtimeModuleManager = RuntimeModuleManager.Instance;

            // 依存チェック（落とさないのが最優先）
            if (_shopView == null || _moduleDataStore == null || _playerCore == null || _runtimeModuleManager == null)
            {
                Debug.LogError($"{nameof(PresenterShopCanvas)}: 依存が不足しています。", this);
                enabled = false;
                return;
            }

            // ランタイムモジュールの変化を購読（レベル/所持数など）
            _runtimeModuleManager.OnAllRuntimeModuleDataChanged
                .Subscribe(_ =>
                {
                    _moduleLevelAndQuantityChangeDisposables.Clear();
                    if (_runtimeModuleManager.AllRuntimeModuleData != null)
                    {
                        foreach (var rmd in _runtimeModuleManager.AllRuntimeModuleData)
                            SubscribeToModuleChanges(rmd);
                    }
                    DisplayShopContent();
                    UpdatePurchaseButtonsInteractability();
                })
                .AddTo(_disposables);

            // ショップのUIイベント
            _shopView.OnModulePurchaseRequested
                .Subscribe(moduleId => HandleModulePurchaseRequested(moduleId))
                .AddTo(_disposables);

            _shopView.OnModuleDetailRequested
                .Subscribe(id => ShowModuleDetailPanel(id))
                .AddTo(_disposables);

            // 所持金変化：表示＆購入可能ボタンの更新
            _playerCore.Money
                .Subscribe(x =>
                {
                    if (_moneyTextScaleAnimation != null)
                        _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                    UpdatePurchaseButtonsInteractability();
                })
                .AddTo(_disposables);

            // 初回描画
            PrepareAndShowShopUI();

            IsInitialized = true;
#if UNITY_EDITOR
            Debug.Log($"{nameof(PresenterShopCanvas)} initialized.", this);
#endif
        }

        // -----PrivateMethod

        private void SubscribeToModuleChanges(RuntimeModuleData runtimeModuleData)
        {
            if (runtimeModuleData?.Level != null)
            {
                runtimeModuleData.Level
                    .Subscribe(_ =>
                    {
                        // レベル変化でUI反映／購入可否を更新
                        PrepareAndShowShopUI();
                        UpdatePurchaseButtonsInteractability();
                    })
                    .AddTo(_moduleLevelAndQuantityChangeDisposables);
            }
        }

        private void PrepareAndShowShopUI()
        {
            if (_shopView == null || _moduleDataStore == null || _runtimeModuleManager == null || _playerCore == null)
                return;

            DisplayShopContent();
        }

        private void DisplayShopContent()
        {
            var list = _runtimeModuleManager.AllRuntimeModuleData?
                .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0)
                .ToList() ?? new List<RuntimeModuleData>();

            _shopView.DisplayShopModules(list, _moduleDataStore);
        }

        private void UpdatePurchaseButtonsInteractability()
        {
            if (_playerCore == null || _moduleDataStore?.DataBase?.ItemList == null || _runtimeModuleManager == null)
                return;

            foreach (var runtimeData in _runtimeModuleManager.AllRuntimeModuleData
                         .Where(rmd => rmd != null && rmd.CurrentLevelValue > 0))
            {
                var masterData = _moduleDataStore.FindWithId(runtimeData.Id);
                if (masterData == null) continue;

                bool canAfford = _playerCore.Money.CurrentValue >= masterData.BasePrice;
                _shopView.SetPurchaseButtonInteractable(runtimeData.Id, canAfford);
            }
        }

        private void ShowModuleDetailPanel(int moduleId)
        {
            var module = _moduleDataStore.FindWithId(moduleId);
            var runtime = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (module == null || runtime == null) return;

            int level = runtime.CurrentLevelValue;

            float scaledAtk = StateValueCalculator.CalcStateValue(
                baseValue: module.ModuleState?.Attack ?? 0f,
                currentLevel: level, maxLevel: 5, maxRate: 0.5f);

            float scaledPrice = StateValueCalculator.CalcStateValue(
                baseValue: module.BasePrice,
                currentLevel: level, maxLevel: 5, maxRate: 0.5f);

            if (_unitName != null) _unitName.text = module.ViewName;
            if (_infoText != null) _infoText.text = module.Description;
            if (_level != null) _level.text = $"{level}";
            if (_image != null) _image.sprite = module.MainSprite;
            if (_icon != null) _icon.sprite = module.BlockSprite;
            if (_atk != null) _atk.text = $"{(int)scaledAtk}";
            if (_rpd != null) _rpd.text = $"{Mathf.FloorToInt(module?.ModuleState?.Interval ?? 0)}";
            if (_rng != null) _rng.text = $"{Mathf.FloorToInt(module?.ModuleState?.SearchRange ?? 0)}";
            if (_prc != null) _prc.text = $"{(int)scaledPrice}";

            _currentSelectedModuleId = moduleId;

            if (_confirmPurchaseButton != null)
            {
                _confirmPurchaseButton.onClick.RemoveAllListeners();
                _confirmPurchaseButton.onClick.AddListener(() => HandleModulePurchaseRequested(moduleId));
            }
        }


        private void HandleModulePurchaseRequested(int moduleId)
        {
            var masterData = _moduleDataStore.FindWithId(moduleId);
            if (masterData == null) return;

            var runtimeModule = _runtimeModuleManager.GetRuntimeModuleData(moduleId);
            if (runtimeModule == null || runtimeModule.CurrentLevelValue == 0) return;

            var payPrice = StateValueCalculator.CalcStateValue(
                baseValue: masterData.BasePrice,
                currentLevel: runtimeModule.Level.CurrentValue,
                maxLevel: 5, maxRate: 0.5f);

            if (_playerCore.Money.CurrentValue >= payPrice)
            {
                _playerCore.PayMoney((int)payPrice);
                _runtimeModuleManager.ChangeModuleQuantity(moduleId, 1);
                UpdatePurchaseButtonsInteractability();
            }
        }

        public void InShopSE() => GameSoundManager.Instance.PlaySE("Sys_menu_in", "System");
        public void OutShopSE() => GameSoundManager.Instance.PlaySE("Sys_menu_out", "System");
    }
}
