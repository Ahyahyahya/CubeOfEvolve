using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using App.GameSystem.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using System.Collections.Generic;
using R3;
using R3.Triggers;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Assets.AT;

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
    // �I������Ă��郂�W���[���̃f�[�^
    private ModuleData _currentModuleData;
    // �폜���[�h����
    private bool _isRemoving;
    // 1�O�̏����Ώۂ������I�u�W�F�N�g
    private GameObject _prevRemoveObject;
    // ���ݏ����Ώۂ̃I�u�W�F�N�g
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
    // ���ɐ�������Ă���I�u�W�F�N�g�B
    private List<GameObject> _createdObjects = new();
    // �v���C���[�ƌq�����Ă���I�u�W�F�N�g�B
    private List<GameObject> _connectCheckedObjects = new();
    // �v���C���[�ƌq�����ċ��Ȃ������I�u�W�F�N�g�B
    private List<CreatePrediction> _disconnectObjects = new();
    // �v���C���[�R�A�ƌq�����Ă��邩�m�F���Ă���֐��̌��݂̎��s��
    private int _runningConnectCheckCount;

    // ---------- Property
    public List<GameObject> CreatedObjects
    {
        get => _createdObjects;
        set => _createdObjects = value;
    }

    public bool GetIsRemoving => _isRemoving;

    // ---------- R3
    private Subject<ModuleData> _selectModuleData = new();
    public Observable<ModuleData> OnSelectModuleData => _selectModuleData;

    private Subject<Unit> _createSubject = new();
    public Observable<Unit> OnCreate => _createSubject;

    private Subject<Unit> _removeSubject = new();

    public Subject<Unit> OnRemove => _removeSubject;

    // ---------- UnityMessage
    /// <summary>
    /// UnityMessage��Start()�Ɠ��`
    /// </summary>
    protected override void OnInitialize()
    {
        _createSubject.AddTo(this);

        var currentState =
            GameManager.Instance.CurrentGameState;

        // �I�����ꂽ���W���[����ID����擾����
        _selectModuleData
            .Subscribe(moduleData =>
            {
                // ���ɐ����\���L���[�u����������Ă�����j��
                if (_predictCube != null)
                {
                    Destroy(_predictCube.gameObject);
                }

                // �폜���[�h���ɐݒu���������̂��I�����ꂽ��폜���[�h���I����
                if (_isRemoving)
                {
                    _isRemoving = false;
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
                _isRemoving = false;
                _targetCreatePrediction = null;
                _currentModuleData = null;
                Destroy(_predictCube.gameObject);
            })
            .AddTo(this);


        // �����E�폜�\������
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD || currentState.CurrentValue == GameState.TUTORIAL)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                // ���C�ɉ���������Ȃ������珈�����Ȃ�
                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                // �����\������
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

                        // �L���[�u�̐ݒu����𒴂��Ă��Ȃ������W���[����I�����Ă�����
                        if (Core.CubeCount.CurrentValue < Core.MaxCubeCount.CurrentValue
                        || _currentModuleData != null)
                        {
                            // �אڂ���L���[�u�����邩�`�F�b�N
                            _predictCube?.CheckCanCreate();
                        }
                        else
                        {
                            _predictCube.ResistCreate();
                        }

                    }
                    // ���C���L���[�u�ɓ�����Ȃ��Ȃ����珈��
                    else
                    {
                        // ���C���y��ɓ������Ă��Ȃ����͐����ł��Ȃ��悤�ɂ���
                        _predictCube.ResistCreate();

                        _predictCube.transform.position = hit.point + _createPosOffset;
                    }
                }
                // �폜�\������
                else
                {
                    // ���C�ɓ������������v���C���[�̕��łȂ�������
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                    {
                        // �q�����Ă��Ȃ������X�g�����Z�b�g
                        ResetDisconnectObjects();

                        // �����C�ɓ������Ă��镨�ϐ���Null�ɂ���
                        _curRemoveObject = null;

                        return;
                    }

                    // ���C�ɓ������������v���C���[�R�A�Ȃ珈�����Ȃ�
                    if (hit.collider.gameObject == this.gameObject) return;

                    // ���C�ɓ������Ă�����̂��ς���Ă��Ȃ��Ȃ珈�����Ȃ�
                    if (_curRemoveObject
                    == hit.collider.GetComponentInParent<CreatePrediction>().gameObject) return;

                    // ���݂̍폜�Ώۂ��X�V
                    _curRemoveObject =
                        hit
                        .collider
                        .GetComponentInParent<CreatePrediction>()
                        .gameObject;

                    // �q�����Ă��Ȃ������X�g�����Z�b�g
                    ResetDisconnectObjects();

                    // �����ς݂̕����폜����q�����Ă��邩�m�F
                    ConnectCheck(this.gameObject, 1f);
                }
            });

        // ��]����
        InputEventProvider.Move
            .Where(_ => currentState.CurrentValue == GameState.BUILD || currentState.CurrentValue == GameState.TUTORIAL)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotatePredictCube(90f * (int)x.y, -90f * (int)x.x, 0f);
            })
            .AddTo(this);

        // �����E�폜����
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD || currentState.CurrentValue == GameState.TUTORIAL)
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

                    _createdObjects.Add(_predictCube.gameObject);

                    // �����\���L���[�u���k����
                    _predictCube = null;

                    // �����������Ƃ�ʒm����
                    _createSubject.OnNext(Unit.Default);

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

                        var curRuntimeModuleData =
                            RuntimeModuleManager
                            .Instance
                            .GetRuntimeModuleData(_currentModuleData.Id);

                        // �I�����Ă��郂�W���[���̏�������0�ȉ��Ȃ�I����������
                        if (curRuntimeModuleData.Quantity.CurrentValue <= 0)
                        {
                            _targetCreatePrediction = null;
                            _currentModuleData = null;
                        }
                    }
                    // ����������̂��L���[�u�̎�
                    else
                    {
                        // ���ݐݒu���Ă���L���[�u��������ȏ�Ȃ珈�����Ȃ�
                        if (Core.CubeCount.CurrentValue > Core.MaxCubeCount.CurrentValue) return;
                        // �L���[�u�̐ݒu���Ă鐔�𑝂₷
                        Core.AddCube();
                    }
                }
                // �폜���[�h
                else
                {
                    // �폜�Ώۂ����݂��Ȃ��Ȃ珈�����Ȃ�
                    if (_curRemoveObject == null) return;

                    // �q�����Ă��Ȃ��I�u�W�F�N�g�B���폜
                    RemoveDisconnectObjects();
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

        // �폜���[�h�Ɉڍs����������Ώۂ����Z�b�g����
        if (_isRemoving )
        {
            _targetCreatePrediction = null;
            _currentModuleData = null;
            Destroy(_predictCube.gameObject);
        }
    }

    /// <summary>
    /// �v���C���[�ƌq�����Ă��邩�����؂���
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="cubeScale"></param>
    private void ConnectCheck(
       GameObject cube,
       float cubeScale)
    {
        // ���ݎ��s����Ă���֐��̐������Z
        _runningConnectCheckCount++;

        // �v���C���[�ƌq�����Ă��邩�`�F�b�N�ς݃��X�g�ɒǉ�
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
                var prediction = hit.collider.gameObject.GetComponentInParent<CreatePrediction>();

                // �����Ȃ珈�����Ȃ�
                if (prediction == null) continue;

                // �Ώۂ��폜�������Ɍq�����Ă��邩�m�F�������̂ōŏ��̍폜�Ώۂ͏������Ȃ�
                if (prediction.gameObject == _curRemoveObject) continue;

                // ���Ƀ`�F�b�N�ς݂̂��̂Ȃ珈�����Ȃ�
                if (_connectCheckedObjects.Contains(prediction.gameObject)) continue;

                // ���ׂẴL���[�u�ɗאڂ��Ă��Ȃ��Ȃ����
                if (!prediction.CheckNeighboringAllCube())
                {
                    Destroy(prediction.gameObject);
                    continue;
                }

                // �Ώۂ̃I�u�W�F�N�g�ł܂����̃X�N���v�g�����s
                ConnectCheck(prediction.gameObject, cubeScale);
            }
        }

        // ���ݎ��s����Ă���֐��̐������Z
        _runningConnectCheckCount--;

        // ���ݎ��s����Ă���֐��������Ȃ����珈��
        if (_runningConnectCheckCount <= 0)
        {
            SearchDisconnectedObjects();
        }
    }

    /// <summary>
    /// �v���C���[�ƌq�����Ă��Ȃ����W���[��/�L���[�u��􂢏o��
    /// </summary>
    private void SearchDisconnectedObjects()
    {
        foreach (var createdObject in _createdObjects)
        {
            // Null�Ȃ珈�����Ȃ�
            if (createdObject == null) continue;
            // �v���C���[�ƌq�����Ă����珈�����Ȃ�
            if (_connectCheckedObjects.Contains(createdObject)) continue;
            // ���݂̑Ώۂ�CreatePrediction���擾
            var curCreatePrediction = createdObject.GetComponent<CreatePrediction>();
            // �q�����Ă��Ȃ����X�g�ɒǉ�
            _disconnectObjects.Add(curCreatePrediction);
            // ������F�ɕύX
            curCreatePrediction.ChangeFalseMaterial();
        }

        // �v���C���[�Ƃ̐ڑ��m�F�p�̃��X�g��������
        _connectCheckedObjects.Clear();
    }

    /// <summary>
    /// �v���C���[�R�A�ƌq�����Ă��Ȃ��I�u�W�F�N�g���폜
    /// </summary>
    private void RemoveDisconnectObjects()
    {
        foreach(var removeTarget in _disconnectObjects)
        {
            // �폜�Ώۂ�ݒu�ς݃I�u�W�F�N�g���X�g��������Ă���(Null�΍�)
            _createdObjects.Remove(removeTarget.gameObject);

            // �Ώۂ��폜
            RemoveObject(removeTarget.gameObject);
        }

        // �폜�C�x���g��ʒm
        OnRemove.OnNext(Unit.Default);

        // �q�����Ă��Ȃ��I�u�W�F�N�g���X�g�����
        ResetDisconnectObjects();
    }

    /// <summary>
    /// �q�����Ă��Ȃ��I�u�W�F�N�g���X�g�����Z�b�g
    /// </summary>
    private void ResetDisconnectObjects()
    {
        // ������F�ɂȂ��Ă����������̐F�ɖ߂�
        foreach( var disconnectObject in _disconnectObjects)
        {
            // ���g�����������珈�����Ȃ�
            if (disconnectObject == null) continue;

            disconnectObject.ChangeNormalMaterial();
        }

        // �q�����Ă��Ȃ��I�u�W�F�N�g���X�g�����Z�b�g
        _disconnectObjects.Clear();
    }

    /// <summary>
    /// �ݒu�ς݂̃��W���[���E�L���[�u������
    /// </summary>
    /// <param name="gameObject"></param>
    private void RemoveObject(GameObject gameObject)
    {
        // WeaponBase���p�����Ă����烂�W���[���ƌ��Ȃ�
        if (gameObject.TryGetComponent<IModuleID>(out var module))
        {
            RuntimeModuleManager.Instance.ChangeModuleQuantity(module.Id, 1);

            if (gameObject.TryGetComponent<OptionBase>(out var option))
            {
                option.ProcessingWhenRemoved();
            }
        }
        else
        {
            Core.RemoveCube();
        }

        Destroy(gameObject);
    }

    public void RemoveAllObjects()
    {
        foreach (var obj in CreatedObjects)
        {
            RemoveObject(obj);
        }

        CreatedObjects.Clear();
    }

}
