using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICalibrationObject 
{
    void InitialiseCalibrationLimits(float min, float max);
}
