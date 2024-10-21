using UnityEngine;

namespace Valkyrie.EIR.Examples {

    /// <summary>
    /// Moves a gameobject back to its start position and orientation.
    /// </summary>
    public class MoveBack : MonoBehaviour {

        #region Private Variables

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        #endregion

        #region Unity Methods

        private void Start() {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Return the gameobject back to the position and orientation it occupied on Start.
        /// </summary>
        public void MoveBackToInitialPosition() {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }

        #endregion
    }
}