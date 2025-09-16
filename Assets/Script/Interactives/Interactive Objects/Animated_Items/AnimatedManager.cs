using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimatedManager : MonoBehaviour
{
    public static AnimatedManager Instance { get; private set; }

    private readonly HashSet<AnimatedInteractable> _animating = new HashSet<AnimatedInteractable>();
    private readonly Dictionary<AnimatedInteractable, Coroutine> _running = new Dictionary<AnimatedInteractable, Coroutine>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsAnimating(AnimatedInteractable obj) => _animating.Contains(obj);

    public void HandleInteraction(AnimatedInteractable obj)
    {
        if (obj == null) return;
        if (_animating.Contains(obj)) return; 

        switch (obj.mode)
        {
            case AnimatedMode.Toggle:
                StartTracked(obj, AnimateRelativeRoutine(obj, obj.isActive ? -1f : +1f));
                obj.isActive = !obj.isActive;
                break;

            case AnimatedMode.OneShot:
                if (!obj.isActive)
                {
                    StartTracked(obj, AnimateRelativeRoutine(obj, +1f));
                    obj.isActive = true;
                }
                break;

            case AnimatedMode.Incremental:
                obj.stepCount++;
                StartTracked(obj, AnimateRelativeRoutine(obj, +1f));
                break;
        }
    }

    private Coroutine StartTracked(AnimatedInteractable obj, IEnumerator routine)
    {
        var c = StartCoroutine(routine);
        _running[obj] = c;
        return c;
    }

    private IEnumerator AnimateRelativeRoutine(AnimatedInteractable obj, float direction)
    {
        _animating.Add(obj);

        Vector3 startPos = obj.transform.localPosition;
        Quaternion startRot = obj.transform.localRotation;

        Vector3 targetPos = startPos;
        Quaternion targetRot = startRot;

        if (obj.type == AnimatedType.Rotation)
            targetRot *= Quaternion.Euler(obj.axis * obj.amount * direction);
        else
            targetPos += obj.axis.normalized * obj.amount * direction;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * obj.speed;

            if (obj.type == AnimatedType.Rotation)
                obj.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            else
                obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        _animating.Remove(obj);
        _running.Remove(obj);
    }

 
    public void CancelAndSnapToInitial(AnimatedInteractable obj, bool markClosed = true)
    {
        if (obj == null) return;

        if (_running.TryGetValue(obj, out var c))
        {
            StopCoroutine(c);
            _running.Remove(obj);
        }
        _animating.Remove(obj);

        obj.transform.localPosition = obj.initialLocalPos;
        obj.transform.localRotation = obj.initialLocalRot;

        if (markClosed)
        {
            obj.isActive = false;
            obj.stepCount = 0;
        }
    }


    public Coroutine ResetAnimatedToInitial(AnimatedInteractable obj, bool markClosed = true)
    {
        if (obj == null) return null;

        if (_running.TryGetValue(obj, out var c))
        {
            StopCoroutine(c);
            _running.Remove(obj);
        }
        _animating.Remove(obj);

        return StartTracked(obj, ForceToInitialRoutine(obj, markClosed));
    }

    private IEnumerator ForceToInitialRoutine(AnimatedInteractable obj, bool markClosed)
    {
        _animating.Add(obj);

        Vector3 startPos = obj.transform.localPosition;
        Quaternion startRot = obj.transform.localRotation;

        Vector3 targetPos = obj.initialLocalPos;
        Quaternion targetRot = obj.initialLocalRot;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * obj.speed;

            if (obj.type == AnimatedType.Rotation)
                obj.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            else
                obj.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        if (markClosed)
        {
            obj.isActive = false;
            obj.stepCount = 0;
        }

        _animating.Remove(obj);
        _running.Remove(obj);
    }
}
