using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valkyrie.EIR.Utilities;

namespace Valkyrie.EIR.Haptics
{
    /// <summary>
    /// Runs the provided HapticPreset in real time. Use by calling CreateHapticPresetRunner from the HapticsManager
    /// </summary>
    [Serializable]
    public class HapticPresetRunner : MonoBehaviour
    {

        #region Events

        public delegate void OnHapticPresetRequestEventHandler(int bodyPart, float intensity);
        public static event OnHapticPresetRequestEventHandler OnHapticPresetRequest;

        #endregion

        #region Public Properties

        /// <summary>
        /// Which body parts will this preset affect during execution.
        /// </summary>
        public List<BodyPart> m_affectedBodyParts { get; private set; } = new List<BodyPart>();

        /// <summary>
        /// Halts execution of the preset if true
        /// </summary>
        public bool paused { get; private set; } = false;

        /// <summary>
        /// Multiplies the output intensity
        /// </summary>
        public float IntensityMultiplier { get { return m_intensityMultiplier; } set { m_intensityMultiplier = value; } }

        #endregion

        #region Serialized Variables

        /// <summary>
        /// The preset this runner is running
        /// </summary>
        [SerializeField]
        private HapticPreset m_preset;

        /// <summary>
        /// Multiplies the output intensity
        /// </summary>
        [SerializeField]
        private float m_intensityMultiplier;

        #endregion

        #region Private Variables

        private IEnumerator coroutine;                                                              // the active running coroutine, either the Execute routine or the Stop routine.

        private Timer timer;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialise and start the haptic preset.
        /// </summary>
        /// <param name="affectedEMSParts"></param>
        /// <param name="properties"></param>
        /// <param name="intensityMultiplier"></param>
        /// <param name="beginActive"></param>
        public void SetupRunner(List<BodyPart> affectedEMSParts, HapticPreset properties, float intensityMultiplier = 1, bool beginActive = true, bool keepAliveBetweenScenes = false)
        {
            m_affectedBodyParts = affectedEMSParts;
            m_preset = properties;
            m_intensityMultiplier = intensityMultiplier;

            if (!keepAliveBetweenScenes) SceneManager.activeSceneChanged += OnSceneChange;

            SetPauseState(!beginActive);
            if (coroutine != null) StopCoroutine(coroutine);
            StartCoroutine(coroutine = ExecuteHapticPreset());
        }

        /// <summary>
        /// Pauses or unpauses the preset runner.
        /// </summary>
        /// <param name="state"></param>
        public void SetPauseState(bool state)
        {
            if (state)
                SendZeroesToAffectedParts();
            paused = state;

            if (paused)
            {
                Debug.LogWarning("[HapticPresetRunner] Pausing a preset currently causes it to skip the segment it is playing. This will be fixed in the next release");
            }
        }

        /// <summary>
        /// Provide a time betweeen 0 and 1 and this will set the current preset to that mapped time
        /// </summary>
        /// <param name="newTime"></param>
        public void SkipTo(float newTime)
        {
            newTime = Mathf.Clamp(newTime, 0, 1);
            SetTimeStarted(timer.TimeStarted - newTime.MapToRange(0, 1, 0, m_preset.totalSegmentTime));
        }

        /// <summary>
        /// Set the time this preset started         
        /// </summary>
        /// <param name="newTime"></param>
        public void SetTimeStarted(float newTime)
        {
            if (newTime < Time.time - m_preset.totalSegmentTime)
            {
                Debug.LogError("[Haptic Preset Runner] Cannot set start time that far back as it will cause the preset to instantly complete");
            }

            timer.SetStartTime(newTime);
        }

        /// <summary>
        /// Stops the preset runner.
        /// </summary>
        public void Stop()
        {
            Debug.Log("[Haptic Preset Runner] Stop");
            if (this != null)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }
                StartCoroutine(coroutine = StopHapticPreset());
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// on scene change, stop the haptic preset execution.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        private void OnSceneChange(Scene arg0, Scene arg1)
        {
            Stop();
        }

        /// <summary>
        /// Sends all zeroes to affected parts. This prevents the intensity from carrying over.
        /// </summary>
        private void SendZeroesToAffectedParts()
        {
            for (int i = 0; i < m_affectedBodyParts.Count; i++)
            {
                OnHapticPresetRequest?.Invoke((int)m_affectedBodyParts[i], 0);
            }
        }

        private IEnumerator StopHapticPreset()
        {
            yield return new WaitForEndOfFrame();
            SendZeroesToAffectedParts();
            Destroy(this);
        }

