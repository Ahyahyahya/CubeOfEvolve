using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AT
{
    public class LevelUpEffectController : MonoBehaviour
    {
        [Header("Models")]
        [SerializeField] private PlayerCore _playerCore;

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private Image glowImage;
        [SerializeField] private ParticleSystem levelUpParticle;

        //[Header("Audio")]
        //[SerializeField] private AudioSource audioSource;
        //[SerializeField] private AudioClip levelUpSE;

        private Tween _glowRotateTween;
        private CancellationTokenSource _cts;

        private void Start()
        {
            _playerCore.Level
                .Pairwise()
                .Where(pair => pair.Previous < pair.Current)
                .Subscribe(_ => PlayAsync().Forget());
        }

        public void Trigger()
        {
            PlayAsync().Forget();
        }


        public async UniTask PlayAsync()
        {
            // �����̍Đ��𒆒f
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // �p�[�e�B�N�����o�i�����j
            levelUpParticle?.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            levelUpParticle?.Play();

            // Tween���f
            _glowRotateTween?.Kill();

            // �p�l��������
            panel.SetActive(false);
            panel.SetActive(true);

            levelUpText.color = new Color(1, 1, 1, 0);
            levelUpText.transform.localScale = Vector3.zero;
            glowImage.transform.rotation = Quaternion.identity;

            // ���Đ�
            GameSoundManager.Instance.PlaySE("Sys_LvUp", "System");

            // ��]�J�n�i���[�v�j
            _glowRotateTween = glowImage.transform
                .DORotate(new Vector3(0, 0, 360), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);

            // �e�L�X�g�A�j���[�V�����i�t�F�[�h�{�X�P�[���j
            var fadeTween = levelUpText.DOFade(1f, 0.5f);
            var scaleTween = levelUpText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

            await UniTask.WhenAll(
                fadeTween.ToUniTask(cancellationToken: token),
                scaleTween.ToUniTask(cancellationToken: token)
            );

            // ��莞�ԕ\��
            await UniTask.Delay(1500, cancellationToken: token);

            // �t�F�[�h�A�E�g
            await levelUpText.DOFade(0f, 0.5f).ToUniTask(cancellationToken: token);

            // �㏈��
            _glowRotateTween?.Kill();
            panel.SetActive(false);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _glowRotateTween?.Kill();
        }
    }
}
