using Assets.IGC2025.Scripts.GameManagers;
using Assets.IGC2025.Scripts.View;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.IGC2025.Scripts.Presenter
{
    public class PresenterResultCanvas : MonoBehaviour
    {
        // ----- SerializedField
        [Header("Models")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private SceneLoader _sceneLoader;
        [Header("Views")]
        [SerializeField] private ViewResultCanvas _view;
        [SerializeField] private Button[] _endGameButton;

        // ----- UnityMessage
        private void Start()
        {
            if (_gameManager != null)
            {
                _gameManager.CurrentGameState
                    .Where(x => x == GameState.GAMEOVER || x == GameState.GAMECLEAR)
                    .Subscribe(x => _view.ShowCanvas(x))
                    .AddTo(this);
            }

            if (_sceneLoader != null)
            {
                if (_endGameButton.Length == 0) return;
                for (int i = 0; i < _endGameButton.Length; i++)
                {
                    _endGameButton[i].onClick.AddListener(() => _sceneLoader.ReloadScene());

                }
            }
        }
        private void Awake()
        {
            // �ˑ��֌W�����ݒ�̏ꍇ�A�V�[������擾�����݂�
            if (_gameManager == null) _gameManager = GameManager.Instance;

            // �K�{�̈ˑ��֌W�������Ă��邩�`�F�b�N
            if (_gameManager == null || _sceneLoader == null)
            {
                Debug.LogError("PresenterResultCanvas: _gameManager���ݒ肳��Ă��܂���B���̃R���|�[�l���g�𖳌��ɂ��܂��B", this);
                enabled = false;
            }
        }

        // -----Private



        #region ModelToView



        #endregion


        #region ViewToModel



        #endregion

    }
}