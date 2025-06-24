using App.BaseSystem.DataStores.ScriptableObjects.Modules;
using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using UnityEngine;

public class PlayerBuilder : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private ModuleDataStore _moduleDataStore;
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    // �Ώۂ̐����\���X�N���v�g
    private CreatePrediction _targetCreatePrediction;

    // �Ώۂ̐����\���L���[�u
    private CreatePrediction _predictCube = null;
    private Vector3 _createPos;
    // ��
    private ModuleData _currentModuleData;

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
                }
            })
            .AddTo(this);

        // �퓬�ɖ߂����烊�Z�b�g
        currentState
            .Where(_ => _predictCube != null)
            .Where(_ => currentState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                Destroy(_predictCube.gameObject);
            })
            .AddTo(this);

        // �ݒu�\������
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _targetCreatePrediction != null)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                // ���C�ɉ���������Ȃ������珈�����Ȃ�
                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                // ���C���L���[�u�����������珈��
                if (hit.collider.TryGetComponent<Cube>(out var cube))
                {
                    // �����ʒu�擾
                    _createPos = cube.transform.position + hit.normal;

                    // �ݒu�\���L���[�u�̑��d�����h�~
                    if (_predictCube == null)
                    {
                        _predictCube = Instantiate(
                            _targetCreatePrediction,
                            _createPos,
                            transform.rotation);

                        _predictCube.transform.SetParent(transform);
                    }

                    // �ݒu�\���L���[�u�̈ʒu���X�V
                    _predictCube.transform.position = _createPos;

                    // �אڂ���L���[�u�����邩�`�F�b�N
                    _predictCube?.CheckNeighboringAllCube();
                }
                // ���C���L���[�u�ɓ�����Ȃ��Ȃ����珈��
                else
                {
                    if (_predictCube == null) return;

                    Destroy(_predictCube.gameObject);
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

        // ��������
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _predictCube != null)
            .Where(_ => _predictCube.CanCreated.CurrentValue)
            .Subscribe(_ =>
            {
                _predictCube.CreateObject();
                _predictCube = null;

                // �I�v�V�����̎�
                if (_currentModuleData != null
                && _currentModuleData.ModuleType == ModuleData.MODULE_TYPE.Options)
                {
                    _currentModuleData.Model.GetComponent<OptionBase>().WhenEquipped();
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
}
