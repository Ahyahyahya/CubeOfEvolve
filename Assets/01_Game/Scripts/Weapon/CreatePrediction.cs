using R3;
using System.Collections.Generic;
using UnityEngine;

public class CreatePrediction : MonoBehaviour
{
    // ---------- SerializeField
    [SerializeField, Tooltip("���f���̓����蔻��̃L���[�u�B")]
    private List<Cube> _cubes = new();
    [SerializeField, Tooltip("���f���̑S�Ẵ����_���[")]
    private List<Renderer> _renderer = new();
    [SerializeField, Tooltip("�ݒu�\�������F�̃}�e���A��")]
    private Material _trueMaterial;
    [SerializeField, Tooltip("�ݒu�s�\�������F�̃}�e���A��")]
    private Material _falseMaterial;

    // ---------- Field
    // ���f���̒ʏ�̃}�e���A���B
    private List<Material> _normalMaterials = new List<Material>();

    // ---------- R3
    [SerializeField]
    private SerializableReactiveProperty<bool> _isActived;
    public ReadOnlyReactiveProperty<bool> IsActived => _isActived;

    private ReactiveProperty<bool> _canCreated = new();
    public ReadOnlyReactiveProperty<bool> CanCreated => _canCreated;

    private Vector3[] _directions =
    {
        Vector3.up,
        -Vector3.up,
        Vector3.right,
        -Vector3.right,
        Vector3.forward,
        -Vector3.forward
    };

    // ---------- UnityMessage
    private void Start()
    {
        // �ʏ�̃}�e���A���ɖ߂����߂ɑS�Ẵ����_���[�̃}�e���A���擾
        foreach (var renderer in _renderer)
        {
            _normalMaterials.Add(renderer.material);
        }

        // �ݒu���o���邩�ŐF��ς���
        _canCreated
            .Where(_ => !_isActived.Value)
            .Subscribe(x =>
            {
                foreach (var renderer in _renderer)
                {
                    if (x)
                    {
                        renderer.material = _trueMaterial;
                    }
                    else
                    {
                        renderer.material = _falseMaterial;
                    }
                }
            })
            .AddTo(this);

        // �ݒu��폜����
        _isActived
            .Skip(1)
            .Subscribe(x =>
            {
                if (x)
                {
                    // ���f���̃}�e���A����ʏ�̂��̖߂�
                    ChangeNormalMaterial();

                    // �����蔻����I���ɂ���
                    foreach (var cube in _cubes)
                    {
                        cube.GetComponent<BoxCollider>().enabled = true;
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------- PrivateMethod
    /// <summary>
    /// �����\�t���O��ς��Ȃ���אڃ`�F�b�N
    /// </summary>
    /// <returns></returns>
    public void CheckCanCreate()
    {
        _canCreated.Value = CheckNeighboringAllCube();
    }

    /// <summary>
    /// �S�ẴL���[�u���אڂ��Ă��邩�`�F�b�N
    /// </summary>
    /// <returns></returns>
    public bool CheckNeighboringAllCube()
    {
        foreach (var cube in _cubes)
        {
            if (CheckNeighboringCube(cube, 1f)) continue;

            return false;
        }

        return true;
    }

    /// <summary>
    /// �L���[�u���אڂ��Ă��邩�`�F�b�N
    /// </summary>
    /// <param name="cube">�Ώۂ̃L���[�u</param>
    /// <param name="cubeScale">�L���[�u�̈�ӂ̒���</param>
    /// <returns></returns>
    private bool CheckNeighboringCube(
    Cube cube,
    float cubeScale)
    {
        foreach (var direction in _directions)
        {
            if (Physics.Raycast(
            cube.transform.position,
            direction,
            out RaycastHit hit,
            cubeScale))
            {
                if (!hit.collider.CompareTag("Cube")) continue;

                // 0.45f�͗אڂ��Ă���L���[�u�����O�����
                var halfScale = cubeScale * 0.45f;

                // �Ώۂ̃L���[�u�ɂ߂荞��ł���R���C�_�[��
                var cubeInsideColliders = Physics.OverlapBox(
                    cube.transform.position,
                    new Vector3(halfScale, halfScale, halfScale),
                    cube.transform.rotation,
                    LayerMask.GetMask("Player"));

                // ���̐���0���傫���Ȃ�d�Ȃ��Ă���
                if (cubeInsideColliders.Length > 0)
                {
                    // �������g���d�Ȃ��Ă��锻��ɂȂ�̂�h�~
                    if (cubeInsideColliders[0] != cube.GetComponent<Collider>())
                    {
                        return false;
                    }
                }

                return true;
            };
        }
        return false;
    }

    // ---------- Event
    /// <summary>
    /// ���̃X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g�𐶐�����
    /// </summary>
    public void CreateObject()
    {
        _isActived.Value = true;
    }

    /// <summary>
    /// ���̃X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g����������
    /// </summary>
    public void RemoveObject()
    {
        _isActived.Value = false;
    }

    /// <summary>
    /// �\�����Ȃ����ɐ����o���Ă��܂��̂�h���֐�
    /// </summary>
    public void ResistCreate()
    {
        // �����ł��Ȃ��悤�ɂ���
        _canCreated.Value = false;
    }

    /// <summary>
    /// ���f���̃}�e���A����ʏ�̂��̖߂�
    /// </summary>
    public void ChangeNormalMaterial()
    {
        for (int i = 0; i < _renderer.Count; i++)
        {
            _renderer[i].material = _normalMaterials[i];
        }
    }

    /// <summary>
    /// ���̃X�N���v�g���A�^�b�`����Ă���I�u�W�F�N�g��F��s�\�������F�ɕς���
    /// </summary>
    public void ChangeFalseMaterial()
    {
        foreach (var renderer in _renderer)
        {
            renderer.material = _falseMaterial;
        }
    }
}
