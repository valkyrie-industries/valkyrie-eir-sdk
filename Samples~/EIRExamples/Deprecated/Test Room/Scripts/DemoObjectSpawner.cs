using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.EIR.Fitness {

    public class DemoObjectSpawner : MonoBehaviour {
        [SerializeField]
        GameObject spawnedObject;

        [SerializeField]
        int maxObjects;

        [SerializeField]
        bool useRandomWait;

        [SerializeField]
        float timeToSpawn, randomWaitMinTime, randomWaitMaxTime;

        [SerializeField]
        Vector3 addedForce;

        List<GameObject> objects = new List<GameObject>();

        IEnumerator SpawnObject() {
            while (true) {
                if (useRandomWait) {
                    yield return new WaitForSeconds(Random.Range(randomWaitMinTime, randomWaitMaxTime));
                }
                else {
                    yield return new WaitForSeconds(timeToSpawn);
                }

                if (objects.Count > maxObjects) {
                    GameObject destroyedObject = objects[0];
                    Destroy(destroyedObject);
                    objects.RemoveAt(0);
                }

                GameObject newObject = Instantiate(spawnedObject, transform.position, Quaternion.identity);

                objects.Add(newObject);

                Rigidbody rig = newObject.GetComponent<Rigidbody>();

                if (rig != null) {
                    rig.AddForce(addedForce);
                }
            }
        }

        private void OnEnable() {
            StartCoroutine(SpawnObject());
        }

        private void OnDisable() {
            StopAllCoroutines();
        }


    }
}