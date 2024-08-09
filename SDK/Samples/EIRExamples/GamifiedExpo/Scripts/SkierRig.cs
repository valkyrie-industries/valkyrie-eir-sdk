using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using PathCreation;

namespace Valkyrie.EIR.Examples
{
    /*
    public class SkierRig : MonoBehaviour
    {
        [HideInInspector]
        public float skierRigVelocity;
        public Vector3 pathDirection;
        //It needs to know the path
        [SerializeField]
        private PathCreator path;
        private float distanceTravelled;

        private float localFriction;

        private void Start()
        {
        }

        private void LateUpdate()
        {
            if (path != null)
                pathDirection = transform.forward;
            skierRigVelocity *= localFriction;
            distanceTravelled += skierRigVelocity;
            //Debug.Log("Distance travelled " + distanceTravelled);
            if (path != null)
            {
                transform.position = path.path.GetPointAtDistance(distanceTravelled);
                transform.rotation = path.path.GetRotationAtDistance(distanceTravelled);
            }
            else
            {
                transform.position += new Vector3(0,0,skierRigVelocity);
            }
        }

        public void AddProjectionToVelocity(float velocityMagnitude, float friction)
        {
            skierRigVelocity += velocityMagnitude;
            localFriction = friction;
        }
    }
    */

}