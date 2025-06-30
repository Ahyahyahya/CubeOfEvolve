using UnityEngine;

public class Weapon_Drill : WeaponBase
{
    // ---------------------------- OverrideMethod
    protected override void Attack()
    {
        // LayerSearch �ɂ�錟�����ʂ��g��
        foreach (var obj in _layerSearch.NearestTargetList)
        {
            string layerName = LayerMask.LayerToName(obj.layer);
            if (obj.transform.root.TryGetComponent<IDamageble>(out var damageble)
                && layerName == _targetTag)
            {
                damageble.TakeDamage(_currentAttack);
            }
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
    }
}
