using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

/// <summary>
/// �����ړ�����G
/// </summary>
public class EnemyMove_Assault : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _moveDelaySecond;
    [SerializeField] private float _destroyDelaySecond;

    // ---------------------------- SerializeField
    private Vector3 _moveForward;
    private bool _isAssault = false;

    // ---------------------------- PrivateMethod
    /// <summary>
    /// �ˌ�����
    /// </summary>
    private void Assault()
    {
        // �ړ������ɃX�s�[�h���|����
        _rb.linearVelocity = _status.Speed * Time.deltaTime * _moveForward.normalized + new Vector3(0, _rb.linearVelocity.y, 0);
    }

    // ---------------------------- OverrideMethod
    public override void Move()
    {
        if (_isAssault)
        {
            Assault();
        }
        else
        {
            // �G����Ώۂւ̃x�N�g�����擾
            _moveForward = _targetObj.transform.position - transform.position;

            // �����͒ǂ�Ȃ�
            _moveForward.y = 0;

            // �L�����N�^�[�̌�����i�s�����Ɍ�����
            if (_moveForward != Vector3.zero)
            {
                // �����x�N�g�����擾
                Vector3 direction = _targetObj.transform.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

                // Y���̉�]�̂ݎ擾
                transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            }
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    public override async void Initialize()
    {
        // �L�����Z�������������Ƃ���v���k
        await UniTask.Delay(TimeSpan.FromSeconds(_moveDelaySecond), cancellationToken: destroyCancellationToken, delayType: DelayType.DeltaTime)
         .SuppressCancellationThrow();

        if (this != null && gameObject != null)
        {
            _isAssault = true;
        }

        // �L�����Z�������������Ƃ���v���k
        await UniTask.Delay(TimeSpan.FromSeconds(_destroyDelaySecond), cancellationToken: destroyCancellationToken, delayType: DelayType.DeltaTime)
         .SuppressCancellationThrow();

        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
