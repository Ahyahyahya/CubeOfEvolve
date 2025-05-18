using Cysharp.Threading.Tasks;
using R3;
using R3.Triggers;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class ItemBase : MonoBehaviour
{
    // ---------------------------- SerializeField
    [SerializeField, Tooltip("���O")] private string _name;

    [SerializeField, Tooltip("�z����܂ł̑ҋ@����")] private float _delaySecond;
    [SerializeField, Tooltip("�ړ����x")] private float _moveSpeed;
    [SerializeField, Tooltip("Collider")] private Collider _collider;
    [SerializeField, Tooltip("RigidBody")] private Rigidbody _rb;

    // ---------------------------- Property
    public string Name => _name;

    // ---------------------------- UnityMessage
    private async void OnCollisionEnter(Collision collision)
    {
        // �L�����Z�������������Ƃ���v���k
        await UniTask.Delay(TimeSpan.FromSeconds(_delaySecond), cancellationToken: destroyCancellationToken, delayType: DelayType.DeltaTime)
         .SuppressCancellationThrow();

        _collider.isTrigger = true;
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;

        this.UpdateAsObservable()
            .Subscribe(_ =>
            {
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
    private void SuctionProcess()
    {
        var targetPos = ItemDrop.Instance.PlayerObj.transform.position;

        // �x�N�g�����擾
        Vector3 moveForward = targetPos - transform.position;

        // �ړ������ɃX�s�[�h���|����
        _rb.linearVelocity = _moveSpeed * Time.deltaTime * moveForward.normalized + new Vector3(0, _rb.linearVelocity.y, 0);

        // �����x�N�g�����擾
        Quaternion targetRotation = Quaternion.LookRotation(moveForward.normalized);

        // Y���̉�]�̂ݎ擾
        transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
    }
}
