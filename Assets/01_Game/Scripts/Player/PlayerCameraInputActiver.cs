using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using R3;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraInputActiver : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private CinemachineInputAxisController _buildCameraInput;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // �Q�[���X�e�[�g���r���h���ɖ��񏉊���
        GameManager.Instance.CurrentGameState
            .Where(x => x == GameState.BUILD
            || x == GameState.TUTORIAL)
            .Subscribe(_ =>
            {
                _buildCameraInput.enabled = false;
            })
            .AddTo(this);

        // �r���h������̃{�^���������Ă���Ƃ��Ɍ���J��������\
        InputEventProvider.MoveCamera
            .Where(_ => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // ��������J��������\
                if(x)
                {
                    _buildCameraInput.enabled = true;
                }
                // ��������J��������s��
                else
                {
                    _buildCameraInput.enabled = false;
                }
            })
            .AddTo(this);

    }
}
