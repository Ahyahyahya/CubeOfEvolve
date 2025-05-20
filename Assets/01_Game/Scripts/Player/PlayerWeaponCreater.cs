using R3;
using R3.Triggers;
using UnityEngine;

public class PlayerWeaponCreater : BasePlayerComponent
{
    // ---------- SerializeField
    [SerializeField] private WeaponCreatePrediction _weaponPrefab;
    [SerializeField] private float _rayDist = 50f;

    // ---------- Field
    public WeaponCreatePrediction _predictWeapon = null;
    private Vector3 _createPos;

    // ---------- UnityMessage
    protected override void OnInitialize()
    {
        // �ݒu�\������
        this.UpdateAsObservable()
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

                    // �ݒu�\���L���[�u�̑��d�����h�~
                    if (_predictWeapon == null)
                    {
                        _predictWeapon = Instantiate(_weaponPrefab, _createPos, transform.rotation);

                        _predictWeapon.transform.SetParent(transform);
                    }

                    // �ݒu�\���L���[�u�̈ʒu���X�V
                    _predictWeapon.transform.position = _createPos;

                    // �אڂ���L���[�u�����邩�`�F�b�N
                    _predictWeapon.CheckNeighboringAllCube();
                }
                // �ݒu�\���L���[�u����������Ă�����폜
                else
                {
                    if (_predictWeapon == null) return;

                    Destroy(_predictWeapon.gameObject);
                }
            });

        // ��]����
        InputEventProvider.Move
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                RotateWeapon(90f * (int)x.y, -90f * (int)x.x, 0f);
            });

        // ��������
        InputEventProvider.Create
            .Where(x => x)
            .Where(x => _predictWeapon.CanCreated.CurrentValue)
            .Subscribe(_ =>
            {
                _predictWeapon.ActiveWeapon();
                _predictWeapon = null;
            });
    }

    // ---------- Event
    public void RotateWeapon(float x, float y, float z)
    {
        if (_predictWeapon == null) return;

        _predictWeapon.transform.Rotate(new Vector3(x, y, z));
    }
}
