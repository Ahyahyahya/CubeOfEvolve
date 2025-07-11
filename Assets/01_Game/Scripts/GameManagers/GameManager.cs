using Assets.AT;
using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

[RequireComponent(typeof(TimeManager))]
[RequireComponent(typeof(SceneLoader))]
public class GameManager : MonoBehaviour
{
    // ---------- Singleton
    public static GameManager Instance;

    // ---------- SerializeField
    private TimeManager _timeManager;
    private SceneLoader _sceneLoader;

    // ---------- Field
    private CameraCtrlManager _cameraCtrlManager;
    private CanvasCtrlManager _canvasCtrlManager;

    private GameState _prevGameState;
    public static bool IsRetry { get; private set; } = false;


    // ---------- RP
    private ReactiveProperty<GameState> _currentGameState = new();
    public ReadOnlyReactiveProperty<GameState> CurrentGameState => _currentGameState;

    // ---------- Property
    public TimeManager TimeManager => _timeManager;
    public SceneLoader SceneLoader => _sceneLoader;
    public GameState PrevGameState => _prevGameState;

    // ---------- UnityMessage
    private void Awake()
    {
        // ���łɕʂ̃C���X�^���X�����݂���ꍇ�A�����j��
        if (Instance != null && Instance != this)
        {
            Debug.Log("[GameManager] �Â��C���X�^���X��j�����A�ŐV�̃C���X�^���X�ɍ����ւ��܂�", this);
            Destroy(Instance.gameObject);
        }

        // ���̃C���X�^���X���ŐV�Ƃ��ēo�^
        Instance = this;
        Debug.Log("[GameManager] �V�����C���X�^���X���ݒ肳��܂���", this);

        // ������
        _timeManager = GetComponent<TimeManager>();
        _sceneLoader = GetComponent<SceneLoader>();
        _currentGameState.Value = GameState.TITLE;
        _prevGameState = _currentGameState.Value;

    }

    private void Start()
    {
        // �Q�[���X�e�[�g�ύX���̏���
        _currentGameState
            .Skip(1)
            .Subscribe(x =>
            {
                Debug.Log($"�yGameManager�z �Q�[���X�e�[�g���ύX����܂��� {_prevGameState} -> {x}");
                _canvasCtrlManager = CanvasCtrlManager.Instance;

                switch (x)
                {
                    case GameState.TITLE:
                        _canvasCtrlManager.ShowOnlyCanvas("TitleView");
                        break;

                    case GameState.INITIALIZE:

                        break;

                    case GameState.READY:
                        ShowStartThenReady().Forget(); // �� �񓯊��̏�����ʂ�
                        break;

                    case GameState.BATTLE:
                        // �J�����ړ�
                        CameraCtrlManager.Instance.ChangeCamera("Player Camera");
                        StartGame();
                        break;

                    case GameState.BUILD:
                        CameraCtrlManager.Instance.ChangeCamera("Build Camera");
                        StopGame();
                        break;

                    case GameState.SHOP:
                        StopGame();
                        break;

                    case GameState.PAUSE:
                        StopGame();
                        break;

                    case GameState.TUTORIAL:
                        StopGame();
                        break;

                    case GameState.GAMEOVER:
                        GameSoundManager.Instance.StopBGMWithFade(.5f);
                        GameSoundManager.Instance.PlaySE("Gameover", "SE");
                        StopGame();
                        break;

                    case GameState.GAMECLEAR:
                        GameSoundManager.Instance.PlayBGM("Clear1", "BGM");

                        StopGame();
                        break;
                }
            })
            .AddTo(this);
    }

    private void OnEnable()
    {
        ResetGame();
        if (IsRetry)
        {
            IsRetry = false;
            ChangeGameState(GameState.READY);
        }
        else
        {
            ChangeGameState(GameState.TITLE);
        }
    }

    // ---------- Event
    /// <summary>
    /// �Q�[���X�e�[�g��ύX
    /// </summary>
    /// <param name="state"></param>
    public void ChangeGameState(GameState state)
    {
        // �O�̃X�e�[�g���X�V
        _prevGameState = _currentGameState.Value;

        // ���݂̃X�e�[�g���X�V
        _currentGameState.Value = state;
    }

    /// <summary>
    /// �Q�[���X�e�[�g�ύX(�C���X�y�N�^�[�p)
    /// </summary>
    /// <param name="stateNum"></param>
    [EnumAction(typeof(GameState))]
    public void ChangeGameState(int stateNum)
    {
        var state = (GameState)stateNum;

        if (_currentGameState.Value == GameState.READY) return;

        // �O�̃X�e�[�g���X�V
        _prevGameState = _currentGameState.Value;

        // ���݂̃X�e�[�g���X�V
        _currentGameState.Value = state;
    }

    // ---------- PrivateMethod
    /// <summary>
    /// �Q�[���J�n�O�̏���
    /// </summary>
    /// <returns></returns>
    private async UniTask ReadyGameAsync()
    {
        _canvasCtrlManager = CanvasCtrlManager.Instance;

        var readyCtrl = _canvasCtrlManager
            .GetCanvas("ReadyView")
            .GetComponent<ReadyViewCanvasController>();

        await readyCtrl.PlayReadySequenceAsync(() =>
        {
            ChangeGameState(GameState.BATTLE);
            _canvasCtrlManager.ShowOnlyCanvas("GameView");
        });
    }


    /// <summary>
    /// �Q�[�����J�n
    /// </summary>
    private void StartGame()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// �Q�[�����ĊJ
    /// </summary>
    private void ContinueGame()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// �Q�[�����ꎞ��~
    /// </summary>
    private void StopGame()
    {
        Time.timeScale = 0f;
    }

    /// <summary>
    /// �Q�[�������Z�b�g
    /// </summary>
    private void ResetGame()
    {
        _timeManager.ResetTimer();
        Time.timeScale = 1;
    }

    // ---------- PublicMethod

    public void RequestRetry()
    {
        IsRetry = true;
        Instance.SceneLoader.ReloadScene();
    }

    // ---------- PrivateMethod
    private async UniTask ShowStartThenReady()
    {
        GuideManager guideManager = GuideManager.Instance;
        await guideManager.ShowGuideAndWaitAsync("Start");
        await guideManager.DoBuildModeAndWaitAsync();
        await ReadyGameAsync();
    }
}
