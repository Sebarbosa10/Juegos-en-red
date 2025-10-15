using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BallController : MonoBehaviour
{
    [SerializeField] private float _initialSpeed = 5f;
    [SerializeField] private float _speedIncrease = 1.1f;
    [SerializeField] private float _maxSpeed = 15f;

    private Rigidbody _rb;
    private Transform _spawnPoint;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        GameObject spawnObj = GameObject.Find("BallSpawn");
        if (spawnObj != null)
            _spawnPoint = spawnObj.transform;
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            LaunchBall();
        }
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
    }

    private void LaunchBall()
    {
        Vector3 dir = new Vector3(Random.Range(0, 2) == 0 ? -1 : 1, Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f)).normalized;
        _rb.velocity = dir * _initialSpeed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector3 newVel = _rb.velocity * (1f + _speedIncrease * Time.fixedDeltaTime);
        _rb.velocity = Vector3.ClampMagnitude(newVel, _maxSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.CompareTag("GoalLeft") || other.CompareTag("GoalRight"))
        {
            ResetBall();
        }
    }

    private void ResetBall()
    {

        if (_spawnPoint == null)
        {
            _rb.position = Vector3.zero;
        }
        else
        {
            _rb.position = _spawnPoint.position;
        }

            _rb.velocity = Vector3.zero;
        Invoke(nameof(LaunchBall), 1.5f);
    }
}
