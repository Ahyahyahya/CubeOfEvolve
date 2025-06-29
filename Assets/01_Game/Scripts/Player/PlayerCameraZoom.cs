using R3;
using R3.Triggers;
using Unity.Cinemachine;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraZoom : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("�v���C���[�J����")] private CinemachineCamera _playerCamera;
    [SerializeField, Tooltip("�r���h�J����")] private CinemachineCamera _buildCamera;
    [SerializeField, Tooltip("�ő��FOV")] private float _maxFov = 90f;
    [SerializeField, Tooltip("�ŏ���FOV")] private float _minFov = 10f;
    [SerializeField, Tooltip("���̃X�N���[���ŃY�[�������")] private float _zoomAmount = 10f;
    [SerializeField, Tooltip("���̃Y�[���ɂ����鎞��")] private float _zoomTime = 0.1f;

    // ---------- Field
    // ���ݎg�p���Ă���J����
    private CinemachineCamera _currentCamera = null;
    // ���݂�FOV
    private float _currentFOV = 0f;
    // �ڕW��FOV
    private float _targetFOV = 0f;
    // SmoothDamp�p�̕ϐ�
    private float _currentVelocity = 0f;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // �Q�[���X�e�[�g�ɂ���ăJ������؂�ւ���
        GameManager.Instance.CurrentGameState
            .Where(x => x == GameState.BATTLE || x == GameState.BUILD)
            .Subscribe(x =>
            {
                // �؂�ւ��鏈��
                if(x == GameState.BATTLE)
                {
                    _currentCamera = _playerCamera;
                }
                else if(x == GameState.BUILD)
                {
                    _currentCamera = _buildCamera;
                }

                // �؂�ւ����J�����ɂ���ď���������
                _currentFOV = _currentCamera.Lens.FieldOfView;
                _targetFOV = _currentFOV;
            })
            .AddTo(this);

        // �}�E�X�z�C�[���ɂ��Y�[������
        InputEventProvider.Zoom
            .Where(x => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD)
            .Subscribe(x =>
            {
                // �O�X�N���[����
                if(x > 0)
                {
                    // �Y�[���C��
                    if (_targetFOV + -x * _zoomAmount >= _minFov)
                    {
                        _targetFOV += -x * _zoomAmount;
                    }
                    // �ŏ��l�������Ȃ��悤��
                    else
                    {
                        _targetFOV = _minFov;
                    }
                }
                // ���X�N���[����
                else if(x < 0)
                {
                    // �Y�[���A�E�g
                    if (_targetFOV + -x * _zoomAmount <= _maxFov)
                    {
                        _targetFOV += -x * _zoomAmount;
                    }
                    // �ő�l�𒴂��Ȃ��悤��
                    else
                    {
                        _targetFOV = _maxFov;
                    }
                }
            })
            .AddTo(this);

        // ���ۂ̃Y�[������
        this.UpdateAsObservable()
            .Where(x => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD)
            .Where(_ => _currentFOV != _targetFOV)
            .Subscribe(x =>
            {
                // ���炩��FOV��ς���
                _currentFOV = Mathf.SmoothDamp(
                    _currentCamera.Lens.FieldOfView,
                    _targetFOV,
                    ref _currentVelocity,
                    _zoomTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

                // �ς����FOV���Z�b�g����
                _currentCamera.Lens.FieldOfView = _currentFOV;
            });
    }
}
