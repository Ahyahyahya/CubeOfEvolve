using UnityEngine;

[RequireComponent(typeof(PlayerCore))]
public abstract class BasePlayerComponent : MonoBehaviour
{
    private IInputEventProvider _inputEventProvider;

    // �e�R���|�[�l���g�ł悭�g������̃R���|�[�l���g
    protected IInputEventProvider InputEventProvider => _inputEventProvider;
    protected PlayerCore Core;

    // ---------- UnityMessage
    private void Start()
    {
        Core = GetComponent<PlayerCore>();
        _inputEventProvider = GetComponent<IInputEventProvider>();
        OnInitialize();
    }

    // ---------- Method
    protected abstract void OnInitialize();
}
