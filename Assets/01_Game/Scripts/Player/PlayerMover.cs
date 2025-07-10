using Assets.IGC2025.Scripts.GameManagers;
using R3;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : BasePlayerComponent
{
    // ---------- Field
    private Rigidbody _rb;

    protected override void OnInitialize()
    {
        // �F�X�擾����
        _rb = GetComponent<Rigidbody>();
        var gameManager = GameManager.Instance;

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
                moveDirection * Core.MoveSpeed.CurrentValue +
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
                if(gameManager.CurrentGameState.CurrentValue == GameState.BATTLE
                || gameManager.CurrentGameState.CurrentValue == GameState.BUILD)
                {
                    gameManager.ChangeGameState(GameState.PAUSE);
                }
                // �|�[�Y���̂ݏ���
                else if(gameManager.CurrentGameState.CurrentValue == GameState.PAUSE)
                {
                    // �|�[�Y����O�̃Q�[���X�e�[�g�ɖ߂�
                    if (gameManager.PrevGameState == GameState.BATTLE)
                    {
                        gameManager.ChangeGameState(GameState.BATTLE);
                    }
                    else if(gameManager.PrevGameState == GameState.BUILD)
                    {
                        gameManager.ChangeGameState(GameState.BUILD);
                    }
                }
            })
            .AddTo(this);
    }
}
