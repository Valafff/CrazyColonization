using UnityEngine;

public class CameraMover : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    private Transform _boardPointUp;
    [SerializeField]
    private Transform _boardPointDown;

    private float xPos = 0;
    private float zPos = 0;

    public void Update()
    {
        xPos = Input.GetAxis("Horizontal");
        zPos = Input.GetAxis("Vertical");

        Vector3 vector = new Vector3(xPos * _speed * Time.deltaTime, 0, zPos * _speed * Time.deltaTime);
        transform.Translate(vector);

        //Z
        if (transform.position.z >= _boardPointUp.position.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, _boardPointUp.position.z);
        }
        if(transform.position.z <= _boardPointDown.position.z)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, _boardPointDown.position.z);
        }

        //X
        if (transform.position.x >= _boardPointUp.position.x)
        {
            transform.position = new Vector3(_boardPointUp.position.x, transform.position.y, transform.position.z);
        }
        if (transform.position.x <= _boardPointDown.position.x)
        {
            transform.position = new Vector3(_boardPointDown.position.x, transform.position.y, transform.position.z);
        }


    }
}
