using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    /// <summary>
    /// Defines physics modes for the calculation of applicable EMS forces.
    /// </summary>
    public enum EmsPhysicsMode { 
        MassOnly, 
        ResistanceBased, 
        MassAndAccelerationVector, 
        TensionBasedMass, 
        MassAndElbowAngle, 
        MassAndAccelerationScalar, 
        MassAndElbowAngleAndAccelerationScalar };


    /// <summary>
    /// Extension methods used throughout the codebase
    /// </summary>
    public static class ValkyrieEIRExtensionMethods {

        #region Constants

        public static readonly float AccelerationMultiplier = 0.001f;
        public static readonly float HitForceMultiplier = 0.015f;
        public static readonly float MassMultipier = 0.2f;

        #endregion

        #region Mapping

        /// <summary>
        /// Maps the input between minimum and maximum values to 0,1 range.
        /// </summary>
        /// <seealso cref="attachPointCompatibilityMode"/>
        public static float Map(float input, float min, float max) {
            float output = 0;
            input = Mathf.Clamp(input, min, max);
            output = (input - min) / (max - min);
            return output;
        }


        /// <summary>
        /// Maps the input between minimum and maximum values to a given range.
        /// </summary>
        public static float MapToRange(this float value, float fromSource, float toSource, float fromTarget, float toTarget) {
            return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }

        /// <summary>
        /// Maps the input to a specific ExpRange.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float MapToExpRange(int value) {
            float output = 0;
            switch (value) {
                case 3:
                    output = 1;
                    break;
                case 4:
                    output = 2;
                    break;
                case 5:
                    output = 4;
                    break;
                case 6:
                    output = 7;
                    break;
                case 7:
                    output = 10;
                    break;
                case 8:
                    output = 20;
                    break;
                case 9:
                    output = 45;
                    break;
                case 10:
                    output = 100;
                    break;
            }
            return output;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Search all of the children of this gameobject for a name that matches the input
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject FindChildByName(this GameObject parent, string name) {
            Transform[] children = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children) {
                if (child.gameObject.name == name) {
                    return child.gameObject;
                }
            }
            return null;
        }


        /// <summary>
        /// Maps the color output between green and black based on the calibration level.
        /// Colour range: 0 - green; 5 - red; 10 - black:
        /// </summary>
        public static Color ColorBasedOnCalibrationLevel(int level) {
            Color bandColor;
            if (level < 6)
                bandColor = Color.HSVToRGB(0.33f - (0.066f * level), 1.0f, 1.0f);
            else
                bandColor = Color.HSVToRGB(0, 1.0f, 2.0f - 0.2f * (level));

            return bandColor;
        }
        /// <summary>
        /// Maps the size output between min and max based on the calibration level.
        /// </summary>
        public static Vector3 SizeBasedOnCalibrationLevel(int level, float min, float max, bool Zaxis = false) {
            float map = MapToRange(level, 0, 10, min, max);
            Vector3 size = new Vector3(map, map, Zaxis ? map : 1);
            return size;
        }

        #endregion
    }
}