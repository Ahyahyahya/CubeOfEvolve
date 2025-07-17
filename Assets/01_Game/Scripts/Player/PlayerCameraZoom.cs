using R3;
using R3.Triggers;
using Unity.Cinemachine;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class PlayerCameraZoom : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField, Tooltip("�v���C���[�J����")] private CinemachineOrbitalFollow _playerCamera;
    [SerializeField, Tooltip("�r���h�J����")] private CinemachineOrbitalFollow _buildCamera;
    [SerializeField, Tooltip("�ő�̔��a")] private float _maxRadius = 20f;
    [SerializeField, Tooltip("�ŏ��̔��a")] private float _minRadius = 1f;
    [SerializeField, Tooltip("���̃X�N���[���ŃY�[�������")] private float _zoomAmount = 10f;
    [SerializeField, Tooltip("���̃Y�[���ɂ����鎞��")] private float _zoomTime = 0.1f;

    // ---------- Field
    // ���ݎg�p���Ă���J����
    private CinemachineOrbitalFollow _currentCamera = null;
    // ���݂�FOV
    private float _currentRadius = 0f;
    // �ڕW��FOV
    private float _targetRadius = 0f;
    // SmoothDamp�p�̕ϐ�
    private float _currentVelocity = 0f;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // �Q�[���X�e�[�g�ɂ���ăJ������؂�ւ���
        GameManager.Instance.CurrentGameState
            .Where(x => x == GameState.BATTLE || x == GameState.BUILD || x == GameState.TUTORIAL)
            .Subscribe(x =>
            {
                // �؂�ւ��鏈��
                if(x == GameState.BATTLE)
                {
                    _currentCamera = _playerCamera;
                }
                else if(x == GameState.BUILD || x == GameState.TUTORIAL)
                {
                    _currentCamera = _buildCamera;
                }

                _currentRadius = _currentCamera.Radius;
                _targetRadius = _currentCamera.Radius;
            })
            .AddTo(this);

        // �}�E�X�z�C�[���ɂ��Y�[������
        InputEventProvider.Zoom
            .Where(x => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.TUTORIAL)
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
            .Where(x => GameManager.Instance.CurrentGameState.CurrentValue == GameState.BATTLE
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.BUILD
            || GameManager.Instance.CurrentGameState.CurrentValue == GameState.TUTORIAL)
            .Where(_ => _currentRadius != _targetRadius)
            .Subscribe(x =>
            {
                // ���炩��FOV��ς���
                _currentRadius = Mathf.SmoothDamp(
                    _currentCamera.Radius,
                    _targetRadius,
                    ref _currentVelocity,
                    _zoomTime,
                    Mathf.Infinity,
                    Time.unscaledDeltaTime);

                // �ς����FOV���Z�b�g����
                _currentCamera.Radius = _currentRadius;
            });
    }
}
