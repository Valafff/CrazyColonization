using System;
using UnityEngine;

public class BaseBeacon : MonoBehaviour
{
    public Guid BaseGuid { get; set; }
    public bool isActive { get; set; } = false;
}
