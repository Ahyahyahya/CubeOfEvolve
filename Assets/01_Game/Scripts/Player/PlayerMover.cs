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
        _rb = GetComponent<Rigidbody>();

        InputEventProvider.Move
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
                moveDirection *  Core.MoveSpeed.CurrentValue +
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
    }
}
