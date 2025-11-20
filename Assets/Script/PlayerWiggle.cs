using UnityEngine;

public class PlayerWiggle : MonoBehaviour
{
    public float wiggleSpeed = 6f;
    public float wiggleAmount = 0.1f;

    private Vector3 _initialLocalPos;
    private bool _isMoving;

    public bool IsMoving => _isMoving;

    private void Awake()
    {
        _initialLocalPos = transform.localPosition;
    }

    public void SetMoving(bool isMoving)
    {
        _isMoving = isMoving;
    }

    private void Update()
    {
        if (_isMoving)
        {
            float wiggle = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
            transform.localPosition = _initialLocalPos + new Vector3(wiggle, 0f, 0f);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                _initialLocalPos,
                Time.deltaTime * 8f);
        }
    }
}
