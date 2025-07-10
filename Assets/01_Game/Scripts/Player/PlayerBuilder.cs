using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;

public class PlayerBuilder : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    // �Ώۂ̐����\���X�N���v�g
    private CreatePrediction _targetCreatePrediction;

    // �Ώۂ̐����\���L���[�u
    private CreatePrediction _predictCube = null;
    private Vector3 _createPos;
    private Vector3 _createPosOffset = new Vector3(0f, 0.5f, 0f);
    // ��
    private ModuleData _currentModuleData;

    public bool _isRemoving;

    private GameObject _prevRemoveObject;
    private GameObject _curRemoveObject;

    // �e�������i�[
    private Vector3[] _directions =
    {
        Vector3.up,
        -Vector3.up,
        Vector3.right,
        -Vector3.right,
        Vector3.forward,
        -Vector3.forward
    };

    // �v���C���[�ƌq�����Ă���I�u�W�F�N�g�B
    private List<GameObject> _connectCheckedObjects = new();

    // ---------- R3
    private Subject<ModuleData> _selectModuleData = new();
    public Observable<ModuleData> OnSelectModuleData => _selectModuleData;

    // ---------- UnityMessage
    /// <summary>
    /// UnityMessage��Start()�Ɠ��`
    /// </summary>
    protected override void OnInitialize()
    {
        var currentState =
            GameManager.Instance.CurrentGameState;

        // �I�����ꂽ���W���[����ID����擾����
        _selectModuleData
            .Subscribe(moduleData =>
            {
                // ���ɐ����\���L���[�u����������Ă�����j��
                if (_predictCube != null)
                {
                    Destroy(_predictCube);
                }

                // ���킪�I������Ă�����
                if (moduleData != null)
                {
                    // ���̕���̐����\���X�N���v�g�擾
                    _targetCreatePrediction =
                        moduleData
                        .Model
                        .GetComponent<CreatePrediction>();

                    _currentModuleData = moduleData;
                }
                else
                {
                    // �L���[�u�̐����\���X�N���v�g�擾
                    _targetCreatePrediction =
                        _cubePrefab
                        .GetComponent<CreatePrediction>();

                    _currentModuleData = null;
                }
            })
            .AddTo(this);

        // �퓬�ɖ߂����烊�Z�b�g
        currentState
            .Where(_ => _predictCube != null)
            .Where(_ => currentState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                _targetCreatePrediction = null;
                _currentModuleData = null;
                Destroy(_predictCube.gameObject);
            })
            .AddTo(this);


        // �ݒu�\������
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                // ���C�ɉ���������Ȃ������珈�����Ȃ�
                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                if (!_isRemoving)
                {
                    if (_targetCreatePrediction == null) return;

                    if (_predictCube == null)
                    {
                        _predictCube = Instantiate(
                            _targetCreatePrediction,
                            hit.point,
                            transform.rotation);

                        _predictCube.transform.SetParent(this.transform);
                    }

                    // ���C���L���[�u�����������珈��
                    if (hit.collider.TryGetComponent<Cube>(out var cube))
                    {
                        // �����ʒu�擾
                        _createPos = cube.transform.position + hit.normal;

                        // �ݒu�\���L���[�u�̈ʒu���X�V
                        _predictCube.transform.position = _createPos;

                        // �אڂ���L���[�u�����邩�`�F�b�N
                        _predictCube?.CheckNeighboringAllCube();
                    }
                    // ���C���L���[�u�ɓ�����Ȃ��Ȃ����珈��
                    else
                    {
                        // ���C���y��ɓ������Ă��Ȃ����͐����ł��Ȃ��悤�ɂ���
                        _predictCube.ResistCreate();

                        _predictCube.transform.position = hit.point + _createPosOffset;
                    }
                }
                else
                {
                    // ���C�ɓ������������v���C���[�̕��łȂ�������
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    {
                        // ���O�Ƀv���C���[�̕��ɓ������Ă��Ȃ��Ȃ珈�����Ȃ�
                        if (_curRemoveObject == null) return;

                        // �폜�ΏۂɃ��C��������Ȃ��Ȃ����猳�̐F�ɖ߂�
                        _curRemoveObject
                        .GetComponent<CreatePrediction>()
                        .ChangeNormalColor();

                        // �����C�ɓ������Ă��镨�ϐ���Null�ɂ���
                        _curRemoveObject = null;

                        return;
                    }

                    // ���C�ɓ������Ă�����̂��ς���Ă��Ȃ��Ȃ珈�����Ȃ�
                    if (_curRemoveObject == hit.collider.GetComponentInParent<CreatePrediction>().gameObject) return;

                    // ���O�Ƀv���C���[�̕��ɓ������Ă��Ȃ��Ȃ珈�����Ȃ�
                    if (_curRemoveObject != null)
                    {
                        _curRemoveObject
                        .GetComponent<CreatePrediction>()
                        .ChangeNormalColor();
                    }

                    //
                    _curRemoveObject =
                        hit
                        .collider
                        .GetComponentInParent<CreatePrediction>()
                        .gameObject;

                    _curRemoveObject.GetComponent<CreatePrediction>()
                    .ChangeFalseColor();
                }
            });

        // ��]����
        InputEventProvider.Move
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotatePredictCube(90f * (int)x.y, -90f * (int)x.x, 0f);
            })
            .AddTo(this);

        // �����E�폜����
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                // �������[�h
                if (!_isRemoving)
                {
                    // �\���L���[�u����������Ă��Ȃ��Ȃ珈�����Ȃ�
                    if (_predictCube == null) return;

                    // ���������𖞂����Ă��Ȃ��Ȃ珈�����Ȃ�
                    if (!_predictCube.CanCreated.CurrentValue) return;

                    // ����E�L���[�u�̐ݒu
                    _predictCube.CreateObject();

                    // �����\���L���[�u���k����
                    _predictCube = null;

                    // ����������̂����W���[���̎�
                    if (_currentModuleData != null)
                    {
                        // �I�v�V�����̎�
                        if (_currentModuleData.ModuleType == ModuleData.MODULE_TYPE.Options)
                        {
                            _currentModuleData.Model.GetComponent<OptionBase>().WhenEquipped();
                        }

                        // ���W���[���̏����������炷
                        RuntimeModuleManager.Instance.ChangeModuleQuantity(
                            _currentModuleData.Id,
                            -1);
                    }
                    // ����������̂��L���[�u�̎�
                    else
                    {
                        // �L���[�u�̐ݒu���Ă鐔�𑝂₷
                        Core.AddCube();
                    }
                }
                // �폜���[�h
                else
                {
                    // �폜�Ώۂ����݂��Ȃ��Ȃ珈�����Ȃ�
                    if (_curRemoveObject == null) return;

                    // WeaponBase���p�����Ă����烂�W���[���ƌ��Ȃ�
                    if (_curRemoveObject.TryGetComponent<WeaponBase>(out var weapon))
                    {
                        // �폜�Ώۂ̃��W���[���̏������𑝂₷
                        RuntimeModuleManager.Instance.ChangeModuleQuantity(weapon.Id, 1);
                    }
                    // �����łȂ��Ȃ�L���[�u(�y��)�ƌ��Ȃ�
                    else
                    {
                        // �L���[�u�̐ݒu�������炷
                        Core.RemoveCube();
                    }

                    // �Ώۂ̃I�u�W�F�N�g���폜����
                    Destroy(_curRemoveObject.gameObject);
                }
            })
            .AddTo(this);
    }

    // ---------- PublicMethod
    /// <summary>
    /// �����\���L���[�u����]������
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    public void RotatePredictCube(float x, float y, float z)
    {
        if (_predictCube == null) return;

        _predictCube.transform.Rotate(new Vector3(x, y, z));
    }

    /// <summary>
    /// ���ݑI�����Ă��郂�W���[����ύX����
    /// </summary>
    /// <param name="moduleData"></param>
    public void SetModuleData(ModuleData moduleData)
    {
        _selectModuleData.OnNext(moduleData);
    }

    /// <summary>
    /// ����������̂��L���[�u�ɕύX����
    /// </summary>
    public void SetCube()
    {
        _selectModuleData.OnNext(null);
    }

    /// <summary>
    /// �������[�h�ƍ폜���[�h��؂�ւ���
    /// </summary>
    public void ChangeBuildMode()
    {
        // �폜���t���O�𔽓]
        _isRemoving = !_isRemoving;
    }

    /// <summary>
    /// �v���C���[�ƌq�����Ă��邩�����؂���
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="cubeScale"></param>
    public void ConnectCheck(
       GameObject cube,
       float cubeScale)
    {
        _connectCheckedObjects.Add(cube);

        foreach (var direction in _directions)
        {
            if (Physics.Raycast(
            cube.transform.position,
            direction,
            out RaycastHit hit,
            cubeScale,
            LayerMask.GetMask("Player")))
            {
                // �Ώۂ̃I�u�W�F�N�g�̐����\���X�N���v�g���擾
                var prediction = GetComponentInParent<CreatePrediction>();

                // �����Ȃ珈�����Ȃ�
                if (prediction == null) continue;

                // �L���[�u�X�N���v�g���擾
                var targetCube = prediction.gameObject.GetComponent<Cube>();

                // �Ώۂ̃I�u�W�F�N�g�ł܂����̃X�N���v�g�����s
                //prediction.DestroyCheck(
                //    targetCube,
                //    cubeScale);
            }
        }
    }
}
