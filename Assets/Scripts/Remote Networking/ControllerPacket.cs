using System;
using UnityEngine;

[Serializable]
public class ControllerPacket
{
    public string type;
    public string phase;
    public long id;
    public double timestamp;

    public float x;
    public float y;

    public float pitch;
    public float roll;
    public float yaw;
    public float gx;
    public float gy;
    public float gz;
    public float ax;
    public float ay;
    public float az;
}