using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public bool _spawningBrigde;
    public GameObject bridgePiecePrefab;

    private BridgeSpawner _bridgeSpawner;

    public static PlayerController Current;

    public float limitX;

    public float runningSpeed;
    public float xSpeed;
    private float _currentRunningSpeed;

    public GameObject ridingCylinderPrefab;

    public List<RidingCylinder> cylinders;

    private float _creatingBridgeTimer;
 
    // Start is called before the first frame update
    void Start()
    {
        Current = this;
        
    }

    // Update is called once per frame
    void Update()
    {

        if (LevelController.Current==null || !LevelController.Current.gameActive)
        {
            return;
        }
        float newX = 0;
        float touchXDelta = 0;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase==TouchPhase.Moved)
        {
            touchXDelta = Input.GetTouch(0).deltaPosition.x / Screen.width;
        }

        else if (Input.GetMouseButton(0))
        {
            touchXDelta = Input.GetAxis("Mouse X"); 
        }
        newX = transform.position.x + xSpeed * touchXDelta * Time.deltaTime;
        newX = Mathf.Clamp(newX, -limitX, limitX);

        Vector3 newPosition = new Vector3(newX,transform.position.y,transform.position.z+_currentRunningSpeed*Time.deltaTime);
        transform.position = newPosition;

        if (_spawningBrigde)
        {
            _creatingBridgeTimer -= Time.deltaTime;
            if (_creatingBridgeTimer < 0)
            {
                _creatingBridgeTimer = 0.1f;
                IncrementCylinderVolume(-0.1f);
                GameObject createdBridgePiece = Instantiate(bridgePiecePrefab);
                Vector3 direction = _bridgeSpawner.endReference.transform.position - _bridgeSpawner.startReference.transform.position;
                createdBridgePiece.transform.forward = direction;
                float distance = direction.magnitude;
                direction = direction.normalized;
                float characterDistance = transform.position.z - _bridgeSpawner.startReference.transform.position.z;
                characterDistance = Mathf.Clamp(characterDistance, 0, distance);
                Vector3 newPiecePosition = _bridgeSpawner.startReference.transform.position + direction*characterDistance;
                newPiecePosition.x = transform.position.x;
                createdBridgePiece.transform.position = newPiecePosition;
            }
        }
    }
    
    public void ChangeSpeed(float value)
    {
        _currentRunningSpeed = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AddCylnder"))
        {
            IncrementCylinderVolume(0.1f);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("SpawnBrigde"))
        {
            StartSpawnBridge(other.transform.parent.GetComponent<BridgeSpawner>());
        }
        else if (other.CompareTag("StopSpawnBridge"))
        {
            StopSpawnBridge();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Trap"))
        {
            IncrementCylinderVolume(-Time.fixedDeltaTime);
        }
    }

    public void IncrementCylinderVolume(float value)
    {
        if (cylinders.Count == 0)
        {
            if (value > 0)
            {
                CreateCylinder(value);
            }
            else
            {

            }
        }
        else
        {
            cylinders[cylinders.Count - 1].IncrementCylinderVolume(value);
        }
    }

    public void CreateCylinder(float value)
    {
        RidingCylinder createdCylinder = Instantiate(ridingCylinderPrefab, transform).GetComponent<RidingCylinder>();
        cylinders.Add(createdCylinder);
        createdCylinder.IncrementCylinderVolume(value);
    }

    public void DestroyCylinder(RidingCylinder cylinder)
    {
        cylinders.Remove(cylinder);
        Destroy(cylinder.gameObject);
    }

    public void StartSpawnBridge(BridgeSpawner spawner)
    {
        _bridgeSpawner = spawner;
        _spawningBrigde = true;
    }

    public void StopSpawnBridge()
    {
        _spawningBrigde = false;
    }
}
