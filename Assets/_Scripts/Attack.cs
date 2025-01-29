using System;
using UnityEngine;

[Serializable]
public struct Attack
{
    public string attackName;
    public Transform hitbox;
    public float hitboxLength, cooldown;
    public bool cancelOnGroundHit;
}
