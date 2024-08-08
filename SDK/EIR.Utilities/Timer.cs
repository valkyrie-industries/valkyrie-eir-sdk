using UnityEngine;

namespace Valkyrie.EIR.Utilities
{

    [System.Serializable]
    public struct Timer
    {
        public float timeStarted { get; private set; }
        public float timeGiven;

        float timePaused;
        public bool isPaused { get; private set; }

        public bool isTiming { get; private set; }

        public Timer(float timeToComplete)
        {
            timeGiven = timeToComplete;
            timeStarted = Time.time;
            isTiming = true;
            timePaused = 0;
            isPaused = false;
        }

        public void ResetTiming(float timeTillComplete)
        {
            timeGiven = timeTillComplete;
            timeStarted = Time.time;
            isTiming = true;
        }

        public bool IsTimeOver()
        {
            if (Time.time > timeStarted + timeGiven && isTiming)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetIsTiming(bool target)
        {
            isTiming = target;
        }

        public float GetRemainingTime()
        {
            float timeRemaining = timeStarted + timeGiven - Time.time;

            if (timeRemaining < 0)
            {
                return 0;
            }
            else
            {
                return timeRemaining;
            }
        }

        public float GetTimeSinceStart()
        {
            return Time.time - timeStarted;
        }

        public float GetMappedTime(float minimum, float maximum)
        {
            return Time.time.MapToRange(timeStarted, timeStarted + timeGiven, minimum, maximum);
        }

        public string GetFormattedRemainingTime(bool plusOneSecond = false)
        {
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

        public void SetPauseState(bool pause)
        {
            if (pause)
            {
                if (isPaused)
                    return;

                timePaused = Time.time;
                isPaused = true;
            }
            else
            {
                if (!isPaused)
                    return;

                float timeDifference = Time.time - timePaused;

                timeStarted += timeDifference;
                isPaused = false;
            }
        }

        public void SetStartTime(float newStartTime)
        {
            timeStarted = newStartTime;
        }


    }

}

