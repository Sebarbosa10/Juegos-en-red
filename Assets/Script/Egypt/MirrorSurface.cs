using UnityEngine;

[DisallowMultipleComponent]
public class MirrorSurface : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis localAxis = Axis.Z;
    public bool flip = false;

    public Vector3 GetWorldNormal()
    {
        Vector3 local = localAxis == Axis.X ? Vector3.right :
                        localAxis == Axis.Y ? Vector3.up :
                                              Vector3.forward;
        Vector3 n = transform.rotation * local;
        return (flip ? -n : n).normalized;
    }
}
