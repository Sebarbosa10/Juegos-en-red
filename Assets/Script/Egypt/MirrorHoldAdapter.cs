using UnityEngine;
using Photon.Pun;


[DisallowMultipleComponent]
public class MirrorHoldAdapter : MonoBehaviour, IInteractive
{
    [Header("Pivots del rig")]
    public Transform heightPivot;  
    public Transform yawPivot;     

    [Header("Rangos")]
    public Vector2 yawLimits = new Vector2(-90f, 90f);     
    public Vector2 heightLimits = new Vector2(0f, 0.6f);   

    [Header("Sensibilidad (px→unidad)")]
    public float yawSensitivity = 0.25f;       
    public float heightSensitivity = 0.01f;    

    [Header("Comportamiento")]
    public bool returnToOriginOnRelease = false;
    public float returnDuration = 0.25f;

    [Header("Animated Cosas")]
    public AnimatedInteractable animated;   
    public AnimatedNetSync netSync;
    [SerializeField] PhotonView photonViewOverride;
    private PhotonView _pv;
    private bool _holding = false;
    private Vector3 _startMouse;
    private float _startYaw;      
    private float _startHeight;   

    private Vector3 _initLocalPosHeight;
    private Quaternion _initLocalRotYaw;



    void Awake()
    {
        _pv = photonViewOverride
           ?? GetComponentInParent<PhotonView>()       
           ?? transform.root.GetComponent<PhotonView>();

        if (!animated) animated = GetComponent<AnimatedInteractable>();
        if (!netSync) netSync = GetComponent<AnimatedNetSync>();

        if (!heightPivot) heightPivot = transform;
        if (!yawPivot) yawPivot = transform;

        _initLocalPosHeight = heightPivot.localPosition;
        _initLocalRotYaw = yawPivot.localRotation;
    }
    public void Interact()
    {
        BeginHold(Input.mousePosition);
    }

    void Update()
    {
        if (_holding)
        {
            if (Input.GetMouseButton(0))
            {
                ContinueHold(Input.mousePosition);
            }
            else
            {
                EndHold();
            }
        }
    }
    private void BeginHold(Vector3 mouseScreenPos)
    {
        if (!_pv.IsMine) _pv.RequestOwnership();

        _holding = true;
        _startMouse = mouseScreenPos;

        _startYaw = GetYawDeg(yawPivot.localRotation);
        _startHeight = heightPivot.localPosition.y;
    }

    private void ContinueHold(Vector3 mouseScreenPos)
    {
        if (!_pv.IsMine) return; 

        Vector3 delta = mouseScreenPos - _startMouse;

        float yawDelta = delta.x * yawSensitivity;
        float targetYaw = Mathf.Clamp(_startYaw + yawDelta, yawLimits.x, yawLimits.y);
        SetYaw(targetYaw);

        float hDelta = -delta.y * heightSensitivity;
        float targetH = Mathf.Clamp(_startHeight + hDelta, heightLimits.x, heightLimits.y);
        SetHeight(targetH);
    }

    private void EndHold()
    {
        _holding = false;

        if (returnToOriginOnRelease)
        {

            if (animated != null)
            {
                AnimatedManager.Instance.AnimateAbsolute(
                    animated,
                    heightPivot.localPosition, yawPivot.localRotation,
                    new Vector3(heightPivot.localPosition.x, _initLocalPosHeight.y, heightPivot.localPosition.z),
                    _initLocalRotYaw,
                    returnDuration,
                    markActive: false
                );
            }
            else
            {
                StartCoroutine(ReturnRoutine());
            }
        }
    }

    System.Collections.IEnumerator ReturnRoutine()
    {
        float t = 0, dur = Mathf.Max(0.0001f, returnDuration);
        Vector3 startPos = heightPivot.localPosition;
        Quaternion startRot = yawPivot.localRotation;

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float h = Mathf.Lerp(startPos.y, _initLocalPosHeight.y, t);
            heightPivot.localPosition = new Vector3(startPos.x, h, startPos.z);
            yawPivot.localRotation = Quaternion.Slerp(startRot, _initLocalRotYaw, t);
            yield return null;
        }
    }
    static float GetYawDeg(Quaternion qLocal)
    {
        Vector3 f = qLocal * Vector3.forward;
        float ang = Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg; 
        return ang;
    }

    void SetYaw(float yawDeg) => yawPivot.localRotation = Quaternion.Euler(0f, yawDeg, 0f);
    void SetHeight(float h)
    {
        var p = heightPivot.localPosition;
        p.y = h;
        heightPivot.localPosition = p;
    }
}
