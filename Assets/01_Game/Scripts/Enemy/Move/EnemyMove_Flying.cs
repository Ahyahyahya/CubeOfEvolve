using UnityEngine;

/// <summary>
/// ��ԓG
/// </summary>
public class EnemyMove_Flying : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _minDistance;
    [SerializeField] private float _minFlyingHeight;
    [SerializeField] private float _maxFlyingHeight;
    [SerializeField] private float _flyingPower;

    [SerializeField] private LayerMask _layerMask;

    // ---------------------------- OverrideMethod
    public override void Move()
    {
        if (Vector3.Distance(_targetObj.transform.position, transform.position) >= _minDistance)
        {
            LinearMovement();
        }

        Flying();
    }

    // ---------------------------- PrivateMethod
    /// <summary>
    /// ��ԏ���
    /// </summary>
    private void Flying()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, _layerMask))
        {
            float dis = transform.position.y - hit.point.y;

            if (dis < _minFlyingHeight)
            {
                // ���x���Ⴗ����̂ŏ㏸
                _rb.AddForce(Vector3.up * _flyingPower, ForceMode.Impulse);
            }
            else if (dis > _maxFlyingHeight)
            {
                // ���x����������̂ŏ㏸���~�߂�
                _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            }
        }
    }

}
