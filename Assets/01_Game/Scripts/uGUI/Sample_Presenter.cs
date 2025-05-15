using R3;
using TMPro;
using UnityEngine;

namespace MVRP.Sample
{
    /// <summary>
    /// Player�̗̑͂�View�ɔ��f����Presenter
    /// </summary>
    public sealed class Sample_Presenter : MonoBehaviour
    {
        // Model
        [SerializeField] private Sample_Model _models;

        // View
        [SerializeField] private Sample_View _views;
        [SerializeField] private TextMeshProUGUI _text;

        private void Start()
        {
            // Player��Health���Ď�
            _models.Health
                .Subscribe(x =>
                {
                    // View�ɔ��f
                    _views.SetValue(x);
                    _text.text = $"{x} / {_models.MaxHealth}";
                }).AddTo(this);
        }
    }
}
