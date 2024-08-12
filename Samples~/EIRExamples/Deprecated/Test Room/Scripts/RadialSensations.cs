using UnityEngine;
#if EIR_HAPTICS
using Valkyrie.EIR.Haptics;
#endif
#if EIR_INTERACTION
using Valkyrie.EIR.Interaction;
#endif
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Examples {
    public class RadialSensations : MonoBehaviour
    {
        #if EIR_HAPTICS
        [SerializeField]
        HapticPreset.PresetType presetType;
#endif
        [SerializeField]
        float period = 1;
        [SerializeField]
        float sensationCutoffDistance = 0.5f;
        [SerializeField]
        float cutoffDistance = 0.5f;

        private enum Decay { linear, square, exponential };
        [SerializeField]
        Decay decay;

#if EIR_HAPTICS

        private HapticPresetRunner[] hapticPresetRunners = new HapticPresetRunner[2];
#endif


        private bool initialised;

#if EIR_INTERACTION
        private InteractingBodyPart[] hands;
        private InteractionManager interaction { get { return EIRManager.Instance.Interaction; } }

#endif
        // Start is called before the first frame update
        void Start()
        {
#if EIR_INTERACTION
            hands = interaction.InteractingBodyParts;
#endif
        }

        // Update is called once per frame
        void Update()
        {
#if EIR_HAPTICS && EIR_INTERACTION
            if (!initialised)
            {
                if (EIRManager.Instance.Haptics != null)
                    InitialisePresets();
                return;
            }
            hapticPresetRunners[(int)BodyPart.leftHand].m_intensityMultiplier = 0;
            hapticPresetRunners[(int)BodyPart.rightHand].m_intensityMultiplier = 0;

            for (int i = 0; i < interaction.InteractingBodyParts.Length; i++)
            {
                if (Vector3.Distance(interaction.InteractingBodyParts[i].position, transform.position) < cutoffDistance)
                {
                    hapticPresetRunners[i].m_intensityMultiplier = UpdateForceDependingOnRadialDistance((BodyPart)i);
                }
            }
#endif
        }

        void InitialisePresets()
        {
#if EIR_HAPTICS
            HapticPreset hapticPresetProperties = HapticPreset.CreateDefaultPreset(presetType, period, HapticPreset.LoopType.Loop);
            hapticPresetRunners[(int)BodyPart.leftHand] = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.leftHand, hapticPresetProperties, 0);
            hapticPresetRunners[(int)BodyPart.rightHand] = EIRManager.Instance.Haptics.CreateHapticPresetRunner(BodyPart.rightHand, hapticPresetProperties, 0);
            initialised = true;
#endif
        }

#if EIR_INTERACTION
        float UpdateForceDependingOnRadialDistance(BodyPart part)
        {
            float distance = (interaction.InteractingBodyParts[(int)part].position - transform.position).magnitude;
            float radial_intensity = 0;
            switch (decay)
            {
                case Decay.linear:
                    radial_intensity = 1.0f - distance / sensationCutoffDistance;
                    break;
                case Decay.square:
                    if (distance < sensationCutoffDistance)
                        radial_intensity = (distance - sensationCutoffDistance) * (distance - sensationCutoffDistance) / (sensationCutoffDistance * sensationCutoffDistance);
                    else
                        radial_intensity = 0;
                    break;
                case Decay.exponential:
                    radial_intensity = Mathf.Exp(-distance / sensationCutoffDistance) * 1.35f;
                    break;
            }
            radial_intensity = ValkyrieEIRExtensionMethods.Map(radial_intensity, 0, 1);

            //Debug.Log(part + ", " + radial_intensity);
            return radial_intensity;
        }
#endif
    }

}

