// �쐬���F   250521
// �X�V���F   250521
// �쐬�ҁF ���� ���l

// �T�v����(AI�ɂ��쐬)�F

// �g���������F
// ��p�̋����������ėp�I�ȃR�[�h�Q

using System.Collections;
using UnityEngine;

namespace Assets.IGC2025.Scripts.Event
{
    public class EventLevelUp : MonoBehaviour
    {
        // -----SerializeField
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private GameObject _LevelupFlame;

        // -----UnityMessage

        private void Start()
        {
            Initialize();
        }

        // -----Public

        /// <summary>
        /// 
        /// </summary>
        public void event_Levelup()
        {
            StartCoroutine(CreateLevelup());
            _particleSystem.Play();
        }

        // -----Private

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            if (!_LevelupFlame || !_particleSystem)
            {
                Destroy(this);
                return;
            }

            _LevelupFlame.transform.localScale = Vector3.zero;
            _particleSystem.Stop();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowLevelup()
        {
            _LevelupFlame.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(3f);
            _LevelupFlame.transform.localScale = Vector3.zero;
        }

        private IEnumerator CreateLevelup()
        {
            var obj = Instantiate(_LevelupFlame, _LevelupFlame.transform.position, Quaternion.identity, _LevelupFlame.transform.parent);
            obj.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(3f);
            Destroy(obj);
        }
    }
}


