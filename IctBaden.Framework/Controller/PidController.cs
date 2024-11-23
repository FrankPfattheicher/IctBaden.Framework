using System;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace IctBaden.Framework.Controller;

public class PidController
{
    public double SetPoint { get; set; }
    public double Kp { get; set; }
    public double Ki { get; set; }
    public double Kd { get; set; }
    public double ControlMin { get; set; }
    public double ControlMax { get; set; }
    public double ErrorMax { get; set; }

    private DateTime _lastControlUtc;
    private double _integral;

    private double _output;
    private double _preError;

    public bool Error => Math.Abs(_integral) > ErrorMax;

    public PidController()
    {
        ControlMin = double.MinValue;
        ControlMax = double.MaxValue;
        ErrorMax = double.MaxValue;
        Reset();
    }

    public void Reset()
    {
        Reset(DateTime.UtcNow);
    }
    public void Reset(DateTime timeStamp)
    {
        _lastControlUtc = timeStamp;
        _integral = 0.0;
        _preError = 0.0;
    }

    public double Control(double actual)
    {
        return Control(actual, DateTime.UtcNow);
    }
    public double Control(double actual, DateTime timeStamp)
    {
        // calculate time between calculation
        var dt = (timeStamp - _lastControlUtc).TotalSeconds;
        _lastControlUtc = timeStamp;

        // calculate the difference between
        // the desired value and the actual value
        var error = SetPoint - actual;

        // track error over time, scaled to the timer interval
        _integral += error * dt;

        // determine the amount of change from the last time checked
        var derivative = (dt > 0.0) ? ((error - _preError) / dt) : 0.0;

        // calculate how much to drive the output in order to get to the 
        // desired set-point. 
        _output += (Kp * error) + (Ki * _integral) + (Kd * derivative);

        // limit control value
        if (_output < ControlMin)
        {
            _output = ControlMin;
        }
        if (_output > ControlMax)
        {
            _output = ControlMax;
        }

        // remember the error for the next time around.
        _preError = error;

        return _output;
    }


}