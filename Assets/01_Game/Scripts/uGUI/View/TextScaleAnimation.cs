using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using R3;

namespace Assets.AT
{
    public class TextScaleAnimation : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _text;

        private float _currentValue = 0f;
        private Vector3 _initScale;

        private void Start()
        {
            if (_text != null)
            {
                _initScale = _text.GetComponent<RectTransform>().localScale;
            }
            else
            {
                Debug.LogError("_text �� null�łȁI");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="totalDuration"></param>
        public void AnimateFloatAndText(float Value, float totalDuration)
        {
            var sizeDuration = totalDuration * 0.5f;

            // 1. float�^�̕ϐ��̒l���A�j���[�V����
            DOTween.To(() => _currentValue,
                n => _currentValue = n,
                Value,
                duration: totalDuration
                ).OnUpdate(TextUpdate); ;

            // 2. �e�L�X�g�T�C�Y�𑍃A�j���[�V�������Ԃ̔����b��2�{�Ɋg��
            Sequence sequence = DOTween.Sequence();
            sequence.Append(_text.rectTransform.DOScale(_initScale * 2, sizeDuration));

            // 3. 2�̊������A���A�j���[�V�������Ԃ̔����b�Ō��̃T�C�Y�ɖ߂�
            sequence.Append(_text.rectTransform.DOScale(_initScale, sizeDuration));
        }

        private void TextUpdate()
        {
            _text.text = $"{_currentValue:F0}";
        }
    }
}
