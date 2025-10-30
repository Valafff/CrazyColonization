using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.GraphicsBuffer;

public class DroneMover : MonoBehaviour
{
    private Guid _baseOwner;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private Transform _baseCoord;
    [SerializeField]
    private Transform[] _wayPoints;
    [SerializeField]
    private Transform _cargoPlace;
    [SerializeField]
    private Base _basePrefab;



    [Header("Приземление")]
    [SerializeField] private LayerMask groundMask;        // только земля для raycast
    [SerializeField] private float groundOffset = 0.05f;  // избежать мерцания в плоскости
    [SerializeField] private float groundProbeUp = 2.0f;  // старт луча над дроном
    [SerializeField] private float groundProbeDown = 5.0f;


    private int _currentWayPoint = 0;
    private Resource _tempResource = null;
    private GameObject _beacon = null;
    private bool _isHaveCommand = false;
    private bool isHaveCommandToBuild = false;
    private bool _isHaveResource = false;
    private int currentWayPont = 0;

    //Для инициализации дрона при постройке
    private void Start()
    {
        GroundClamp();
    }

    public void DroneInicialization(Transform baseTransform, Transform[] wayPoints, Guid guid)
    {
        _baseCoord = baseTransform;
        _wayPoints = wayPoints ?? System.Array.Empty<Transform>();
        currentWayPont = 0;
        _baseOwner = guid;

        GroundClamp();
    }

    //public DroneMover(Transform baseTransform, Transform[] wayPoints)
    //{
    //    _baseCoord = baseTransform;
    //    _wayPoints = wayPoints ?? System.Array.Empty<Transform>();
    //    currentWayPont = 0;

    //    GroundClamp();
    //}

    // ---------- Прилеп к земле ----------
    private void GroundClamp()
    {
        // луч вниз от точки над дроном
        Vector3 probeStart = transform.position + Vector3.up * groundProbeUp;
        float maxDist = groundProbeUp + groundProbeDown;

        if (Physics.Raycast(probeStart, Vector3.down, out var hit, maxDist, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 p = hit.point + Vector3.up * groundOffset;
            transform.position = new Vector3(transform.position.x, p.y, transform.position.z);
        }
        // иначе не трогаем Y — значит, не попали в землю (mask/коллайдер)
    }

    private void FixedUpdate()
    {
        if (_isHaveCommand)
        {
            if (_isHaveResource)
            {
                var dist = Vector3.Distance(this.transform.position, _tempResource.transform.position);

                //if (dist > 3)
                //{
                //    transform.DetachChildren();
                //    _isHaveResource = false;
                //    _isHaveCommand = false;
                //    Destroy(_tempResource);
                //    _tempResource = null;
                //}

                //Едем кбазе
                MoveToTarget(_baseCoord);
            }
            else
            {
                //Едем к ресурсу
                
                MoveToTarget(_tempResource.transform);
            }
        }
        else if (isHaveCommandToBuild)
        {
            MoveToTarget(_beacon.transform);
        }
        else
        {
            FreeMove();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out Point point))
        {
            _currentWayPoint = ++_currentWayPoint % _wayPoints.Length;
        }

        //if (other.gameObject.TryGetComponent(out Resource resource))
        //{
        //    if (_tempResource != null && resource.transform.position == _tempResource.transform.position)
        //    {
        //        //Грузим ресурс
        //        PickUpResource(resource);
        //    }
        //}
        //else if (other.gameObject.TryGetComponent(out Base baseComponent) && _isHaveResource)
        //{
        //    baseComponent.AddDroneToQueue(this);
        //    DropDownResource();
        //}
        if (other.gameObject.TryGetComponent(out Base baseComponent) && _isHaveResource)
        {
            baseComponent.AddDroneToQueue(this);
            DropDownResource();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Resource resource))
        {
            if (_tempResource != null && resource.transform.position == _tempResource.transform.position)
            {
                //Грузим ресурс
                PickUpResource(resource);
            }
        }
        else if (collision.gameObject.TryGetComponent(out Base baseComponent) && _isHaveResource)
        {
            DropDownResource();
            baseComponent.AddDroneToQueue(this);
        }
        else if (collision.gameObject.TryGetComponent(out BaseBeacon baseBeacon))
        {
            Vector3 v3 = new Vector3(_beacon.transform.position.x, _beacon.transform.position.y + 0.15f, _beacon.transform.position.z);
            Base newBase = Instantiate(_basePrefab, v3, Quaternion.identity);

            _baseCoord = newBase.transform;
            _wayPoints = newBase.GetBaseWayPoints();
            newBase.AddToDronePool(this);
            isHaveCommandToBuild = false;

            var data = _beacon.GetComponent<BaseBeacon>();
            data.isActive = false;

            _beacon.transform.position = new Vector3(1000, 1000, 1000);
            _beacon = null;
        }
    }


    public void TakeResource(Resource resource)
    {
        _tempResource = resource;
        _isHaveCommand = true;
    }

    public void MoveToBeaconCommand(GameObject beacon)
    {
        if (beacon == null) return;

        this._beacon = beacon;
        var beaconTransform = beacon.transform;
        transform.position = Vector3.MoveTowards(transform.position, beaconTransform.position, _speed * Time.deltaTime);
        var look = beaconTransform.position; look.y = transform.position.y;
        transform.LookAt(look);
        isHaveCommandToBuild = true;
    }


    //private void FreeMove()
    //{
    //    transform.position = Vector3.MoveTowards(transform.position, _wayPoints[_currentWayPoint].position, _speed * Time.deltaTime);
    //    transform.LookAt(_wayPoints[_currentWayPoint]);
    //}

    private bool FreeMove()
    {
        if (_wayPoints == null || _wayPoints.Length == 0) return false;
        if (currentWayPont < 0 || currentWayPont >= _wayPoints.Length)
            currentWayPont = 0;

        MoveTo(_wayPoints[currentWayPont]);

        if (Vector3.SqrMagnitude(transform.position - _wayPoints[currentWayPont].position) < 0.04f)
            currentWayPont = (currentWayPont + 1) % _wayPoints.Length;

        return true;
    }

    private void MoveTo(Transform target)
    {
        if (target == null) return;
        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);
        var look = target.position; look.y = transform.position.y;
        transform.LookAt(look);
    }

    public void MoveToTarget(Transform target)
    {
        if (target == null || !target.gameObject)
            return;

        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);
        transform.LookAt(target);
    }

    private void PickUpResource(Resource resource)
    {
        resource.transform.SetParent(this.transform);


        ////resource.transform.position = _cargoPlace.position;
        //Rigidbody rb = resource.GetComponent<Rigidbody>();
        //rb.detectCollisions = false;
        //Collider collider = resource.GetComponent<Collider>();
        //collider.enabled = false;

        _isHaveResource = true;
    }


    private void DropDownResource()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.TryGetComponent(out Resource resource))
            {
                _tempResource = null;
                resource.transform.SetParent(null);
                resource.baseOwner = Guid.Empty;
                Destroy(resource.gameObject);
                _isHaveCommand = false;
                _isHaveResource = false;

            }
        }
    }


    public void SetNewBeaconPlace(GameObject beacon)
    {
        this._beacon = beacon;
    }
    public void SetWayPoints(Transform[] points)
    {
        _wayPoints = points;
    }
    public Transform[] GetWayPoints()
    {
        return _wayPoints;
    }
    public void SetOwner(Guid guid)
    {
        _baseOwner = guid;
    }
    public Guid GetOwner()
    {
        return _baseOwner;
    }
}
