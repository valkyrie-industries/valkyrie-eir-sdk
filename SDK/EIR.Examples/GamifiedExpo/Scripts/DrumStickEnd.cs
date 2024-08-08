using UnityEngine;
using Valkyrie.EIR;
using Valkyrie.EIR.Interaction.Interactables;
using Valkyrie.EIR.Haptics;

public class DrumStickEnd : MonoBehaviour
{
    [SerializeField]
    GrabInteractable drumStick;

    Vector3 previousPosition, velocity;

    float intensityMultiplier = 4.0f;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        velocity = transform.position - previousPosition;
        previousPosition = transform.position;
    }
    

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision " + velocity.magnitude);
        Drum drum = collision.collider.GetComponent<Drum>();
        if (drum != null)
        {
            drum.MakeSound(collision.relativeVelocity.magnitude);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        //Debug.Log("Trigger " + velocity.magnitude);
        Drum drum = collider.GetComponent<Drum>();
        if (drum != null)
        {
            drum.MakeSound(velocity.magnitude * 2);
            //drumStick.ApplyForceToGrabbingArm(velocity.magnitude * intensityMultiplier);
#if EIR_HAPTICS
            HapticPresetRunner runner = EIRManager.Instance.Haptics.CreateHapticPresetRunner(drumStick.currentlyInteractingBodyPart.BodyPart, HapticPreset.CreateDefaultPreset(HapticPreset.PresetType.maximum, 0.1f), velocity.magnitude * intensityMultiplier);
#endif
        }
    }
}
