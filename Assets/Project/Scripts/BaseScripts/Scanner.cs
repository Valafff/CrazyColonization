using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    [SerializeField] private float scanRadius;
    [SerializeField] private LayerMask layerMask;
    public Queue<Resource> Scan(Queue<Resource> resources, Guid baseGuid)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, scanRadius, layerMask);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out Resource resource))
            {
                if (resource!= null && !resources.Contains(resource) && !resource.onBase && resource.baseOwner == Guid.Empty)
                {
                    resource.baseOwner = baseGuid;
                    resources.Enqueue(resource);
                }
            }
        }
        return resources;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
}
