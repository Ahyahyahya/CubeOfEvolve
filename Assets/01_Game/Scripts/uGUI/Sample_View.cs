using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MVRP.Sample
{
    public sealed class Sample_View : MonoBehaviour
    {
        /// <summary>
        /// uGUI��Slider���A�j���[�V����������R���|�[�l���g�iView�j
        /// </summary>
        [SerializeField] private Slider _slider;

        public void SetValue(float value)
        {
            // �A�j���[�V�������Ȃ���Slider�𓮂���
            DOTween.To(() => _slider.value,
                n => _slider.value = n,
                value,
                duration: 1.0f);
        }
    }
}
