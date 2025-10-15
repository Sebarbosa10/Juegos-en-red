using Photon.Pun;
using UnityEngine;

public class PongManager : MonoBehaviourPunCallbacks
{
    [Header("Prefabs")]
    [SerializeField] private GameObject _paddlePrefab;
    [SerializeField] private GameObject _ballPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform _leftSpawn;
    [SerializeField] private Transform _rightSpawn;
    [SerializeField] private Transform _ballSpawn;

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
            SpawnPaddle();
    }

    private void SpawnPaddle()
    {
        Transform spawnPoint = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? _leftSpawn : _rightSpawn;

        GameObject paddle = PhotonNetwork.Instantiate(_paddlePrefab.name, spawnPoint.position, spawnPoint.rotation);

        Rigidbody rb = paddle.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Instantiate(_ballPrefab.name, _ballSpawn.position, _ballSpawn.rotation);
    }
}
