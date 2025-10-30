using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private float _delay;
    [SerializeField]
    private Transform _boardPointUp;
    [SerializeField]
    private Transform _boardPointDown;
    [SerializeField]
    private Transform _container;
    [SerializeField]
    private Resource _prefabRes;
    [SerializeField] 
    private LayerMask _deniyLayer;

    private WaitForSeconds _wait;
    private Vector3 _spawnPosition;

    const int maxAttempts = 10;

    void Start()
    {
        _wait = new WaitForSeconds(_delay);
        StartCoroutine(SpawnResource());
    }

    //private IEnumerator SpawnResource()
    //{
    //    while (enabled)
    //    {

    //        _spawnPosition = new Vector3(
    //            Random.Range(_boardPointDown.position.x-4.0f, _boardPointUp.position.x+4.0f),
    //            1,
    //            Random.Range(_boardPointDown.position.z-2.20f, _boardPointUp.position.z+2.20f));

    //        Instantiate(_prefabRes, _spawnPosition, Quaternion.identity, _container);

    //        yield return _wait;
    //    }
    //}





    private IEnumerator SpawnResource()
    {
        while (enabled)
        {
            Vector3 potentialPosition;
            bool positionValid = false;
            int attempts = 0;

            do
            {
                potentialPosition = new Vector3(
                    Random.Range(_boardPointDown.position.x - 4.0f, _boardPointUp.position.x + 4.0f),
                    1,
                    Random.Range(_boardPointDown.position.z - 2.20f, _boardPointUp.position.z + 2.20f));

                float checkRadius = 1.5f;
                positionValid = !Physics.CheckSphere(potentialPosition, checkRadius, _deniyLayer);

                attempts++;
            }
            while (!positionValid && attempts < maxAttempts);

            if (positionValid)
            {
                Instantiate(_prefabRes, potentialPosition, Quaternion.identity, _container);
            }

            yield return _wait;
        }
    }


    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.TryGetComponent(out Base spawnOnBase))
    //    {
    //        if (spawnOnBase != null)
    //        {

    //            this.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    //        }
    //    }
    //}

    //private IEnumerator SpawnResource()
    //{
    //    while (enabled)
    //    {
    //        _spawnPosition = new Vector3(
    //            Random.RandomRange(_boardPointDown.position.x - 6, _boardPointUp.position.x + 6),
    //            0,
    //            Random.RandomRange(_boardPointDown.position.z - 3, _boardPointUp.position.z + 3));

    //        Instantiate(_prefabRes, _spawnPosition, Quaternion.identity, _container);

    //        yield return _wait;
    //    }
    //}
}
