using R3;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;
using Assets.AT;
using System.Collections;
using Unity.Cinemachine;
using AT.uGUI;

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
        // �V���O���g��
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // ������
        _timeManager = GetComponent<TimeManager>();
        _sceneLoader = GetComponent<SceneLoader>();
        _prevGameState = _currentGameState.Value;

    }

    private void Start()
    {
        // �A�N�Z�X�擾
        _cameraCtrlManager = CameraCtrlManager.Instance;
        _canvasCtrlManager = CanvasCtrlManager.Instance;

        // �Q�[���X�e�[�g�ύX���̏���
        _currentGameState
            .Skip(1)
            .Subscribe(x =>
            {
                Debug.Log($"�yGameManager�z �Q�[���X�e�[�g���ύX����܂��� {_prevGameState} -> {x}");

                switch (x)
                {
                    case GameState.TITLE:
                        Time.timeScale = 1f;
                        break;

                    case GameState.INITIALIZE:
                        break;

                    case GameState.READY:
                        StartCoroutine(ReadyGame());
                        ResetGame();
                        break;

                    case GameState.BATTLE:
                        // �J�����ړ�
                        CameraCtrlManager.Instance.ChangeCamera("Player Camera");
                        StartGame();
                        break;

                    case GameState.BUILD:
                        // �J�����ړ�
                        CameraCtrlManager.Instance.ChangeCamera("Build Camera");
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
    private IEnumerator ReadyGame()
    {


        // �e�L�X�g���o�i��F�u�o���������v�j��\��
        var readyTextCanvas = _canvasCtrlManager.GetCanvas("ReadyView").GetComponent<Canvas>();
        readyTextCanvas.enabled = true;
        var startText = readyTextCanvas.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>();
        startText.text = "";

        // �ꎞ��~���
        Time.timeScale = 0f;

        // �J�����ړ�
        CameraCtrlManager.Instance.ChangeCamera("Player Camera");
        // ���o������
        yield return new WaitForSecondsRealtime(_cameraCtrlManager.CameraBlendTime);


        // ���b�ԑҋ@�i���A�����ԂŁj
        yield return new WaitForSecondsRealtime(1f);

        // �u�^�b�v���ĊJ�n�v�Ȃǂ̉��o��\��
        startText.text = "�N���b�N���ďo���I";

        // ���͑ҋ@�i�}�E�X�N���b�N or �^�b�v�j
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        // ���������F�Q�[���r���[��
        ChangeGameState(GameState.BATTLE);
        _canvasCtrlManager.ShowOnlyCanvas("GameView");
        Time.timeScale = 1f;

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
}
