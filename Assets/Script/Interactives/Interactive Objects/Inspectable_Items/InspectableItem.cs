using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InspectableItem : MonoBehaviour, IInteractive
{
    private Vector3 originalPos;
    private Quaternion originalRot;
    private Transform originalParent;

    private bool isBeingInspected;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Interact()
    {
        if (!isBeingInspected)
            InspectionManager.Instance.StartInspection(this);
        else
            InspectionManager.Instance.EndInspection();
    }

    public void StartInspection(Transform anchor)
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        originalParent = transform.parent;

        if(_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        transform.SetParent(anchor);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        isBeingInspected = true;
    }

    public void EndInspection()
    {
        transform.SetParent(originalParent);
        transform.position = originalPos;
        transform.rotation = originalRot;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        isBeingInspected = false;
    }
}
