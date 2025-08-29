using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using AT.uGUI;
using ObservableCollections;
using R3;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : BasePlayerComponent
{
    // ---------- Field
    private Rigidbody _rb;
    private float _currentSpeed;

    protected override void OnInitialize()
    {
        // �F�X�擾����
        _rb = GetComponent<Rigidbody>();
        var gameManager = GameManager.Instance;
        var ccm = CanvasCtrlManager.Instance;
        var builder = GetComponent<PlayerBuilder>();

        UpdateMoveSpeedStatus();

        // �I�v�V�����Ď�
        ObserveStatusEffects();

        // �ړ�����
        InputEventProvider.Move
            .Where(_ => gameManager.CurrentGameState.CurrentValue == GameState.BATTLE)
            .Subscribe(x =>
            {
                // �J������xz�̒P�ʃx�N�g���擾
                var camaraForward =
                Vector3.Scale(
                    Camera.main.transform.forward,
                    new Vector3(1f, 0f, 1f)).normalized;

                // �ړ������擾
                var moveDirection = camaraForward * x.y + Camera.main.transform.right * x.x;

                _rb.linearVelocity =
                moveDirection * _currentSpeed +
                new Vector3(0f, _rb.linearVelocity.y, 0f);

                // �ړ����Ă������]
                if (moveDirection != Vector3.zero)
                {
                    var from = transform.rotation;
                    var to = Quaternion.LookRotation(moveDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        from,
                        to,
                        Core.RotateSpeed.CurrentValue * Time.deltaTime);
                }
            })
            .AddTo(this);

        // �|�[�Y�̊J����
        InputEventProvider.Pause
            .Where(x => x)
            .Subscribe(x =>
            {
                // �Q�[�����̂݃|�[�Y���J����悤��
                if (gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.PAUSE);
                }
                // �|�[�Y���̂ݏ���
                else if (gameManager.CurrentGameState.CurrentValue == GameState.PAUSE)
                {
                    // �|�[�Y����O�̃Q�[���X�e�[�g�ɖ߂�
                    if (gameManager.PrevGameState == GameState.BATTLE)
                    {
                        gameManager.ChangeGameState(GameState.BATTLE);
                    }
                    else if (gameManager.PrevGameState == GameState.BUILD)
                    {
                        gameManager.ChangeGameState(GameState.BUILD);
                    }
                }
            })
            .AddTo(this);

        // �V���b�v��ʂ̊J����
        InputEventProvider.Shop
            .Where(x => x)
            .Subscribe(_ =>
            {
                if (gameManager.CurrentGameState.CurrentValue == GameState.SHOP)
                {
                    gameManager.ChangeGameState(GameState.BATTLE);
                    ccm.GetCanvas("ShopView")?.OnCloseCanvas();
                }
                else if (gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.SHOP);
                    ccm.GetCanvas("GameView")?.OnCloseCanvas();
                    ccm.GetCanvas("BuildView")?.OnCloseCanvas();
                }
            })
            .AddTo(this);

        // �r���h��ʂ̊J����
        InputEventProvider.Build
            .Where(x => x)
            .Subscribe(_ =>
            {
                if (gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.BATTLE);
                    ccm.GetCanvas("BuildView")?.OnCloseCanvas();
                }
                else if (gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.SHOP)
                {
                    gameManager.ChangeGameState(GameState.BUILD);
                    ccm.GetCanvas("GameView")?.OnCloseCanvas();
                    ccm.GetCanvas("ShopView")?.OnCloseCanvas();
                }
            })
            .AddTo(this);

        // �r���h���̃��[�h�ؑ�
        InputEventProvider.Remove
            .Where(x => x)
            .Where(_ => gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                builder.ChangeBuildMode();
            })
            .AddTo(this);
    }

    /// <summary>
    /// �I�v�V�������Ď�
    /// </summary>
    private void ObserveStatusEffects()
    {
        var addStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
        .ObserveAdd(destroyCancellationToken)
        .Select(_ => Unit.Default);

        var removeStream = RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList
            .ObserveRemove(destroyCancellationToken)
            .Select(_ => Unit.Default);

        // �ǂ��炩�̃C�x���g���������������Ď�
        addStream.Merge(removeStream)
            .Subscribe(_ =>
            {
                UpdateMoveSpeedStatus();
            })
            .AddTo(this);
    }

    /// <summary>
    /// �ړ����x�X�V
    /// </summary>
    private void UpdateMoveSpeedStatus()
    {
        var speedRate = 0f;

        foreach (var effect in RuntimeModuleManager.Instance.CurrentCurrentStatusEffectList)
        {
            speedRate += effect.MoveSpeed;
        }

        _currentSpeed = Core.MoveSpeed.CurrentValue * (1f + speedRate / 100f);
    }
}
