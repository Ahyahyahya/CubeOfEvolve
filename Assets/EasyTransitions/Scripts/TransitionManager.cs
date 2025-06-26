using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using R3;
using Unity.VisualScripting;
using Unit = R3.Unit;

namespace EasyTransition
{

    public class TransitionManager : MonoBehaviour
    {
        // ---------- SerializeField
        [SerializeField] private GameObject transitionTemplate;

        // ---------- R3
        private Subject<Unit> _onTransitionStarted = new();
        public Observable<Unit> OnTransitionStarted => _onTransitionStarted;

        private Subject<Unit> _onSceneLoadCompleted = new();
        public Observable<Unit> OnTransitionHalf => _onSceneLoadCompleted;

        private Subject<Unit> _onTranstionCompleted = new();
        public Observable<Unit> OnTransitionCompleted => _onTranstionCompleted;

        // ---------- Field
        private bool _isRunning;
        public bool IsRunning => _isRunning;

        // ---------- UnityMessage
        private void Start()
        {
            _onTransitionStarted.AddTo(this);
            _onSceneLoadCompleted.AddTo(this);
            _onTranstionCompleted.AddTo(this);
        }

        // ---------- Public


        /// <summary>
        /// Starts a transition without loading a new level.
        /// </summary>
        /// <param name="transition">The settings of the transition you want to use.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(TransitionSettings transition, float startDelay)
        {
            if (transition == null || _isRunning)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            _isRunning = true;
            StartCoroutine(Timer(startDelay, transition));
        }

        /// <summary>
        /// �f�B���C�������O�V�[���J��
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="transition"></param>
        public void Transition(string sceneName, TransitionSettings transition)
        {
            if (transition == null || _isRunning)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            _isRunning = true;
            StartCoroutine(Timer(sceneName, transition));
        }

        /// <summary>
        /// �f�B���C�����C���f�b�N�X�V�[���J��
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="transition"></param>
        public void Transition(int sceneIndex, TransitionSettings transition)
        {
            if (transition == null || _isRunning)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            _isRunning = true;
            StartCoroutine(Timer(sceneIndex, transition));
        }

        /// <summary>
        /// Loads the new Scene with a transition.
        /// </summary>
        /// <param name="sceneName">The name of the scene you want to load.</param>
        /// <param name="transition">The settings of the transition you want to use to load you new scene.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(string sceneName, TransitionSettings transition, float startDelay)
        {
            if (transition == null || _isRunning)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            _isRunning = true;
            StartCoroutine(Timer(sceneName, startDelay, transition));
        }

        /// <summary>
        /// Loads the new Scene with a transition.
        /// </summary>
        /// <param name="sceneIndex">The index of the scene you want to load.</param>
        /// <param name="transition">The settings of the transition you want to use to load you new scene.</param>
        /// <param name="startDelay">The delay before the transition starts.</param>
        public void Transition(int sceneIndex, TransitionSettings transition, float startDelay)
        {
            if (transition == null || _isRunning)
            {
                Debug.LogError("You have to assing a transition.");
                return;
            }

            _isRunning = true;
            StartCoroutine(Timer(sceneIndex, startDelay, transition));
        }

        /// <summary>
        /// Gets the index of a scene from its name.
        /// </summary>
        /// <param name="sceneName">The name of the scene you want to get the index of.</param>
        int GetSceneIndex(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).buildIndex;
        }

        IEnumerator Timer(string sceneName, TransitionSettings transitionSettings)
        {
            _onTransitionStarted.OnNext(Unit.Default);

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            // �V�[���J�ڂ������������Ƃ�ʒm
            _onSceneLoadCompleted.OnNext(Unit.Default);

            // �A�j���[�V�������I������܂őҋ@
            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            _isRunning = false;

            Destroy(template);

            // �V�[���J�ڃA�j���[�V�����������������Ƃ�ʒm
            _onTranstionCompleted.OnNext(Unit.Default);
        }

        IEnumerator Timer(int sceneIndex, TransitionSettings transitionSettings)
        {
            _onTransitionStarted.OnNext(Unit.Default);

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            yield return SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);

            // �V�[���J�ڂ������������Ƃ�ʒm
            _onSceneLoadCompleted.OnNext(Unit.Default);

            // �A�j���[�V�������I������܂őҋ@
            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            _isRunning = false;

            Destroy(template);

            // �V�[���J�ڃA�j���[�V�����������������Ƃ�ʒm
            _onTranstionCompleted.OnNext(Unit.Default);
        }

        IEnumerator Timer(string sceneName, float startDelay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            _onTransitionStarted.OnNext(Unit.Default);

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            yield return SceneManager.LoadSceneAsync(sceneName);

            _onSceneLoadCompleted.OnNext(Unit.Default);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            Destroy(template);

            _isRunning = false;

            _onTranstionCompleted.OnNext(Unit.Default);
        }

        IEnumerator Timer(int sceneIndex, float startDelay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(startDelay);

            _onTransitionStarted.OnNext(Unit.Default);

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            yield return SceneManager.LoadSceneAsync(sceneIndex);

            _onSceneLoadCompleted.OnNext(Unit.Default);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            Destroy(template);

            _isRunning = false;

            _onTranstionCompleted.OnNext(Unit.Default);
        }

        IEnumerator Timer(float delay, TransitionSettings transitionSettings)
        {
            yield return new WaitForSecondsRealtime(delay);

            _onTransitionStarted.OnNext(Unit.Default);

            GameObject template = Instantiate(transitionTemplate) as GameObject;
            template.GetComponent<Transition>().transitionSettings = transitionSettings;

            float transitionTime = transitionSettings.transitionTime;
            if (transitionSettings.autoAdjustTransitionTime)
                transitionTime = transitionTime / transitionSettings.transitionSpeed;

            yield return new WaitForSecondsRealtime(transitionTime);

            template.GetComponent<Transition>().OnSceneLoad(SceneManager.GetActiveScene(), LoadSceneMode.Single);

            _onSceneLoadCompleted.OnNext(Unit.Default);

            yield return new WaitForSecondsRealtime(transitionSettings.destroyTime);

            Destroy(template);

            _isRunning = false;

            _onTranstionCompleted.OnNext(Unit.Default);
        }
    }
}
