using R3;
using UnityEngine;
using Assets.IGC2025.Scripts.GameManagers;

public class StopEffect : MonoBehaviour
{
    // ---------------------------- Field
    private ParticleSystem _particleSystem;
    private Vector3 _startScale;

    // ---------------------------- UnityMessage
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        _startScale = gameObject.transform.localScale;

        GameManager.Instance.CurrentGameState
            .Subscribe(value =>
            {
                if (value == GameState.BATTLE || value == GameState.GAMECLEAR)
                {
                    _particleSystem.Play();
                    gameObject.transform.localScale = _startScale;
                }
                else
                {
                    _particleSystem.Pause();
                    gameObject.transform.localScale = Vector3.zero;
                }
            })
            .AddTo(_particleSystem);
    }
}
