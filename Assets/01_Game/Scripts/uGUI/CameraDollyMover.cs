using Assets.IGC2025.Scripts.GameManagers;
using DG.Tweening;
using R3;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace Assets.AT
{
    public class CameraDollyMover : MonoBehaviour
    {
        // -----Field
        [Header("Reference")]
        [SerializeField] private CinemachineSplineDolly _cinemachineSplineDolly;
        [SerializeField] private SplineContainer[] _splineContainer;

        [Header("Value")]
        [SerializeField] private float _intervalForChangeSpline = 10f;

        [Header("UI")]
        [SerializeField] private Image _panel;

        private int currentSplineContainerIndex;
        private float _fadeTime;
        private float _intervalTime;

        private Coroutine _coroutine;
        private const int FIRST_SPLINE_NUM = 0;

        // -----UnityMessage
        private void Start()
        {
            // null �m�F
            if (_cinemachineSplineDolly == null || _splineContainer == null)
            {
                Debug.LogError("CameraDollyMover:CinemachineSplineDolly �� SplineContainer ���ݒ肳��Ă��܂���");
                enabled = false;
                return;
            }

            // ������
            currentSplineContainerIndex = FIRST_SPLINE_NUM;
            _cinemachineSplineDolly.Spline = _splineContainer[currentSplineContainerIndex];
            _panel.enabled = true;

            _fadeTime = _intervalForChangeSpline * 0.1f;
            _intervalTime = _intervalForChangeSpline - _fadeTime * 2;

            // GameState��TITLE�̂Ƃ��̂� Coroutine ���J�n�A����ȊO�Œ�~
            GameManager.Instance.CurrentGameState
                .Subscribe(state =>
                {
                    if (state == GameState.TITLE)
                    {
                        // ���ɓ����Ă���Ύ~�߂�
                        if (_coroutine != null)
                        {
                            StopCoroutine(_coroutine);
                            _coroutine = null;
                        }

                        // Coroutine ���J�n
                        _coroutine = StartCoroutine(AutoChangeSpline());
                    }
                    else
                    {
                        // Coroutine ���~�A���S����
                        if (_coroutine != null)
                        {
                            StopCoroutine(_coroutine);
                            _coroutine = null;
                        }

                        // DOTween���o���~�߂�
                        _panel?.DOComplete();
                        _panel?.DOKill();
                    }
                })
                .AddTo(this);
        }

        private void OnDestroy()
        {
            // Coroutine ��~�Ɖ��o��~�i�j�����j
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            _panel?.DOComplete();
            _panel?.DOKill();
        }

        // -----Private
        /// <summary>
        /// ���Ԍo�߂ŃJ�����̈ړ����[�g��؂�ւ���iCoroutine�j
        /// </summary>
        private IEnumerator AutoChangeSpline()
        {
            while (true)
            {
                // �t�F�[�h�A�E�g
                _panel?.DOFade(0, _fadeTime);
                yield return new WaitForSeconds(_intervalTime);

                // �t�F�[�h�C��
                _panel?.DOFade(1, _fadeTime);
                yield return new WaitForSeconds(_fadeTime);

                // �J�����ʒu�����Z�b�g���ăX�v���C���؂�ւ�
                if (_cinemachineSplineDolly != null)
                {
                    _cinemachineSplineDolly.CameraPosition = 0;
                    SwitchToNextSpline();
                }

                yield return new WaitForSeconds(_fadeTime);
            }
        }

        /// <summary>
        /// �X�v���C�������Ԃɐ؂�ւ���
        /// </summary>
        private void SwitchToNextSpline()
        {
            currentSplineContainerIndex = (currentSplineContainerIndex + 1) % _splineContainer.Length;
            _cinemachineSplineDolly.Spline = _splineContainer[currentSplineContainerIndex];
        }
    }
}
