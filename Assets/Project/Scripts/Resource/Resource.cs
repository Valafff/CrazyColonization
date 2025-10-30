using System;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public bool onBase { get; set; } = false;
    public Guid baseOwner { get; set; } = Guid.Empty;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Base spawnOnBase))
        {
            if (spawnOnBase != null)
            {
                onBase = true;
                //this.transform.position = new Vector3(transform.position.x+1, transform.position.y+10, transform.position.z+1);
                //this.transform.position = new Vector3(transform.position.x, transform.position.y - 100000, transform.position.z);
            }
        }
    }
    private void FixedUpdate()
    {
        if (this != null && transform.position.y < -99000f && baseOwner == Guid.Empty)
        {
            Destroy(this);
        }
    }
}
