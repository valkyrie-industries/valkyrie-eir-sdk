using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.EventSystems;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Interaction
{

    [Serializable]
    public class VLKXRBridge
    {
#if EIR_INTERACTION
        [SerializeField]
        public XRController[] controllers { get; private set; }

        public void Initialise()
        {
            controllers = GameObject.FindObjectsOfType<XRController>();
            Debug.Log("[VLKXRB] Initialise");
            if (controllers.Length == 0)
                Debug.LogError("[VLKXRB] No controllers found to read inputs from");
        }


        //Read a given button from a controller
        public bool ReadButton(bool isLeft, ControllerButtons button)
        {
            XRNode node;

            if (isLeft)
                node = XRNode.LeftHand;
            else
                node = XRNode.RightHand;

            XRController controller = FindControllerWithNode(node);

            if (controller == null)
            {
                Debug.Log("[VLKXRB] No controllers with the node " + node + " are loaded");
                return false;
            }

            InputFeatureUsage<bool> usage = ButtonToFeature(button);

            if (controller.enableInputActions &&
                controller.inputDevice.TryGetFeatureValue(usage, out var controllerInput))
            {
                return controllerInput;
            }
            return false;
        }

        public Vector2 ReadAxis(bool isLeft, ControllerAxis axis)
        {
            XRNode node;

            if (isLeft)
                node = XRNode.LeftHand;
            else
                node = XRNode.RightHand;

            XRController controller = FindControllerWithNode(node);

            if (controller == null)
            {
                Debug.Log("[VLKXRB] No controllers with the node " + node + " are loaded");
                return Vector2.zero;
            }

            InputFeatureUsage<Vector2> usage = AxisToFeature(axis);

            if (controller.enableInputActions &&
                controller.inputDevice.TryGetFeatureValue(usage, out var controllerInput))
            {
                return controllerInput;
            }

            return Vector2.zero;
        }


        //Send a vibration to the left or right controller
        public void SendVibration(bool isLeft, float _intensity, float _duration)
        {
            XRNode node;
            Debug.Log("[VLKXRB] Sending vibrations to Left: " + isLeft);
            if (isLeft)
                node = XRNode.LeftHand;
            else
                node = XRNode.RightHand;

            XRController controller = FindControllerWithNode(node);

            Debug.Log("[VLKXRB] Sending vibrations to: " + controller);

            if (controller == null)
            {
                Debug.Log("[VLKXRB] No controllers with the node " + node + " are loaded");
                return;
            }

#if EIR_USE_OVR_VIBRATIONS
            OVRInput.SetControllerVibration(_duration, _intensity, isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
#else
            controller.SendHapticImpulse(_intensity, _duration);
#endif
        }

        public void SendVibration(int pointerId, float _intensity, float _duration)
        {
            XRUIInputModule GetXRInputModule() => EventSystem.current.currentInputModule as XRUIInputModule;

            var inputModule = GetXRInputModule();
            if (inputModule == null)
            {
                return;
            }

            XRRayInteractor rayInteractor = inputModule.GetInteractor(pointerId) as XRRayInteractor;

            Debug.Log("[VLKXRB] Sending vibrations to pointer ID: " + pointerId);
#if EIR_USE_OVR_VIBRATIONS
             OVRInput.SetControllerVibration(_duration, _intensity, rayInteractor.GetComponent<XRController>().controllerNode == XRNode.LeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch);
#else
            if (rayInteractor != null) rayInteractor.SendHapticImpulse(_intensity, _duration);
#endif
        }

        InputFeatureUsage<bool> ButtonToFeature(ControllerButtons button)
        {
            switch (button)
            {
                case ControllerButtons.menu:
                    {
                        return CommonUsages.menuButton;
                    }
                case ControllerButtons.a:
                    {
                        return CommonUsages.primaryButton;
                    }
                case ControllerButtons.b:
                    {
                        return CommonUsages.secondaryButton;
                    }
                case ControllerButtons.trigger:
                    {
                        return CommonUsages.triggerButton;
                    }
                case ControllerButtons.grip:
                    {
                        return CommonUsages.gripButton;
                    }

            }

            InputFeatureUsage<bool> usage;

            return usage;

        }

        InputFeatureUsage<Vector2> AxisToFeature(ControllerAxis axis)
        {
            switch (axis)
            {
                case ControllerAxis.primaryAxis:
                    return CommonUsages.primary2DAxis;
            }

            InputFeatureUsage<Vector2> usage;

            return usage;

        }

        XRController FindControllerWithNode(XRNode node)
        {
            if (controllers[0] == null)
                Initialise();

            for (int i = 0; i < controllers.Length; i++)
            {
                if (controllers[i].controllerNode == node)
                {
                    return controllers[i];
                }
            }

            Debug.Log("[VLKXRB] No controllers with the node " + node + " are loaded");
            return null;
        }

#endif
        }

    }
