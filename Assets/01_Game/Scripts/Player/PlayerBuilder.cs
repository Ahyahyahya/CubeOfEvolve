using Assets.IGC2025.Scripts.GameManagers;
using R3;
using R3.Triggers;
using TreeEditor;
using UnityEngine;

public class PlayerBuilder : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private SerializableReactiveProperty<GameObject> _selectedWeapon;
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    private CreatePrediction _predictObject = null;
    private Vector3 _createPos;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        var currentState =
            GameManager.Instance.CurrentGameState;

        // �퓬�ɖ߂����烊�Z�b�g
        currentState
            .Where(_ => _predictObject != null)
            .Where(_ => currentState.CurrentValue == GameState.BATTLE)
            .Subscribe(_ =>
            {
                Destroy(_predictObject.gameObject);
            });

        // �ݒu�\������
        this.UpdateAsObservable()
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Subscribe(_ =>
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (!Physics.Raycast(
                    mouseRay.origin,
                    mouseRay.direction * _rayDist,
                    out RaycastHit hit)) return;

                if (hit.collider.TryGetComponent<Cube>(out var cube))
                {
                    _createPos = cube.transform.position + hit.normal;

                    // �����Ώۂ̐����\���X�N���v�g
                    CreatePrediction targetCreatePrediction;

                    // ���킪�I������Ă�����
                    if(_selectedWeapon.Value != null)
                    {
                        targetCreatePrediction =
                        _selectedWeapon.Value.GetComponent<CreatePrediction>();
                    }
                    else
                    {
                        targetCreatePrediction =
                        _cubePrefab.GetComponent<CreatePrediction>();
                    }

                    // �ݒu�\���L���[�u�̑��d�����h�~
                    if (_predictObject == null)
                    {
                        _predictObject = Instantiate(
                            targetCreatePrediction,
                            _createPos,
                            transform.rotation);

                        _predictObject.transform.SetParent(transform);
                    }

                    // �ݒu�\���L���[�u�̈ʒu���X�V
                    _predictObject.transform.position = _createPos;

                    // �אڂ���L���[�u�����邩�`�F�b�N
                    _predictObject?.CheckNeighboringAllCube();
                }
                // �ݒu�\���L���[�u����������Ă�����폜
                else
                {
                    if (_predictObject == null) return;

                    Destroy(_predictObject.gameObject);
                }
            });

        // ���킪�ς������\���L���[�u�X�V
        _selectedWeapon
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _predictObject != null)
            .Subscribe(_ =>
            {
                Destroy(_predictObject.gameObject);
            });

        // ��]����
        InputEventProvider.Move
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotateWeapon(90f * (int)x.y, -90f * (int)x.x, 0f);
            });

        // ��������
        InputEventProvider.Create
            .Where(x => x)
            .Where(_ => currentState.CurrentValue == GameState.BUILD)
            .Where(_ => _predictObject != null)
            .Where(_ => _predictObject.CanCreated.CurrentValue)
            .Subscribe(_ =>
            {
                _predictObject.ActiveWeapon();
                _predictObject = null;
            });
    }

    // ---------- Event
    public void RotateWeapon(float x, float y, float z)
    {
        if (_predictObject == null) return;

        _predictObject.transform.Rotate(new Vector3(x, y, z));
    }
}
