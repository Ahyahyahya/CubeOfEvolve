using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.View;
using AT.uGUI;
using Cysharp.Threading.Tasks;
using Game.Utils;
using R3;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public sealed class PresenterDropCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializedField
        [Header("Models")]
        [SerializeField] private RuntimeModuleManager _runtimeModuleManager;
        [SerializeField] private ModuleDataStore _moduleDataStore;

        [Header("Views")]
        [SerializeField] private ViewDropCanvas _dropView;

        [Header("Views_Hovered")]
        [SerializeField] private TextMeshProUGUI _infoText;
        [SerializeField] private TextMeshProUGUI _level;
        [SerializeField] private TextMeshProUGUI _levelNext;
        [SerializeField] private TextMeshProUGUI _ATK;
        [SerializeField] private TextMeshProUGUI _ATKNext;
        [SerializeField] private TextMeshProUGUI _Price;
        [SerializeField] private TextMeshProUGUI _PriceNext;

        // -----Field
        public bool IsInitialized { get; private set; } = false;
        private const int NUMBER_OF_OPTIONS = 3;
        private List<int> _candidateModuleIds = new List<int>();

        // ----- UnityMessage

        private void Awake()
        {
            if (_runtimeModuleManager == null) _runtimeModuleManager = RuntimeModuleManager.Instance;
        }

        // -----PublicMethod
        public void Initialize()
        {
            if (IsInitialized) return;

            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError($"{nameof(PresenterDropCanvas)}: �ˑ����s�����Ă��܂��B", this);
                enabled = false;
                return;
            }

            _dropView.OnModuleSelected
                .Subscribe(HandleModuleSelected)
                .AddTo(this);

            _dropView.OnModuleHovered
                .Subscribe(HandleModuleHovered)
                .AddTo(this);

            IsInitialized = true;

        }


        #region ModelToView

        /// <summary>
        /// �h���b�v�I��UI��\�����鏀�������AView�ɕ\�����˗����܂��B
        /// </summary>
        public async void PrepareAndShowDropUI()
        {
            if (_runtimeModuleManager == null || _moduleDataStore == null || _dropView == null)
            {
                Debug.LogError("�ˑ��֌W����������Ă��܂���I", this);
                return;
            }

            var gm = GameManager.Instance;
            var gameState = gm != null ? gm.CurrentGameState.CurrentValue : default;

            var displayIds = _runtimeModuleManager.GetDisplayModuleIds(NUMBER_OF_OPTIONS, gameState);
            var candidatePool = _runtimeModuleManager.AllRuntimeModuleData?
                                   .Where(m => m != null && m.CurrentLevelValue < 5).ToList()
                                   ?? new List<RuntimeModuleData>();

            if (displayIds == null || displayIds.Count == 0)
            {
                var player = FindFirstObjectByType<PlayerCore>();
                if (player != null) player.ReceiveExp(30);
                else Debug.LogWarning($"{nameof(PresenterDropCanvas)}: PlayerCore �������炸�A��֕�V��t�^�ł��܂���ł����B", this);
                return;
            }

            _dropView.DisplayModulesByIdOrRandom(displayIds, candidatePool, _moduleDataStore);

            var canvasCtrl = _dropView.GetComponent<CanvasCtrl>();
            if (canvasCtrl != null) canvasCtrl.OnOpenCanvas();

            // �J�A�j���J�n
            _dropView.PrepareInitialStatesForOpen();
            await _dropView.PlayOpenAsync();
        }

        #endregion


        #region ViewToModel

        /// <summary>
        /// ���[�U�[�����W���[����I�������ۂ̃C�x���g�n���h���B
        /// View����̃C�x���g�iR3�ōw�ǁj�ɂ���ČĂяo����܂��B
        /// </summary>
        /// <param name="selectedModuleId">�I�����ꂽ���W���[����ID�B</param>
        private async void HandleModuleSelected(int selectedModuleId)
        {
            if (_dropView == null) return;

            if (selectedModuleId != -1 && _runtimeModuleManager != null)
                _runtimeModuleManager.LevelUpModule(selectedModuleId);

            // �� �ǉ��F�A�j�� �� ����
            await _dropView.PlayCloseAsync();

            var canvasCtrl = _dropView.gameObject.GetComponent<CanvasCtrl>();
            if (canvasCtrl != null) canvasCtrl.OnCloseCanvas(); // �����̕���:contentReference[oaicite:7]{index=7}
        }

        /// <summary>
        /// ���W���[���Ƀ}�E�X�I�[�o�[�����ۂ̃C�x���g�n���h���B
        /// ���������X�V���܂��B
        /// </summary>
        /// <param name="EnterModuleId"></param>
        private void HandleModuleHovered(int enterModuleId)
        {
            if (_moduleDataStore == null || _runtimeModuleManager == null) return;

            var module = _moduleDataStore.FindWithId(enterModuleId);
            var runtime = _runtimeModuleManager.GetRuntimeModuleData(enterModuleId);
            if (module == null || runtime == null) return;

            int level = runtime.CurrentLevelValue;
            float atkBase = module.ModuleState?.Attack ?? 0f;
            float priceBase = module.BasePrice;

            if (_infoText != null) _infoText.text = module.Description;
            if (_level != null) _level.text = $"{level}";
            if (_levelNext != null) _levelNext.text = $"{level + 1}";
            if (_ATK != null) _ATK.text = $"{StateValueCalculator.CalcStateValue(atkBase, level, 5, 0.5f):F1}";
            if (_ATKNext != null) _ATKNext.text = $"{StateValueCalculator.CalcStateValue(atkBase, level + 1, 5, 0.5f):F1}";
            if (_Price != null) _Price.text = $"{StateValueCalculator.CalcStateValue(priceBase, level, 5, 0.5f):F1}";
            if (_PriceNext != null) _PriceNext.text = $"{StateValueCalculator.CalcStateValue(priceBase, level + 1, 5, 0.5f):F1}";
        }


        #endregion

    }
}