        private IEnumerator ExecuteHapticPreset()
        {

            float finalIntensity = -1;

        start:

            if (m_preset.totalSegmentTime == 0)
            {
                Debug.LogError("[Haptic Preset Runner] Segments have a total time of 0, cannot run preset");
                yield break;
            }

            if (m_preset.m_segments.Length == 0)
            {
                Debug.LogError("[Haptic Preset Runner] No segments in preset, cannot run");
                yield break;
            }

            // set up the timer and pause if needed
            timer = new Timer(m_preset.totalSegmentTime);
            timer.SetPauseState(paused);

            // cache the segments for easy access
            HapticSegment[] segments = m_preset.m_segments;

            // total time in the segments we've done so far
            float cumulativeSegmentTime = segments[0].m_time;

            // which segment we're currently on
            int currentSegmentIndex = 0;

            float intensity = 0;

            // begin main while loop
            while (!timer.IsTimeOver() || !timer.IsTiming)
            {

                // wait for the frame and loop if we're paused.
                timer.SetPauseState(paused);
                timer.SetIsTiming(!paused);

                // waiting for the end of the frame ensures we're not constantly while looping
                yield return new WaitForEndOfFrame();

                if (paused)
                {
                    continue;
                }


                if (finalIntensity == -1)
                {
                    // are we further than the segment we're looking at? If so, increase index and cumulative time
                    if (timer.GetTimeSinceStart() > cumulativeSegmentTime)
                    {
                        // only increase the index if we aren't over the maximum segments
                        if (currentSegmentIndex < segments.Length - 1)
                        {
                            currentSegmentIndex++;
                            cumulativeSegmentTime += segments[currentSegmentIndex].m_time;
                        }
                        else
                        {
                            // if we are over the maximum segments, break the while loop
                            OutputIntensity();
                            break;
                        }
                    }

                    // check for bridges and find the intensity points
                    (float, float) points = BridgeAndPointCheck();

                    // find the intensity by mapping the time across the segment to the 2 intensities
                    intensity = ValkyrieEIRExtensionMethods.MapToRange(timer.GetTimeSinceStart(), cumulativeSegmentTime - segments[currentSegmentIndex].m_time, cumulativeSegmentTime, points.Item1, points.Item2);
                    
                    float highClamp = Mathf.Max(points.Item1, points.Item2);
                    highClamp = Mathf.Clamp01(highClamp);
                    float lowClamp = Mathf.Min(points.Item1, points.Item2);
                    lowClamp = Mathf.Clamp01(lowClamp);

                    intensity = Mathf.Clamp(intensity, lowClamp, highClamp);
                }
                else
                {
                    intensity = finalIntensity;
                }

                if (float.IsNaN(intensity))
                {
                    intensity = 0;
                }

                if (float.IsNaN(m_intensityMultiplier))
                {
                    m_intensityMultiplier = 0;
                }

                OutputIntensity();
            }

            // if we are looping, restart the coroutine
            if (m_preset.m_loopType != HapticPreset.LoopType.None)
            {
                if (m_preset.m_loopType == HapticPreset.LoopType.LoopFinalIntensity)
                {
                    finalIntensity = intensity;
                }

                // we could use another loop here instead but goto is cleaner here
                goto start;
            }
            else
            {
                Stop();
            }

            void OutputIntensity()
            {
                // send out a signal to each part that we affect
                for (int i = 0; i < m_affectedBodyParts.Count; i++)
                {
                    OnHapticPresetRequest?.Invoke((int)m_affectedBodyParts[i], intensity * m_intensityMultiplier);
                }
            }

            // local function that finds the bridges or points needed
            (float, float) BridgeAndPointCheck()
            {
                float point1;
                float point2;

                if (segments[currentSegmentIndex].usePrevAsPoint1)
                {
                    if (currentSegmentIndex > 0)
                        point1 = segments[currentSegmentIndex - 1].m_point2;
                    else
                        point1 = segments[segments.Length - 1].m_point2;
                }
                else
                    point1 = segments[currentSegmentIndex].m_point1;


                if (segments[currentSegmentIndex].useNextAsPoint2)
                {
                    if (currentSegmentIndex < segments.Length - 1)
                        point2 = segments[currentSegmentIndex + 1].m_point1;
                    else
                        point2 = segments[0].m_point1;
                }
                else
                    point2 = segments[currentSegmentIndex].m_point2;


                return (point1, point2);
            }

        }

        #endregion

    }
}
