using Assets.IGC2025.Scripts.Event;
using Assets.AT;
using R3;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

namespace Assets.IGC2025.Scripts.Presenter
{
    /// <summary>
    /// Player�̗̑͂�View�ɔ��f����Presenter
    /// </summary>
    public sealed class PresenterGameCanvas : MonoBehaviour
    {
        [Header("Models")]
        [SerializeField] private PlayerCore _models;

        [Header("Views")]
        [SerializeField] private TextMeshProUGUI _timeText;
        [SerializeField] private SliderAnimation _hpSliderAnimation;
        [SerializeField] private SliderAnimation _expSliderAnimation;
        [SerializeField] private EventLevelUp _levelUp;
        [SerializeField] private TextScaleAnimation _cubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _maxCubeCountTextScaleAnimation;
        [SerializeField] private TextScaleAnimation _moneyTextScaleAnimation;

        private TimeManager _timeManager;

        private void Start()
        {
            _timeManager = GameManager.Instance.GetComponent<TimeManager>();
            _timeManager.CurrentTimeSecond
                .Subscribe(x =>
                {
                    _timeText.text = $"{(x).ToString("F1")}";
                })
                .AddTo(this);

            // Player��Health���Ď�
            _models.Hp
                .Subscribe(x =>
                {
                    // View�ɔ��f
                    _hpSliderAnimation.SliderAnime(x);
                }).AddTo(this);

            // Player�̌o���l���Ď�
            _models.Exp
                .Subscribe(x =>
                {
                    // View�ɔ��f
                    _expSliderAnimation.SliderAnime((float)x);
                }).AddTo(this);

            // Player�̌o���l���Ď�
            _models.Level
                .Subscribe(_ =>
                {
                    // Event���s
                    _levelUp.PlayLevelUpEvent();
                }).AddTo(this);

            // Player�̏����L���[�u�����Ď�
            _models.CubeCount
                .Subscribe(x =>
                {
                    // View�ɔ��f
                    _cubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // Player�̏����L���[�u�����Ď�
            _models.MaxCubeCount
                .Subscribe(x =>
                {
                    // View�ɔ��f
                    _maxCubeCountTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);

            // Player�̏��������Ď�
            _models.Money
                .Subscribe(x =>
                {
                    // View�ɔ��f
                    _moneyTextScaleAnimation.AnimateFloatAndText(x, 1f);
                }).AddTo(this);
        }
    }
}
