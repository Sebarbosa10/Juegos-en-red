using UnityEngine;
using UnityEngine.Events;

public class StatuesManager : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    public UnityEvent onSolved;
    [System.Serializable]
    public class StatueRequirement
    {
        
        public Transform statue;

      
        public Axis axis = Axis.Y;

       
        public bool useLocalRotation = true;

        
        public float targetAngle = 0f;

        public float offsetDegrees = 0f;

        
        public float tolerance = 2f;

        [HideInInspector] public bool wasCorrect = false;  
        [HideInInspector] public bool isCorrect = false;   
        [HideInInspector] public float lastDelta = 0f;     
        [HideInInspector] public float lastCurrent = 0f;   
    }

    [SerializeField] private StatueRequirement[] statues = new StatueRequirement[4];

    
    [SerializeField] private bool autoCheckEveryFrame = true;

   
    [SerializeField] private bool logOnlyOnce = true;

    
    [SerializeField] private int solvedStickyFrames = 1;

    private bool solved = false;
    private int consecutiveAllCorrectFrames = 0;

    private void Update()
    {
        if (autoCheckEveryFrame) CheckSolved();
    }

    
    public void CheckSolved()
    {
        if (statues == null || statues.Length == 0)
        {
            Debug.LogWarning("[Statues] No hay estatuas configuradas.");
            return;
        }

        bool allCorrectThisFrame = true;

      
        for (int i = 0; i < statues.Length; i++)
        {
            var s = statues[i];
            if (s == null || s.statue == null)
            {
                Debug.LogWarning($"[Statues] Falta asignar la estatua en el slot {i}.");
                return;
            }

            float current = GetAxisAngle(s.statue, s.axis, s.useLocalRotation);
            float target = s.targetAngle + s.offsetDegrees;
            float delta = Mathf.Abs(Mathf.DeltaAngle(current, target));
            bool isCorrect = (delta <= Mathf.Abs(s.tolerance));

            s.lastCurrent = current;
            s.lastDelta = delta;
            s.isCorrect = isCorrect;

            if (isCorrect && !s.wasCorrect)
            {
                Debug.Log($"[Statues] {s.statue.name} en ángulo correcto " +
                          $"(objetivo {target:0.0}, actual {current:0.0}, ={delta:0.00})");
            }

            s.wasCorrect = isCorrect;

            if (!isCorrect) allCorrectThisFrame = false;
        }

       
        if (allCorrectThisFrame)
        {
            consecutiveAllCorrectFrames++;
            if (consecutiveAllCorrectFrames >= Mathf.Max(1, solvedStickyFrames))
            {
                if (!solved || !logOnlyOnce) Debug.Log("Puzzle completo");
                solved = true;
                onSolved?.Invoke();
            }
        }
        else
        {
            consecutiveAllCorrectFrames = 0;
            if (!logOnlyOnce) solved = false; 
        }
    }

    public bool IsSolved => solved;

   
    public void ResetSolvedFlag()
    {
        solved = false;
        consecutiveAllCorrectFrames = 0;
    }

   
    public void PrintStatus()
    {
        int ok = 0;
        for (int i = 0; i < statues.Length; i++)
        {
            var s = statues[i];
            if (s?.statue == null)
            {
                Debug.LogWarning($"[Statues] Slot {i} vacío.");
                continue;
            }
            float target = s.targetAngle + s.offsetDegrees;
            Debug.Log($"[Statues] [{i}] {s.statue.name} ({s.axis}) " +
                      $"actual={s.lastCurrent:0.0}  objetivo={target:0.0}  ={s.lastDelta:0.00}   {(s.isCorrect ? "OK" : "NO")}");
            if (s.isCorrect) ok++;
        }
        Debug.Log($"[Statues] Correctas: {ok}/{statues.Length} | solved={solved} | stick={consecutiveAllCorrectFrames}/{Mathf.Max(1, solvedStickyFrames)}");
    }

    private static float GetAxisAngle(Transform t, Axis axis, bool useLocal)
    {
        Vector3 e = useLocal ? t.localEulerAngles : t.eulerAngles;
        return axis == Axis.X ? e.x : axis == Axis.Y ? e.y : e.z;
    }
}

