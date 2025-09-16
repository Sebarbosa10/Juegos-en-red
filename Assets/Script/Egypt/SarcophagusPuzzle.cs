using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SarcophagusPuzzle : MonoBehaviour
{
    [Header("Config")]
    public SarcophagusLid[] lids;  
    public int[] correctOrder;          
    [Tooltip("QUE ES ESTO DIOS, ESTOY CHANCHEANDO")]
    public bool animatedReset = true;

    // Estado
    private readonly List<int> attemptOrder = new List<int>();
    private readonly HashSet<int> reserved = new HashSet<int>();
    private bool solved = false;
    private bool resetting = false;

    public bool IsBusy => resetting || solved;

    private void OnValidate()
    {
        if (lids != null && correctOrder != null)
        {
            foreach (var i in correctOrder)
            {
                if (i < 0 || i >= lids.Length)
                    Debug.LogWarning($"Índice {i} fuera de rango en 'correctOrder'", this);
            }
        }
    }


    public bool TryReserveIndex(int index)
    {
        if (IsBusy) return false;
        if (reserved.Contains(index)) return false;                         
        if (attemptOrder.Count >= correctOrder.Length) return false;        
        reserved.Add(index);
        return true;
    }

    public void OnLidClicked(int index)
    {
        if (IsBusy) return;

        attemptOrder.Add(index);
        if (attemptOrder.Count >= correctOrder.Length)
        {
            StartCoroutine(EvaluateAfterAnimations());
        }
    }

    private IEnumerator EvaluateAfterAnimations()
    {

        bool anyAnimating;
        do
        {
            anyAnimating = false;
            foreach (var lid in lids)
            {
                if (lid != null && lid.animatedLid != null &&
                    AnimatedManager.Instance.IsAnimating(lid.animatedLid))
                {
                    anyAnimating = true;
                    break;
                }
            }
            yield return null;
        } while (anyAnimating);

        bool ok = attemptOrder.Count == correctOrder.Length;
        if (ok)
        {
            for (int i = 0; i < correctOrder.Length; i++)
            {
                if (attemptOrder[i] != correctOrder[i]) { ok = false; break; }
            }
        }

        if (ok)
        {
            solved = true;
            Debug.Log("VAMOOOOOOOO");
     
        }
        else
        {
            Debug.Log("MALISIMO TODO. TU LOGICA Y LA MATERIA");
            StartCoroutine(ResetAttemptRoutine());
        }
    }

    private IEnumerator ResetAttemptRoutine()
    {
        resetting = true;


        foreach (var lid in lids)
        {
            if (lid == null) continue;
            lid.Unlock();
            if (animatedReset) lid.AnimateBackToInitial();
            else lid.SnapToInitial();
        }


        if (animatedReset)
        {
            bool anyAnimating;
            do
            {
                anyAnimating = false;
                foreach (var lid in lids)
                {
                    if (lid != null && lid.animatedLid != null &&
                        AnimatedManager.Instance.IsAnimating(lid.animatedLid))
                    {
                        anyAnimating = true;
                        break;
                    }
                }
                yield return null;
            } while (anyAnimating);
        }

        attemptOrder.Clear();
        reserved.Clear();

        resetting = false;
        Debug.Log("NOOOOOOOOOOO");
    }
}
