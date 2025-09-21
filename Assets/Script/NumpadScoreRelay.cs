using UnityEngine;

public class NumpadScoreRelay : MonoBehaviour
{
    [SerializeField] private TeamNumpadController controller; // keypad de ESTE equipo

    public void OnPadCorrect()
    {
        if (!controller) return;
        // Usa el teamFilter del keypad para saber quién hizo punto
        MatchManager.Instance?.TeamScored(controller ? controller.name.Contains("Red") ? "Red" : controller.name.Contains("Blue") ? "Blue" : controllerTeam() : controllerTeam());
    }

    // Más robusto: preguntarle directo al controller su filtro
    private string controllerTeam()
    {
        // Si tu TeamNumpadController expone un getter público al teamFilter, mejor.
        // Si no, hack: serialize el equipo en este Relay:
        return (controllerTeamOverride.Length > 0) ? controllerTeamOverride : "Blue";
    }

    [Header("Si no querés inferir por nombre, setealo acá")]
    [SerializeField] private string controllerTeamOverride = ""; // "Blue" o "Red"
}
