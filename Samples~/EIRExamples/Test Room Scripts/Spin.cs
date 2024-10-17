using UnityEngine;

/// <summary>
/// Rotates a gameobject over time.
/// </summary>
public class Spin : MonoBehaviour {

    #region Serialized Variables

    [SerializeField]
    private float speed = 10;

    #endregion

    #region Unity Methods

    private void Update() {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }

    #endregion
}
