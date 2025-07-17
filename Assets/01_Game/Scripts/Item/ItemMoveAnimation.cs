using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using UnityEngine;

public class ItemMoveAnimation : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("�}�e���A��")] private Renderer _material;
    [SerializeField, Tooltip("�F")] private Color _color;

    [SerializeField, Tooltip("�z����܂ł̑ҋ@����")] private float _delaySecond;
    [SerializeField, Tooltip("�ړ����x")] private float _moveSpeed;
    [SerializeField, Tooltip("Collider")] private Collider _collider;
    [SerializeField, Tooltip("RigidBody")] private Rigidbody _rb;

    // ---------------------------- UnityMessage
    private void Awake()
    {
        // �}�e���A�����擾�i���ӁFsharedMaterial �͑S�̋��L�Amaterial �̓C���X�^���X�j
        Material mat = _material.material;

        // Emission�L����
        mat.EnableKeyword("_EMISSION");

        // Emission�̐F��ύX
        mat.SetColor("_EmissionColor", _color);
    }

    private async void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ground")) return;

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.linearVelocity = Vector3.zero;
        }

        // �L�����Z�������������Ƃ���v���k
        await UniTask.Delay(TimeSpan.FromSeconds(_delaySecond), cancellationToken: destroyCancellationToken, delayType: DelayType.DeltaTime)
         .SuppressCancellationThrow();


        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
                if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
                {
                    _rb.linearVelocity = Vector3.zero;
                    return;
                }

                SuctionProcess();
            })
            .AddTo(this);

        this.OnTriggerEnterAsObservable()
            .Subscribe(other =>
            {
                if (other.CompareTag("Player"))
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �v���C���[�ɋz�����܂�鏈��
    /// </summary>
    private void SuctionProcess()
    {
        var targetPos = PlayerMonitoring.Instance.PlayerObj.transform.position;

        // �x�N�g�����擾
        Vector3 moveForward = targetPos - transform.position;

        // �ړ������ɃX�s�[�h���|����
        _rb.linearVelocity = _moveSpeed * Time.deltaTime * moveForward.normalized;
    }
}
