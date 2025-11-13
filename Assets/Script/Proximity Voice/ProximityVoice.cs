using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;

[RequireComponent(typeof(Speaker))]
[RequireComponent(typeof(AudioSource))]
public class ProximityVoice : MonoBehaviour
{
    [System.Serializable]
    public class HearingDistanceSettings
    {
        [Header("Distances (meters)")]
        public float fullVolumeDistance = 10f;   
        public float mediumVolumeDistance = 20f; 
        public float maxHearingDistance = 50f;   
    }

    public HearingDistanceSettings distanceSettings = new HearingDistanceSettings();

    private Speaker _speaker;
    private AudioSource _audioSource;
    private Transform _listener;

    private void Awake()
    {
        _speaker = GetComponent<Speaker>();
        _audioSource = GetComponent<AudioSource>();

    }

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer != null &&
            PhotonNetwork.LocalPlayer.TagObject is GameObject localPlayerObj)
        {
            _listener = localPlayerObj.transform;
        }
        else if (Camera.main != null)
        {
            _listener = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (_listener == null || _audioSource == null)
            return;

        float distance = Vector3.Distance(_listener.position, transform.position);
        _audioSource.volume = CalculateVolume(distance);
    }

    private float CalculateVolume(float distance)
    {
        float full = distanceSettings.fullVolumeDistance;
        float medium = distanceSettings.mediumVolumeDistance;
        float max = distanceSettings.maxHearingDistance;

        if (distance >= max)
            return 0f;

        if (distance <= full)
            return 1f;

        if (distance <= medium)
            return 0.7f; 

        // esto hace un fade lineal
        float t = (distance - medium) / (max - medium);
        return Mathf.Lerp(0.7f, 0f, t);
    }

    public void SetListener(Transform listenerTransform)
    {
        _listener = listenerTransform;
    }
}