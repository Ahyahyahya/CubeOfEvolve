using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using R3;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterPauseCanvas : MonoBehaviour, IPresenter
    {
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private GameManager gameManager;

        [Header("Views")]
        [SerializeField] private Canvas canvas;

        // -----Field
        public bool IsInitialized { get; private set; } = false;

        // -----UnityMessage
        private void Start()
        {
            if (canvas != null) canvas.enabled = false; // ‰Šú‚Í•Â
        }

        // -----PublicMethods
        public void Initialize()
        {
            if (IsInitialized) return;

            if (gameManager == null) gameManager = GameManager.Instance;

            if (gameManager == null || canvas == null)
            {
                Debug.LogWarning($"{nameof(PresenterPauseCanvas)}: ˆË‘¶‚ª•s‘«‚Ì‚½‚ß‰Šú‰»‚ğ’†~‚µ‚Ü‚·B", this);
                return;
            }

            var canvasCtrl = canvas.GetComponent<CanvasCtrl>();
            if (canvasCtrl == null)
            {
                Debug.LogWarning($"{nameof(PresenterPauseCanvas)}: CanvasCtrl ‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñB", this);
                return;
            }

            gameManager.CurrentGameState
                .Skip(1)
                .Subscribe(x =>
                {
                    if (x == GameState.PAUSE) canvasCtrl.OnOpenCanvas();
                    else canvasCtrl.OnCloseCanvas();
                })
                .AddTo(this);

            IsInitialized = true;
#if UNITY_EDITOR
            Debug.Log($"{nameof(PresenterPauseCanvas)} initialized.", this);
#endif
        }

    }
}