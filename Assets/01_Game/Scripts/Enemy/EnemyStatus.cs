using R3;
using UnityEngine;

public class EnemyStatus : MonoBehaviour, IDamageble
{
    // ---------------------------- SerializeField
    [Header("�X�e�[�^�X")]
    [SerializeField, Tooltip("�ړ����x")] private float _speed;
    [SerializeField, Tooltip("�̗�")] private float _maxHp;

    [Header("�h���b�v")]
    [SerializeField, Tooltip("�h���b�v")] private ItemDrop _itemDrop;

    [Header("�G�t�F�N�g")]
    [SerializeField, Tooltip("���񂾎�")] private GameObject _effect;

    // ---------------------------- Field
    public enum ActionPattern                              // �s���p�^�[��
    {
        IDLE,           // �ҋ@
        MOVE,           // �ړ�
        WAIT,           // �s������U��~
        ATTACK,         // ��~���čU��
        MOVEANDATTACK,  // �ړ����Ȃ���U��
        AVOID,          // ���
    }

    private ReactiveProperty<ActionPattern> _currentAct = new();    // ���݂̍s��

    private ReactiveProperty<float> _hp = new();            // �̗�


    // ---------------------------- Property
    public ReadOnlyReactiveProperty<ActionPattern> CurrentAct => _currentAct;
    public float MaxHp => _maxHp;
    public ReadOnlyReactiveProperty<float> Hp => _hp;
    public float Speed => _speed;


    // ---------------------------- UnityMessage
    private void Awake()
    {
        _currentAct.Value = ActionPattern.WAIT;

        _hp.Value = _maxHp;

        // ���̔���
        _hp.Where(value => value <= 0)
            .Subscribe(value =>
            {
                _itemDrop.DropItemProcess();

                Instantiate(_effect, transform.position, Quaternion.identity);

                Destroy(gameObject);
            })
            .AddTo(this);
    }

    // ---------------------------- PublicMethod
    /// <summary>
    /// �G�l�~�[����������鏈��
    /// </summary>
    public void EnemySpawn()
    {
        _currentAct.Value = ActionPattern.MOVE;
    }


    // ---------------------------- Interface
    public void TakeDamage(float damage)
    {
        _hp.Value -= damage;
    }
}
