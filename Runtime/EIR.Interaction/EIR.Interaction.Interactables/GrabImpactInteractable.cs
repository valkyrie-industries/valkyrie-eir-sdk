using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction.Interactables
{
    public class GrabImpactInteractable : GrabInteractable {
        private Collider col;
        protected Rigidbody rig;


        [Header("Grab Impact Interactable")]
        [SerializeField]
        private float maximumAccountedMagnitude;
#if EIR_HAPTICS
        [SerializeField]
        private HapticPreset.PresetType presetToRun = HapticPreset.PresetType.maximum;
#endif
        [SerializeField]
        private float presetTime = 0.1f;

        private bool doCollisionCheck = false;

        [SerializeField]
        private Collider[] blackListedColliders;


        public override void Start() {
            col = GetComponent<Collider>();

            if (col == null) Debug.LogError("[Grab Impact Interactable] No collider found on this gameobject", gameObject);

            rig = GetComponent<Rigidbody>();

            if (rig == null) Debug.LogError("[Grab Impact Interactable] No Rigidbody found on this gameobject", gameObject);
            base.Start();
        }

        public override void Interacting() {
            doCollisionCheck = !(!grabbing);
        }

        private void FixedUpdate() {
            if (justGrabbed) {
                SendZeroForce();
            }

            if (doCollisionCheck)
                CheckForCollisionsWhileHolding();
        }

        void CheckForCollisionsWhileHolding() {
            Physics.SyncTransforms();

            int layerMask = ~LayerMask.GetMask("Hands");

            Collider[] colliders = Physics.OverlapBox(transform.position, col.bounds.extents, transform.localRotation, layerMask);

            List<Collider> colList = colliders.ToList();

            for (int i = 0; i < colliders.Length; i++) {
                for (int j = 0; j < blackListedColliders.Length; j++) {
                    if (colliders[i] == blackListedColliders[j])
                        colList.Remove(colliders[i]);
                }

            }

            colliders = colList.ToArray();

            if (colliders.Length == 0) {
                SendZeroForce();
                return;
            }

            float totalCollisionMagnitude = currentlyInteractingBodyPart.velocity.magnitude;

            for (int i = 0; i < colliders.Length; i++) {

                Rigidbody otherRig = colliders[i].gameObject.GetComponent<Rigidbody>();

                if (otherRig == null || otherRig == rig)
                    continue;

                totalCollisionMagnitude += otherRig.velocity.magnitude;
            }

            float avgCollisionMagnitude = totalCollisionMagnitude / colliders.Length;

            avgCollisionMagnitude = Mathf.Clamp(avgCollisionMagnitude, 0, maximumAccountedMagnitude);

            float force = ValkyrieEIRExtensionMethods.MapToRange(avgCollisionMagnitude, 0, maximumAccountedMagnitude, 0, 1);

            force = Mathf.Clamp(force, 0, 1);

            //if (runner != null)
            //    runner.Stop(); // Can we do an event which stops all runners?
#if EIR_INTERACTION && EIR_HAPTICS

            InvokeHapticPresetTypeRequest(currentlyInteractingBodyPart.BodyPart, presetToRun);
#endif
        }

    }
}


