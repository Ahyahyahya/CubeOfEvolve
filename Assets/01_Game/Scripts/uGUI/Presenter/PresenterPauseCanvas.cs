// AT
// �|�[�Y��ʂł̏��������s����B

using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterPauseCanvas : MonoBehaviour
    {
        // -----
        // -----SerializeField
        [Header("Models")]
        [SerializeField] private GameManager gameManager;

        [Header("Views")]
        [SerializeField] private Canvas canvas;

        // -----UnityMessage
        private void Start()
        {
            if (!Initialize()) enabled = false;
        }

        // -----private
        private bool Initialize()
        {
            // �Q�Ɗm�F
            if (gameManager == null)
            {
                Debug.LogWarning($"PresenterPauseCanvas: �Q�Ɛ؂�̂��ߑ��");
                gameManager = GameManager.Instance;
            }

            // �ˑ��`�F�b�N
            if (gameManager == null || canvas == null)
            {
                Debug.LogWarning($"PresenterPauseCanvas: �ˑ����s���̂��ߏ������~");
                return false;
            }

            var canvasCtrl = canvas.GetComponent<CanvasCtrl>();

            gameManager.CurrentGameState
                .Skip(1)
                .Subscribe(
                x =>
                {
                    if (x == GameState.PAUSE)
                        canvasCtrl.OnOpenCanvas();
                    else
                        canvasCtrl.OnCloseCanvas();
                })
                .AddTo(this);

            return true;
        }

    }
}