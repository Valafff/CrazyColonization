using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;

[RequireComponent(typeof(Scanner))]
public class Base : MonoBehaviour
{
    [SerializeField] private KeyCode scannerKey;

    private Scanner _scanner;
    private Queue<Resource> _resources = new Queue<Resource>();


    [Header("Настройки базы")]
    private Guid baseId;
    [SerializeField]
    private string baseName;
    [SerializeField]
    private Transform baseCoords;
    [SerializeField]
    private ResourceCounter resourceCounter;
    [Header("Настройки дронов")]
    [SerializeField]
    private int droneLimit;
    [SerializeField]
    private DroneMover dronePrefab;
    [SerializeField]
    private DroneMover[] droneMoversArray;

    [Header("Строительство новой базы")]
    [SerializeField]
    private LayerMask groundMask;
    [SerializeField]
    private GameObject baseBeacon;
    [SerializeField]
    private Point wayPointPrefab;

    private Queue<DroneMover> dronesPool = new Queue<DroneMover>();
    private bool isBuildingMode = false;
    private bool waitScanCommand = false;
    private DroneMover droneBuilder = null;


    private Transform[] droneWayPoints;


    private void Awake()
    {
        baseId = Guid.NewGuid();
        WayPointInitialization(3);
        if (baseName == "noName")
        {
            baseName = "Base_" + DateTime.Now.ToString();
        }

    }

    public void Start()
    {
        _scanner = GetComponent<Scanner>();

        if (droneMoversArray != null && droneMoversArray.Length > 0)
        {
            foreach (var dron in droneMoversArray)
            {
                if (dron != null)
                {
                    dron.SetOwner(baseId);
                    dron.SetWayPoints(droneWayPoints);
                    dronesPool.Enqueue(dron);
                }
            }
        }
        //if (dronesPool.Count > 0)
        //{
        //    droneWayPoints = dronesPool.First().GetWayPoints();
        //}
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryPlaceBuildSite();
    }

    private void FixedUpdate()
    {
        if (baseBeacon != null && dronesPool.Count > 1 && baseBeacon.GetComponent<BaseBeacon>().isActive)
        {
            var data = baseBeacon.GetComponent<BaseBeacon>();
            if (data.BaseGuid == baseId)
            {
                isBuildingMode = true;
            }
        }
        
        if(!baseBeacon.GetComponent<BaseBeacon>().isActive)
        {
            isBuildingMode = false;
        }

        //Если ресурсов достаточно, включен режим строительства, дронов достаточно и маяк не null - понеслась
        if (isBuildingMode && resourceCounter.GetResourceCount() >= 5 && dronesPool.Count > 1 && baseBeacon != null && droneBuilder == null)
        {
            Debug.Log("Если ресурсов достаточно, включен режим строительства, дронов достаточно и маяк не null - понеслась");

            droneBuilder = dronesPool.Dequeue();
            SendDroneToBeacon(droneBuilder, baseBeacon);
            resourceCounter.SetResourceCount(resourceCounter.GetResourceCount() - 5);
            droneBuilder = null;
        }

        if (!waitScanCommand)
        {
            _resources = _scanner.Scan(_resources, baseId);

            if (dronesPool.Count > 0 && _resources.Count > 0)
            {
                Debug.Log($"Количество дронов {baseName} {dronesPool.Count} до");
                Debug.Log($"{baseName} {_resources.Count}");

                var res = _resources.Dequeue();
                if (res.baseOwner == baseId && res.transform.position.y > 0f)
                {
                    SendDroneToResource(dronesPool.Dequeue(), res);
                }
                else
                {
                    Destroy(res);
                }

  

                Debug.Log($"Количество дронов {baseName} {dronesPool.Count} после");
            }
        }

        //Производство дронов
        if (!isBuildingMode)
        {
            SpawnDroneForBase(baseId);
        }

    }

    private void SendDroneToResource(DroneMover currentDrone, Resource resource)
    {
        //Debug.Log($"{baseName} {resource.transform.position.z}");

        currentDrone.TakeResource(resource);
        waitScanCommand = true;
    }

    private void SendDroneToBeacon(DroneMover currentDrone, GameObject beacon)
    {
        currentDrone.MoveToBeaconCommand(beacon);
    }

