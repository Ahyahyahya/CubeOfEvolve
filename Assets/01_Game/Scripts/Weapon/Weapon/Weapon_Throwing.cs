using Assets.AT;
using UnityEngine;

public class Weapon_Throwing : WeaponBase
{
    [Header("�e")]
    [SerializeField] private Transform _bulletSpawnPos;
    [SerializeField] private Bullet_Bomb _bullet;
    [SerializeField] private float _shootAngle;

    protected override void Attack()
    {
        // �{�[�����ˏo����
        ThrowingBall();

        GameSoundManager.Instance.PlaySFX(_fireSEName, transform, _fireSEName);
    }

    /// <summary>
    /// �{�[�����ˏo����
    /// </summary>
    private void ThrowingBall()
    {
        if (_bullet != null && _layerSearch.NearestTargetObj != null)
        {
            var ball = Instantiate(_bullet, _bulletSpawnPos.position, Quaternion.identity);

            Transform enemy = _layerSearch.NearestTargetObj.transform;
            Rigidbody enemyRb = _layerSearch.NearestTargetObj.GetComponent<Rigidbody>();

            Vector3 targetPosition = enemy.position;
            Vector3 enemyVelocity = enemyRb != null ? enemyRb.linearVelocity : Vector3.zero;

            // ���̑��x�Ŕ�s���Ԃ�\��
            float dummySpeed = 10f; // �K���ȏ����i��Œ����j
            float distance = Vector3.Distance(_bulletSpawnPos.position, targetPosition);
            float flightTime = distance / dummySpeed;

            // �\���ʒu
            Vector3 predictedPosition = targetPosition + enemyVelocity * flightTime;

            // ���ۂ̑��x���Čv�Z
            Vector3 velocity = CalculateVelocity(_bulletSpawnPos.position, predictedPosition, _shootAngle);

            Rigidbody rid = ball.GetComponent<Rigidbody>();
            rid.AddForce(velocity * rid.mass, ForceMode.Impulse);


            ball.Initialize(
                _targetTag,
                _currentAttack);
        }
    }


    /// <summary>
    /// �W�I�ɖ�������ˏo���x�̌v�Z
    /// </summary>
    /// <param name="startPos">�ˏo�J�n���W</param>
    /// <param name="endPos">�W�I�̍��W</param>
    /// <returns>�ˏo���x</returns>
    private Vector3 CalculateVelocity(Vector3 startPos, Vector3 endPos, float angle)
    {
        // �ˏo�p�����W�A���ɕϊ�
        float rad = angle * Mathf.PI / 180;

        // ���������̋���x
        float x = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(endPos.x, endPos.z));

        // ���������̋���y
        float y = startPos.y - endPos.y;

        // �Ε����˂̌����������x�ɂ��ĉ���
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(x, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (x * Mathf.Tan(rad) + y)));

        if (float.IsNaN(speed))
        {
            // �����𖞂����������Z�o�ł��Ȃ����Vector3.zero��Ԃ�
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(endPos.x - startPos.x, x * Mathf.Tan(rad), endPos.z - startPos.z).normalized * speed);
        }
    }
}
