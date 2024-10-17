using UnityEngine;

namespace Valkyrie.EIR.Utilities {

    /// <summary>
    /// Utility class to handle timing operations.
    /// The timescale is linked to Unity's Time.time property.
    /// </summary>
    [System.Serializable]
    public struct Timer {

        #region Public Properties

        /// <summary>
        /// Returns the timestamp of when this timer was started.
        /// </summary>
        public float TimeStarted { get; private set; }

        /// <summary>
        /// Returns the time alloted to this timer.
        /// </summary>
        public float TimeAlloted { get; private set; }

        /// <summary>
        /// Returns true if the timer is paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Returns true if the timer is actively timing.
        /// </summary>
        public bool IsTiming { get; private set; }


        #endregion

        #region Private Variables

        private float timePaused;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a timer object with the input timespan.
        /// </summary>
        /// <param name="timeToComplete"></param>
        public Timer(float timeToComplete) {
            TimeAlloted = timeToComplete;
            TimeStarted = Time.time;
            IsTiming = true;
            timePaused = 0;
            IsPaused = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the timer to the beginning.
        /// </summary>
        /// <param name="timeTillComplete"></param>
        public void ResetTiming(float timeTillComplete) {
            TimeAlloted = timeTillComplete;
            TimeStarted = Time.time;
            IsTiming = true;
        }

        /// <summary>
        /// Sets whether the timer is timing.
        /// </summary>
        /// <param name="target"></param>
        public void SetIsTiming(bool target) {
            IsTiming = target;
        }

        /// <summary>
        /// Returns true if the timer has reached 0.
        /// </summary>
        /// <returns></returns>
        public bool IsTimeOver() {
            if (Time.time > TimeStarted + TimeAlloted && IsTiming) {
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Returns the remaining time left on this timer.
        /// </summary>
        /// <returns></returns>
        public float GetRemainingTime() {
            float timeRemaining = TimeStarted + TimeAlloted - Time.time;

            if (timeRemaining < 0) {
                return 0;
            } else {
                return timeRemaining;
            }
        }

        /// <summary>
        /// Returns the delta between the current time and the time this timer was started.
        /// </summary>
        /// <returns></returns>
        public float GetTimeSinceStart() {
            return Time.time - TimeStarted;
        }

        /// <summary>
        /// Returns the time maped to a range of time started, the time it should complete within a given range.
        /// </summary>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public float GetMappedTime(float minimum, float maximum) {
            return Time.time.MapToRange(TimeStarted, TimeStarted + TimeAlloted, minimum, maximum);
        }

        /// <summary>
        /// Returns the time formatted to be human readable.
        /// Optional parameter can add one second in cases where the timer does not need to be zero indexed.
        /// </summary>
        /// <param name="plusOneSecond"></param>
        /// <returns></returns>
        public string GetFormattedRemainingTime(bool plusOneSecond = false) {
            float time = GetRemainingTime();

            int minutes = (int)time / 60;
            int seconds = (int)time % 60;

            if (plusOneSecond)
                seconds += 1;

            string timeString = "";

            if (minutes < 10)
                timeString += "0" + minutes + ":";
            else
                timeString += minutes + ":";

            if (seconds < 10)
                timeString += "0" + seconds;
            else
                timeString += seconds;

            return timeString;
        }

        /// <summary>
        /// Sets the pause state of the timer.
        /// </summary>
        /// <param name="pause"></param>
        public void SetPauseState(bool pause) {
            if (pause) {
                if (IsPaused)
                    return;

                timePaused = Time.time;
                IsPaused = true;
            } else {
                if (!IsPaused)
                    return;

                float timeDifference = Time.time - timePaused;

                TimeStarted += timeDifference;
                IsPaused = false;
            }
        }

        /// <summary>
        /// Sets the starting time of the timer in cases where the timer should not be started immediately.
        /// </summary>
        /// <param name="newStartTime"></param>
        public void SetStartTime(float newStartTime) {
            TimeStarted = newStartTime;
        }

        #endregion
    }

}

