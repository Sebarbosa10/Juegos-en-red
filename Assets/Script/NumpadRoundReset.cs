using UnityEngine;

public class NumpadRoundReset : MonoBehaviour
{
    [SerializeField] private TeamNumpadController controller;

    public void ResetForNewRound()
    {
        if (!controller) controller = GetComponent<TeamNumpadController>();
        if (!controller) return;

        // Limpia y desbloquea
        controller.SetCorrectCode(controllerCode(controller)); // reutiliza su mismo código
        // Si dejaste lockAfterSolve en true, SetCorrectCode ya deja _solved=false
    }

    // Obtiene el mismo código actual (si lo exponés). Si no, pasále el mismo string manualmente:
    [SerializeField] private string correctCodeMirror = ""; // deja este igual que el del controller, o expón un getter en el controller
    private string controllerCode(TeamNumpadController _) => string.IsNullOrEmpty(correctCodeMirror) ? "1234" : correctCodeMirror;
}

