using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.EIR.Examples
{
    public class Bullet : MonoBehaviour
    {
        bool fired = false;

        public bool debug = false;

        [SerializeField]
        private Rigidbody rb;

        public Rigidbody Rigid { get { return rb; } }

        float fireTime = 0;
        float colliderWait = 0.3f;

        Collider col;

        public void OnFired()
        {
            fireTime = Time.time;
            fired = true;
            col = GetComponent<Collider>();
            col.enabled = false;
        }

        public void Update()
        {
            if(col != null && fireTime + colliderWait < Time.time)
            {
                col.enabled = true;
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (debug) Debug.Log("I hit " + collision.gameObject.name, collision.gameObject);
            if(fired && fireTime + colliderWait < Time.time)
                Destroy(gameObject);
        }
    }
}


