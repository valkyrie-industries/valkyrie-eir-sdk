using System.Collections.Generic;
using UnityEngine;
using System;

namespace Valkyrie.EIR.Bluetooth {

    /// <summary>
    /// Class to dispatch actions called from threads other than the main thread within the Bluetooth Java plugin.
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour {

        private readonly List<System.Action> actions = new List<System.Action>();


        private static MainThreadDispatcher Instance;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Run this action on the main thread.
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnMainThread(System.Action action) {
            if (Instance != null) {
                Instance.Run(action);
            }
        }

        private void Run(System.Action action) {
            if (action == null) {
                return;
            }

            lock (actions) {
                actions.Add(action);
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
                }
                catch (Exception ex) {
                    Debug.LogError($"[EIR Bluetooth] Exception in MainThreadDispatcher action: {ex}");
                }
            }
        }



    }
}