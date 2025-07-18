using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using System.Threading;
using UnityEngine;

/// <summary>
/// �����ړ�����G
/// </summary>
public class EnemyMove_Assault : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _moveDelaySecond;
    [SerializeField] private float _destroyDelaySecond;

    // ---------------------------- Field
    private Vector3 _moveForward;
    private bool _isAssault = false;
    private float _countSecond = 0;

    private CancellationToken _token;

    // ---------------------------- UnityMessage
    private void Awake()
    {
        _token = this.GetCancellationTokenOnDestroy();
    }

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

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState.CurrentValue != Assets.IGC2025.Scripts.GameManagers.GameState.BATTLE)
        {
            return;
        }

        _countSecond += Time.deltaTime;

        if (_countSecond >= _moveDelaySecond)
        {
            if (this != null && gameObject != null)
            {
                _isAssault = true;
            }
        }
        if (_countSecond >= _moveDelaySecond + _destroyDelaySecond)
        {
            if (this != null && gameObject != null)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// ������
    /// </summary>
    public override void Initialize()
    {
        //// �L�����Z�������������Ƃ���v���k
        //await UniTask.Delay(TimeSpan.FromSeconds(_moveDelaySecond), cancellationToken: _token, delayType: DelayType.DeltaTime)
        // .SuppressCancellationThrow();

        //if (this != null && gameObject != null)
        //{
        //    _isAssault = true;
        //}

        //// �L�����Z�������������Ƃ���v���k
        //await UniTask.Delay(TimeSpan.FromSeconds(_destroyDelaySecond), cancellationToken: _token, delayType: DelayType.DeltaTime)
        // .SuppressCancellationThrow();

        //if (this != null && gameObject != null)
        //{
        //    Destroy(gameObject);
        //}
    }
}