    public void AddDroneToQueue(DroneMover drone)
    {
        if (drone.GetOwner() == baseId)
        {
            dronesPool.Enqueue(drone);
            resourceCounter.AddResource();
            waitScanCommand = false;
        }
    }



    //Производство дронов
    private void SpawnDroneForBase(Guid guid)
    {
        if (dronePrefab == null)
        {
            return;
        }

        //Debug.Log($"База {baseName} Количество дронов {dronesPool.Count} Лимит дронов {droneLimit}");

        if (resourceCounter.GetResourceCount() >= 3 && dronesPool.Count < droneLimit - 1)
        {
            Debug.Log($"Количество ресурсов базы {baseName} равно: {resourceCounter.GetResourceCount()}");
            resourceCounter.SetResourceCount(resourceCounter.GetResourceCount() - 3);

            // Безопасная высота спавна (чтоб не проваливался)
            Vector3 pos = baseCoords.position + Vector3.up * 0.4f;
            DroneMover newDrone = Instantiate(dronePrefab, pos, Quaternion.identity);
            newDrone.DroneInicialization(baseCoords, droneWayPoints, guid);


            dronesPool.Enqueue(newDrone);
        }
        else
        {
            return;
        }
    }

    //Строительство новой базы
    private void TryPlaceBuildSite()
    {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, 1000f, groundMask))
        {
            return;
        }

        if (hit.collider.gameObject.GetComponent<Base>())
        {
            var currentBase = hit.collider.gameObject.GetComponent<Base>();
            CommonMethods.SelectNewBase(currentBase, hit, Color.blue, Color.red);
            return;
        }

        //if (hit.collider.gameObject.GetComponent<Ground>() && resourceCounter.GetResourceCount() >= 5 && dronesPool.Count > 1 && CommonMethods.GetSelectedId() == baseId)
        //{
        //    baseBeacon = CommonMethods.SetNewBeacon(hit, baseBeacon);
        //    isBuildingMode = true;
        //    return;
        //}

        if (hit.collider.gameObject.GetComponent<Ground>() && CommonMethods.GetSelectedId() == baseId)
        {
            Debug.Log($"Состояние активности маяка {baseBeacon.GetComponent<BaseBeacon>().isActive}");

            baseBeacon = CommonMethods.SetNewBeacon(hit, baseBeacon);
            if (droneBuilder != null && droneBuilder.GetOwner() == baseId)
            {
                baseBeacon = CommonMethods.SetNewBeacon(hit, baseBeacon);
                SendDroneToBeacon(droneBuilder, baseBeacon);
            }
            else
            {
                baseBeacon = CommonMethods.SetNewBeacon(hit, baseBeacon);
            }

            return;
        }

    }

    private void WayPointInitialization(float ortoDistnce)
    {
        droneWayPoints = new Transform[4];

        Vector3 tmpPos = new Vector3(transform.position.x + ortoDistnce, 0.5f, transform.position.z + ortoDistnce);
        var newWayPoint = Instantiate(wayPointPrefab, tmpPos, Quaternion.identity);
        droneWayPoints[0] = newWayPoint.transform;

        tmpPos = new Vector3(transform.position.x + ortoDistnce, 0.5f, transform.position.z - ortoDistnce);
        newWayPoint = Instantiate(wayPointPrefab, tmpPos, Quaternion.identity);
        droneWayPoints[1] = newWayPoint.transform;

        tmpPos = new Vector3(transform.position.x - ortoDistnce, 0.5f, transform.position.z - ortoDistnce);
        newWayPoint = Instantiate(wayPointPrefab, tmpPos, Quaternion.identity);
        droneWayPoints[2] = newWayPoint.transform;

        tmpPos = new Vector3(transform.position.x - ortoDistnce, 0.5f, transform.position.z + ortoDistnce);
        newWayPoint = Instantiate(wayPointPrefab, tmpPos, Quaternion.identity);
        droneWayPoints[3] = newWayPoint.transform;
    }

    public Guid GetBaseId()
    {
        return baseId;
    }
    public void AddToDronePool(DroneMover drone)
    {
        drone.SetOwner(baseId);
        dronesPool.Enqueue(drone);
    }
    public Transform[] GetBaseWayPoints()
    {
        return droneWayPoints;
    }
}