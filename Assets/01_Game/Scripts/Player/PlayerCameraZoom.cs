using R3;
using R3.Triggers;
using Unity.Cinemachine;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraZoom : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("�v���C���[�J����")] private CinemachineOrbitalFollow _playerCamera;
    [SerializeField, Tooltip("�ő�̔��a")] private float _maxRadius = 20f;
    [SerializeField, Tooltip("�ŏ��̔��a")] private float _minRadius = 1f;
    [SerializeField, Tooltip("���̃X�N���[���ŃY�[�������")] private float _zoomAmount = 10f;
    [SerializeField, Tooltip("���̃Y�[���ɂ����鎞��")] private float _zoomTime = 0.1f;

    // ---------- Field
    // ���݂̔��a
    private float _currentRadius = 0f;
    // �ڕW�̔��a
    private float _targetRadius = 0f;
    // SmoothDamp�p�̕ϐ�
    private float _currentVelocity = 0f;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // ���݂̃Q�[���X�e�[�g��RP���擾
        var currentGameState = GameManager.Instance.CurrentGameState;

        // �����̔��a���擾
        _currentRadius = _playerCamera.Radius;
        _targetRadius = _currentRadius;

        // �}�E�X�z�C�[���ɂ��Y�[������
        InputEventProvider.Zoom
            .Where(x => currentGameState.CurrentValue == GameState.BATTLE
            || currentGameState.CurrentValue == GameState.BUILD
            || currentGameState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // �O�X�N���[����
                if(x > 0)
                {
                    // �Y�[���C��
                    if (_targetRadius + -x * _zoomAmount >= _minRadius)
                    {
                        _targetRadius += -x * _zoomAmount;
                    }
                    // �ŏ��l�������Ȃ��悤��
                    else
                    {
                        _targetRadius = _minRadius;
                    }
                }
                // ���X�N���[����
                else if(x < 0)
                {
                    // �Y�[���A�E�g
                    if (_targetRadius + -x * _zoomAmount <= _maxRadius)
                    {
                        _targetRadius += -x * _zoomAmount;
                    }
                    // �ő�l�𒴂��Ȃ��悤��
                    else
                    {
                        _targetRadius = _maxRadius;
                    }
                }
            })
            .AddTo(this);

        // ���ۂ̃Y�[������
        this.UpdateAsObservable()
            .Where(x => currentGameState.CurrentValue == GameState.BATTLE
            || currentGameState.CurrentValue == GameState.BUILD
            || currentGameState.CurrentValue == GameState.TUTORIAL)
            .Where(_ => _currentRadius != _targetRadius)
            .Subscribe(x =>
            {
                // ���炩��FOV��ς���
                _currentRadius = Mathf.SmoothDamp(
                    _playerCamera.Radius,
                    _targetRadius,
                    ref _currentVelocity,
                    _zoomTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

                // �ς����FOV���Z�b�g����
                _playerCamera.Radius = _currentRadius;
            });
    }
}
