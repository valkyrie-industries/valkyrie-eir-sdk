using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR;
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif

namespace Valkyrie.EIR.Examples
{
    public class Arrow : MonoBehaviour
    {
        public Transform arrowBody;

        Collider col;
        Rigidbody rb;

        bool inFlight = false;

        float returnAfter = 5;
        float flyStartTime;
        
        float waitBeforeCollide = 10f;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void BeginFlight()
        {
            inFlight = true;
            flyStartTime = Time.time;
            col = GetComponent<Collider>();
            col.enabled = false;
        }

        private void LateUpdate()
        {
            if (inFlight && rb != null && arrowBody != null)
            {
                arrowBody.LookAt(transform.position + rb.velocity);
            }

            if (Time.time + waitBeforeCollide < Time.time && inFlight)
            {
                if(col != null)
                    col.enabled = true;
            }

            if (Time.time - flyStartTime > returnAfter && !rb.isKinematic && inFlight)
            {
                EndFlight();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("I HIT SOMETHING");

            if(flyStartTime + waitBeforeCollide < Time.time)
                EndFlight();
        }

        private void EndFlight()
        {
            inFlight = false;
#if EIR_INTERACTION
            if (GetComponent<ReturnAfterGrabbing>() != null)
                GetComponent<ReturnAfterGrabbing>().ReturnToOrigin();
#endif
            gameObject.layer = 0;
            col.enabled = true;

            Invoke("ResetRotation", 0.3f);
        }

        private void ResetRotation()
        {
            arrowBody.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
    }
}