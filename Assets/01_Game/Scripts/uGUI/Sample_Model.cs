using R3;
using UnityEngine;

namespace MVRP.Sample
{
    public sealed class Sample_Model : MonoBehaviour
    {
        /// <summary>
        /// �̗�
        /// ReactiveProperty�Ƃ��ĊO���ɏ�Ԃ�ReadOnly�Ō��J
        /// </summary>
        public ReadOnlyReactiveProperty<int> Health => _health;
        // �̗͂̍ő�l
        public readonly int MaxHealth = 100;

        private readonly ReactiveProperty<int> _health = new ReactiveProperty<int>(100);

        /// <summary>
        /// �Փ˃C�x���g
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            _health.Value -= 10;

            // Enemy�ɐG�ꂽ��̗͂����炷
            //if (collision.gameObject.TryGetComponent<Enemy>(out var _))
            //{
            //    _health.Value -= 10;
            //}
        }

        private void OnDestroy()
        {
            _health.Dispose();
        }
    }
}
