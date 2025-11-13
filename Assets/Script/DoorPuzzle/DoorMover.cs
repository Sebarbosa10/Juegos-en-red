using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMover : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveDistance = 3f; 
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _open = false;
    private Vector3 _startPos;

    void Awake() => _startPos = transform.position;

    public bool IsOpen => _open;

    public void Open()
    {
        if (_open) return;
        _open = true;
        StopAllCoroutines();
        StartCoroutine(DoMove(_startPos, _startPos + Vector3.down * moveDistance, duration));
    }

    public void ResetClosed()
    {
        _open = false;
        StopAllCoroutines();
        transform.position = _startPos;
    }

    private System.Collections.IEnumerator DoMove(Vector3 from, Vector3 to, float t)
    {
        float el = 0f;
        while (el < t)
        {
            el += Time.deltaTime;
            float k = Mathf.Clamp01(el / t);
            transform.position = Vector3.LerpUnclamped(from, to, curve.Evaluate(k));
            yield return null;
        }
        transform.position = to;
    }
}
