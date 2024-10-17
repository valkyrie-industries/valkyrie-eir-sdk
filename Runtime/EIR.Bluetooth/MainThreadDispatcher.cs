using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valkyrie.EIR.Bluetooth {

    /// <summary>
    /// Class to dispatch actions called from threads other than the main thread within the Bluetooth Java plugin.
    /// Some actions can only be called upon the main thread (e.g. Unity UX) and would otherwise fail to invoke.
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour {

        #region Events

        private readonly List<Action> actions = new List<Action>();

        #endregion

        #region Static Accessors

        private static MainThreadDispatcher Instance;

        #endregion

        #region Unity Methods

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }
        private void Update() {
            List<Action> actionsCopy;
            lock (actions) {
                actionsCopy = new List<Action>(actions);
                actions.Clear();
            }
            foreach (var action in actionsCopy) {
                try {
                    action?.Invoke();
                } catch (Exception ex) {
                    Debug.LogError($"[EIR Bluetooth] Exception in MainThreadDispatcher action: {ex}");
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Run the input action on the main thread.
        /// This method should only be called from threads other than the main thread.
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnMainThread(Action action) {
            if (Instance != null) {
                Instance.Run(action);
            }
        }

        #endregion

        private void Run(Action action) {
            if (action == null) {
                return;
            }

            lock (actions) {
                actions.Add(action);
            }
        }
    }
}