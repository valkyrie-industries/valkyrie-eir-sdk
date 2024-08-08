
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
    public class HapticPresetRunner : MonoBehaviour {

        public delegate void OnHapticPresetRequestEventHandler(int bodyPart, float intensity);
        public static event OnHapticPresetRequestEventHandler OnHapticPresetRequest;

        public List<BodyPart> m_affectedBodyParts { get; private set; } = new List<BodyPart>();     // which body parts will this preset affect during execution.

        public HapticPreset m_preset;                                                               // the preset this runner is running.

        public float m_intensityMultiplier;                                                         // the multiplier that affects this runner's intensity.

        public bool paused { get; private set; } = false;                                                                // temporarilly halts execution of the preset.
        private IEnumerator coroutine;                                                              // the active running coroutine, either the Execute routine or the Stop routine.

        private Timer timer;

        /// <summary>
        /// Initialise and start the haptic preset.
        /// </summary>
        /// <param name="affectedEMSParts"></param>
        /// <param name="properties"></param>
        /// <param name="startIntensity"></param>
        /// <param name="runNow"></param>
        public void SetupRunner(List<BodyPart> affectedEMSParts, HapticPreset properties, float startIntensity = 1, bool runNow = true, bool keepalivebetweenscenes = false) {
            m_affectedBodyParts = affectedEMSParts;
            m_preset = properties;
            m_intensityMultiplier = startIntensity;

            if (!keepalivebetweenscenes) SceneManager.activeSceneChanged += OnSceneChange;

            SetPauseState(!runNow);
            if (coroutine != null) StopCoroutine(coroutine);
            StartCoroutine(coroutine = ExecuteHapticPreset());
        }

        /// <summary>
        /// Pauses or unpauses the preset runner.
        /// </summary>
        /// <param name="state"></param>
        public void SetPauseState(bool state) {
            if (state)
                SendZeroesToAffectedParts();
            paused = state;
        }

        /// <summary>
        /// Provide a time betweeen 0 and 1 and this will set the current preset to that mapped time
        /// </summary>
        /// <param name="newTime"></param>
        public void SkipTo(float newTime)
        {
            newTime = Mathf.Clamp(newTime, 0, 1);
            SetTimeStarted(timer.timeStarted - newTime.MapToRange(0, 1, 0, m_preset.totalSegmentTime));
            Debug.Log("Skipped backwards this far: " + newTime.MapToRange(0, 1, 0, m_preset.totalSegmentTime));
        }

        /// <summary>
        /// Set the time this preset started         
        /// </summary>
        /// <param name="newTime"></param>
        public void SetTimeStarted(float newTime)
        {
            if(newTime < Time.time - m_preset.totalSegmentTime)
            {
                Debug.LogError("[HapticPresetRunner] Cannot set start time that far back as it will cause the preset to instantly complete");
            }

            timer.SetStartTime(newTime);
        }

        /// <summary>
        /// Stops the preset runner.
        /// </summary>
        public void Stop() {
            Debug.Log("[Haptic Preset Runner] Stop");
            if (this != null) {
                if (coroutine != null) {
                    StopCoroutine(coroutine);
                }
                StartCoroutine(coroutine = StopHapticPreset());
            }
        }

        /// <summary>
        /// on scene change, stop the haptic preset execution.
        /// </summary>
        /// <param name="arg0"></param>
        /// <param name="arg1"></param>
        private void OnSceneChange(Scene arg0, Scene arg1) {
            Stop();
        }

        /// <summary>
        /// Sends all zeroes to affected parts. This prevents the intensity from carrying over.
        /// </summary>
        private void SendZeroesToAffectedParts() {
            for (int i = 0; i < m_affectedBodyParts.Count; i++) {
                OnHapticPresetRequest?.Invoke((int)m_affectedBodyParts[i], 0);
            }
        }

        private IEnumerator StopHapticPreset() {
            yield return new WaitForEndOfFrame();
            SendZeroesToAffectedParts();
            Destroy(this);
        }

        private IEnumerator ExecuteHapticPreset() {

            float finalIntensity = -1;

        start:

            if (m_preset.totalSegmentTime == 0) {
                Debug.LogError("[Haptic Preset Runner] Segments have a total time of 0, cannot run preset");
                yield break;
            }

            if (m_preset.m_segments.Length == 0) {
                Debug.LogError("[Haptic Preset Runner] No segments in preset, cannot run");
                yield break;
            }

            //Set up the timer and pause if needed
            timer = new Timer(m_preset.totalSegmentTime);
            timer.SetPauseState(paused);

            //Cache the segments for easy access
            HapticSegment[] segments = m_preset.m_segments;

            //Total time in the segments we've done so far
            float cumulativeSegmentTime = segments[0].m_time;

            //which segment we're currently on
            int currentSegmentIndex = 0;

            float intensity = 0;

            //Begin main while loop
            while (!timer.IsTimeOver()) {
                
                //Wait for the frame and loop if we're paused.
                timer.SetPauseState(paused);

                //Waiting for the end of the frame ensures we're not constantly while looping
                yield return new WaitForEndOfFrame();

                if (paused)
                    continue;

                if(finalIntensity == -1)
                {
                    //Are we further than the segment we're looking at? If so, increase index and cumulative time
                    if (timer.GetTimeSinceStart() > cumulativeSegmentTime)
                    {
                        //Only increase the index if we aren't over the maximum segments
                        if (currentSegmentIndex < segments.Length - 1)
                        {
                            //Debug.Log("Increased segment index");
                            currentSegmentIndex++;
                            cumulativeSegmentTime += segments[currentSegmentIndex].m_time;
                        }
                        else
                        {
                            //If we are over the maximum segments, break the while loop
                            break;
                        }
                    }

                    //Check for bridges and find the intensity points
                    (float, float) points = BridgeAndPointCheck();

                    //Find intensity by mapping the time across the segment to the 2 intensities
                    intensity = ValkyrieEIRExtensionMethods.MapToRange(timer.GetTimeSinceStart(), cumulativeSegmentTime - segments[currentSegmentIndex].m_time, cumulativeSegmentTime, points.Item1, points.Item2);
                    intensity = Mathf.Clamp(intensity, 0, 1);

                    //Debug.Log("Points: " + points.Item1 + " " + points.Item2);
                    //Debug.Log("Raw intensity: " + intensity);
                    //Debug.Log("Multiplied intensity: " + intensity * m_intensityMultiplier);
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

                //Send out a signal to each part that we affect
                for (int i = 0; i < m_affectedBodyParts.Count; i++) {
                    //Debug.Log("sending intensity of " + intensity * m_intensityMultiplier);
                    OnHapticPresetRequest?.Invoke((int)m_affectedBodyParts[i], intensity * m_intensityMultiplier);
                }
            }

            //If we are looping, restart the coroutine
            if (m_preset.m_loopType != HapticPreset.LoopType.None) {

                if(m_preset.m_loopType == HapticPreset.LoopType.LoopFinalIntensity)
                {
                    finalIntensity = intensity;
                }

                //We could use another loop here instead but goto is cleaner here
                goto start;
            }
            else {
                Stop();
            }

            //Local function that finds the bridges or points needed
            (float, float) BridgeAndPointCheck() {
                float point1;
                float point2;

                if (segments[currentSegmentIndex].usePrevAsPoint1) {
                    if (currentSegmentIndex > 0)
                        point1 = segments[currentSegmentIndex - 1].m_point2;
                    else
                        point1 = segments[segments.Length - 1].m_point2;
                }
                else
                    point1 = segments[currentSegmentIndex].m_point1;


                if (segments[currentSegmentIndex].useNextAsPoint2) {
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
    }
}
