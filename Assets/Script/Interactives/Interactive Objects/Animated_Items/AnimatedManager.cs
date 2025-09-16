using UnityEngine;
using System.Collections;

public class AnimatedManager : MonoBehaviour
{
    public static AnimatedManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void HandleInteraction(AnimatedInteractable obj)
    {
        // Check mode and decide what to do
        switch (obj.mode)
        {
            case AnimatedMode.Toggle:
                StartCoroutine(AnimateToggle(obj));
                break;
            case AnimatedMode.OneShot:
                if (!obj.isActive) // Only once
                    StartCoroutine(Animate(obj));
                break;
            case AnimatedMode.Incremental:
                StartCoroutine(AnimateIncremental(obj));
                break;
        }
    }

    private IEnumerator AnimateToggle(AnimatedInteractable obj)
    {
        obj.isActive = !obj.isActive;
        float direction = obj.isActive ? 1f : -1f;
        yield return Animate(obj, direction);
    }

    private IEnumerator AnimateIncremental(AnimatedInteractable obj)
    {
        obj.stepCount++;
        yield return Animate(obj, 1f);
    }

    private IEnumerator Animate(AnimatedInteractable obj, float direction = 1f)
    {
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;

        Vector3 targetPos = startPos;
        Quaternion targetRot = startRot;

        if (obj.type == AnimatedType.Rotation)
            targetRot *= Quaternion.Euler(obj.axis * obj.amount * direction);
        else if (obj.type == AnimatedType.Translation)
            targetPos += obj.axis.normalized * obj.amount * direction;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * obj.speed;
            if (obj.type == AnimatedType.Rotation)
                obj.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            else
                obj.transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }
    }
}
