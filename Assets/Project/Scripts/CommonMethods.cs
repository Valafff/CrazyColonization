using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommonMethods : MonoBehaviour
{
    //static string currentObjectName = string.Empty;
    static Guid selectedGuid = Guid.Empty;
    static Renderer oldObjectRender = null;
    static List<(Guid, GameObject)> BeaconList = new List<(Guid, GameObject)>();

    public static Guid GetSelectedId()
    {
        return selectedGuid;
    }

    public static void SelectNewBase(Base baseObj, RaycastHit hit, Color newColor, Color defaultColor)
    {
        if (selectedGuid == Guid.Empty)
        {
            Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
            selectedGuid = baseObj.GetBaseId();
            oldObjectRender = renderer;

            if (renderer != null)
            {
                renderer.material.color = newColor;
            }
        }
        if (baseObj.GetBaseId() != selectedGuid)
        {
            if (oldObjectRender != null)
            {
                oldObjectRender.material.color = defaultColor;
            }
            Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
            selectedGuid = baseObj.GetBaseId();
            oldObjectRender = renderer;

            if (renderer != null)
            {
                renderer.material.color = newColor;
            }
        }
        //Debug.Log($"Имя объекта: {hit.collider.gameObject.name}");
    }

    public static GameObject SetNewBeacon(RaycastHit hit, GameObject beacon)
    {
        if (selectedGuid != Guid.Empty)
        {
            if (!BeaconList.Any(b => b.Item1 == selectedGuid))
            {
                var activeNewBasePlace = Instantiate(beacon, hit.point, Quaternion.identity);

                var data = activeNewBasePlace.GetComponent<BaseBeacon>();
                data.BaseGuid = selectedGuid;
                data.isActive = true;

                BeaconList.Add((selectedGuid, activeNewBasePlace));
                return activeNewBasePlace;
            }
            else
            {
                var temp = BeaconList.First(t => t.Item1 == selectedGuid);
                BeaconList.Remove(temp);
                Destroy(temp.Item2);

                var activeNewBasePlace = Instantiate(beacon, hit.point, Quaternion.identity);

                var data = activeNewBasePlace.GetComponent<BaseBeacon>();
                data.BaseGuid = selectedGuid;
                data.isActive = true;

                BeaconList.Add((selectedGuid, activeNewBasePlace));
                return activeNewBasePlace;
            }
        }
        return beacon;
    }

    public static void RemoveBeacon(GameObject beacon)
    {
        var target = BeaconList.FirstOrDefault(b => b.Item2 == beacon);

        if (target.Item2 != null)
        {
            BeaconList.Remove(target);
        }

    }
}





//using NUnit.Framework;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class CommonMethods : MonoBehaviour
//{
//    static string currentObjectName = string.Empty;
//    static Renderer oldObjectRender = null;
//    static List<(string, GameObject)> BeaconList = new List<(string, GameObject)>();

//    public static void SelectNewBase(RaycastHit hit, Color newColor, Color defaultColor)
//    {
//        if (string.IsNullOrEmpty(currentObjectName))
//        {
//            Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
//            currentObjectName = hit.collider.gameObject.name;
//            oldObjectRender = renderer;

//            if (renderer != null)
//            {
//                renderer.material.color = newColor;
//            }
//        }
//        if (hit.collider.gameObject.name != currentObjectName)
//        {
//            if (oldObjectRender != null)
//            {
//                oldObjectRender.material.color = defaultColor;
//            }
//            Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
//            currentObjectName = hit.collider.gameObject.name;
//            oldObjectRender = renderer;

//            if (renderer != null)
//            {
//                renderer.material.color = newColor;
//            }
//        }
//        //Debug.Log($"Имя объекта: {hit.collider.gameObject.name}");
//    }

//    public static void SetNewBeacon(RaycastHit hit, GameObject newBasePrefab)
//    {
//        if (!string.IsNullOrEmpty(currentObjectName))
//        {
//            if (!BeaconList.Any(b => b.Item1 == currentObjectName))
//            {
//                var activeNewBasePlace = Instantiate(newBasePrefab, hit.point, Quaternion.identity);
//                BeaconList.Add((currentObjectName, activeNewBasePlace));
//            }
//            else
//            {
//                var temp = BeaconList.First(t => t.Item1 == currentObjectName);
//                BeaconList.Remove(temp);
//                Destroy(temp.Item2);

//                var activeNewBasePlace = Instantiate(newBasePrefab, hit.point, Quaternion.identity);
//                BeaconList.Add((currentObjectName, activeNewBasePlace));
//            }
//        }

//    }
//}
