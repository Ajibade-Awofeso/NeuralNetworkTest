using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour, IEntity
{
    [SerializeField] private float _horizontal;
    [SerializeField] private float _vertical;
    [SerializeField] Vector2 movementVector;

    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private bool _hasMutated = false;
    [SerializeField] public bool _isWinning = false;

    [SerializeField] private GameObject playerPrefab;

    public float _range;

    [SerializeField] public float fitness;

    [SerializeField] private float[] _coinDistances = new float[64];
    [SerializeField] private float[] _enemyDistance = new float[64];

    Rigidbody _rigidbody;
    NeuralNetwork _nn;

    LevelManager level;

    public float mutationAmount = 0.8f;
    public float mutationChance = 0.2f;

    Vector3 _startPosition;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _nn = GetComponent<NeuralNetwork>();
        level = FindObjectOfType<LevelManager>();

        _startPosition = transform.position;

        _hasMutated = false;
        fitness = 0;
        _isWinning = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        if (!_hasMutated)
        {
            MutatePlayer();
            _hasMutated = true;
        }

        GetCoinLocation();
        NeuralNetwork();
        Movement();
    }

    void NeuralNetwork()
    {
        float[] inputs = _coinDistances;
        float[] outputs = _nn.Brain(inputs);

        _horizontal = outputs[0];
        _vertical = outputs[1];
    }

    void Movement()
    {
        _horizontal = Mathf.Clamp(_horizontal, -1, 1);
        _vertical = Mathf.Clamp(_vertical, -1, 1);

        movementVector = new Vector2(_horizontal, _vertical);
        movementVector.Normalize();

        _rigidbody.velocity = Vector3.MoveTowards(_rigidbody.velocity, new Vector3(movementVector.x * _speed, _rigidbody.velocity.y, movementVector.y * _speed), _acceleration * Time.deltaTime);
    }

    void GetCoinLocation()
    {
        RaycastHit hit;

        for (int i = 0; i < _coinDistances.Length; i++)
        {
            float angle = ((2 * i + 1 - _coinDistances.Length) * 5.625f / 2);

            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 rayDirection = rotation * transform.forward * -1;

            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayStart, rayDirection, out hit, _range))
            {
                Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.red);
                if (hit.transform.gameObject.tag == "Coin")
                {
                    // Use the length of the raycast as the distance to the food object
                    _coinDistances[i] = hit.distance / _range;
                }
                else
                {
                    // If no food object is detected, set the distance to the maximum length of the raycast
                    _coinDistances[i] = 1;
                }
            }
            else
            {
                // Draw a line representing the raycast in the scene view for debugging purposes
                Debug.DrawRay(rayStart, rayDirection * _range, Color.red);
                _coinDistances[i] = 1;
            }
        }
    }

    void MutatePlayer()
    {
        mutationAmount += Random.Range(-1.0f, 1.0f) / 100;
        mutationChance += Random.Range(-1.0f, 1.0f) / 100;

        //make sure mutation amount and chance are positive using max function
        mutationAmount = Mathf.Max(mutationAmount, 0);
        mutationChance = Mathf.Max(mutationChance, 0);

        _nn.MutateNetwork(mutationAmount, mutationChance);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            other.gameObject.SetActive(false);
            fitness += 1;
            level.GetTopPlayer();
        }
    }

    public void Reset()
    {
   
        if (_isWinning)
        {
            for (int i = 0; i < 5; i++)
            {
                //create a new agent, and set its position to the parent's position + a random offset in the x and z directions (so they don't all spawn on top of each other)
                GameObject child = Instantiate(playerPrefab, new Vector3(
                    (float)_startPosition.x + Random.Range(-10, 11),
                    0.75f,
                    (float)_startPosition.z + Random.Range(-10, 11)),
                    Quaternion.identity);

                //copy the parent's neural network to the child
                child.GetComponent<NeuralNetwork>().layers = GetComponent<NeuralNetwork>().copyLayers();
            }
            GameObject.Destroy(this.gameObject);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
