using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBack : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void MoveBackToInitialPosition()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
    }
}
