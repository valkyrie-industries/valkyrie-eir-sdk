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

        public void OnFired()
        {
            fired = true;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (debug) Debug.Log("I hit " + collision.gameObject.name, collision.gameObject);
            if(fired)
                Destroy(gameObject);
        }
    }
}


