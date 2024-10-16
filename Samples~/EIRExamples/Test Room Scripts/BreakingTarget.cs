using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.EIR.Haptics;

namespace Valkyrie.EIR.Examples {
    public class BreakingTarget : Target
    {
        [SerializeField]
        private MeshRenderer meshRenderer;
        private Rigidbody[] parts;
        private Vector3[] initPosArray;
        private Quaternion[] initRotArray;
        [SerializeField]
        private bool restartAfterBreaking;
        [SerializeField]
        private bool doEMSHaptics = false;

        public override void Start()
        {
            base.Start();
            if (meshRenderer == null)
                Debug.LogWarning("No meshrenderer found on this object", this);

            parts = GetComponentsInChildren<Rigidbody>(true);
            initPosArray = new Vector3[parts.Length];
            initRotArray = new Quaternion[parts.Length];
            for (int i = 1; i < parts.Length; i++)
            {
                initPosArray[i] = parts[i].transform.localPosition;
                initRotArray[i] = parts[i].transform.localRotation;
            }
        }

        public override void ReactToCollision(object parameter = null)
        {
            Material mat = meshRenderer.material;
            Debug.Log("Explode!");
            if (GetComponent<AudioSource>() != null)
                GetComponent<AudioSource>().Play();
            meshRenderer.enabled = false;
            this.GetComponent<Collider>().enabled = false;
            for (int i = 1; i < parts.Length; i++)
            {
                parts[i].gameObject.SetActive(true);
                parts[i].transform.SetParent(null);
                parts[i].AddForce(Vector3.forward * 2, ForceMode.Impulse);
            }

#if EIR_HAPTICS
            //Quick hack for demo scene
            if (doEMSHaptics)
            {
                if (EIRManager.Instance.Haptics != null)
                {
                    EIRManager.Instance.Haptics.AddHapticIntensity(DeviceRole.A, 1, false);
                    EIRManager.Instance.Haptics.AddHapticIntensity(DeviceRole.B, 1, false);
                }
            }
#endif

            //Give the coroutine to an object we know will be present so that it always deactivates (even without other spawns)
            StartCoroutine(coroutine = DelayedDeactivate(3));
            //Invoke("Deactivate", 2); - invoke doesn't work because the gameobject can be deactivated due to the z-position


        }


        private IEnumerator coroutine;

        private IEnumerator DelayedDeactivate(float delay)
        {
            float timeDelay = 0;
            while (timeDelay < delay)
            {
                yield return new WaitForEndOfFrame();
                timeDelay += Time.deltaTime;
            }
            Deactivate();
        }

        private void Deactivate()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            // for exploding objects, turn the meshrenderers back before deactivating the gameobjects
            MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
                mr.enabled = true;
            StopAllCoroutines(); // If anything runs on this behaviour - stop it

            //base.Deactivate();
            // Reset any other properties or components of the object if needed
            for (int i = 1; i < parts.Length; i++)
            {
                if (gameObject.activeInHierarchy)
                    parts[i].transform.SetParent(this.transform);
                parts[i].transform.localPosition = initPosArray[i];
                parts[i].transform.localRotation = initRotArray[i];
                parts[i].gameObject.SetActive(false);
            }

            if (restartAfterBreaking)
            {
                meshRenderer.enabled = true;
                if (this.GetComponent<Collider>())
                    this.GetComponent<Collider>().enabled = true;
            }
            else
            {
                if (gameObject.activeInHierarchy)
                    this.gameObject.SetActive(false);
            }

        }

        private IEnumerator RestartCoroutine()
        {
            yield return new WaitForSeconds(5);
            RestartObject();
        }

        public override void RestartObject()
        {
            base.RestartObject();

            // for exploding objects, turn the meshrenderers back before deactivating the gameobjects
            MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in renderers)
                mr.enabled = true;

            // Reset any other properties or components of the object if needed
            if (parts == null)
                return;

            for (int i = 1; i < parts.Length; i++)
            {
                parts[i].transform.SetParent(this.transform);
                parts[i].transform.localPosition = initPosArray[i];
                parts[i].transform.localRotation = initRotArray[i];
                parts[i].gameObject.SetActive(false);
            }
        }
    }
}


