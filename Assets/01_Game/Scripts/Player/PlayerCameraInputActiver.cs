using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using R3;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraInputActiver : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private CinemachineInputAxisController _playerCameraInput;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        var currentGameState = GameManager.Instance.CurrentGameState;

        // �X�e�[�g���ς�邲�ƂɃJ����������ł��邩��ς���
        currentGameState
            .Subscribe(x =>
            {
                if (x == GameState.BATTLE)
                {
                    _playerCameraInput.enabled = true;
                }
                else
                {
                    _playerCameraInput.enabled = false;
                }
            })
            .AddTo(this);

        // �r���h������̃{�^���������Ă���Ƃ��Ɍ���J��������\
        InputEventProvider.MoveCamera
            .Where(_ => currentGameState.CurrentValue == GameState.BUILD
            || currentGameState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // ��������J��������\
                if(x)
                {
                    _playerCameraInput.enabled = true;
                }
                // ��������J��������s��
                else
                {
                    _playerCameraInput.enabled = false;
                }
            })
            .AddTo(this);

    }
}
