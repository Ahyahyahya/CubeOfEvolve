using UnityEngine;

/// <summary>
/// �ʏ�̒��i���Ă���G
/// </summary>
public class EnemyMove_Ground : EnemyMoveBase
{
    // ---------------------------- SerializeField
    [SerializeField] private float _minDistance;

    // ---------------------------- OverrideMethod
    public override void Move()
    {
        if (Vector3.Distance(_targetObj.transform.position, transform.position) >= _minDistance)
        {
            LinearMovement();
        }
    }
}
