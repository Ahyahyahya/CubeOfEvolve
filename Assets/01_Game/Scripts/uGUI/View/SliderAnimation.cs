using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.AT
{
    public class SliderAnimation : MonoBehaviour
    {
        private Slider _slider;

        private void Start()
        {
            _slider = GetComponent<Slider>();
        }

        public void SliderAnime(float value)
        {
            // �A�j���[�V�������Ȃ���Slider�𓮂���
            DOTween.To(() => _slider.value,
                n => _slider.value = n,
                value,
                duration: 1.0f);
        }
    }
}
