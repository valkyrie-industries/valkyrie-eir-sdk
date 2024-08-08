using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.EIR.Examples
{
    public class BowAndArrowGame : MonoBehaviour
    {
        [SerializeField]
        Target targetPrefab;

        public List<Target> targets;

        [SerializeField]
        private bool gameOnGoing;

        [SerializeField]
        float spawningPeriod = 3;

        void Start()
        {
            targets = new List<Target>();
            StartCoroutine(TargetSpawning());
        }

        // Update is called once per frame
        void Update()
        {
            //foreach (Target target in targets)

        }

        IEnumerator TargetSpawning()
        {
            while (true)
            {
                if (gameOnGoing)
                    SpawnTarget();
                yield return new WaitForSeconds(spawningPeriod);
            }
        }

        void SpawnTarget()
        {
            Target newTarget = Instantiate(targetPrefab, transform);
            newTarget.GetComponent<Transform>().localPosition = new Vector3(Random.Range(-5, 5), Random.Range(0, 3), 5 + Random.Range(0, 5));
            float size = Random.Range(0.3f, 3f);
            newTarget.GetComponent<Transform>().localScale = Vector3.one * size;
            targets.Add(newTarget);
        }

        public void DestroyTarget(Target target)
        {
            if (targets.Contains(target))
            {
                targets.Remove(target);
                Destroy(target.gameObject);
                //Debug.Log("Members " + targets.Count);

            }
        }

        public void GameOnGoing(bool _input)
        {
            gameOnGoing = _input;
        }
    }
}

    
