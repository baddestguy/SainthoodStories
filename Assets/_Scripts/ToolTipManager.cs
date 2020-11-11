using UnityEngine;

public class ToolTipManager : MonoBehaviour
{
    public static ToolTipManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ShowToolTip(string locKey, TooltipStats stats = null)
    {
        var locText = LocalizationManager.Instance.GetText(locKey);
        if(stats != null)
        {
            locText += $"\n";
            if(stats.Ticks > 0)
            {
                locText += $"+{stats.Ticks / 2} hr(s) <color=\"white\">|";
            }
            if (stats.FP != 0)
            {
                locText += stats.FP > 0 ? $" <color=\"green\">+{stats.FP} FP <color=\"white\">|" : $" <color=\"red\">{stats.FP} FP <color=\"white\">|";
            }
            if (stats.CP != 0)
            {
                locText += stats.CP > 0 ? $" <color=\"green\">+{stats.CP} CP <color=\"white\">|" : $" <color=\"red\">{stats.CP} CP <color=\"white\">|";
            }
            if (stats.Energy != 0)
            {
                locText += stats.Energy > 0 ? $" <color=\"green\">+{stats.Energy} EN" : $" <color=\"red\">{stats.Energy} EN";
            }
        }

        UI.Instance.DisplayToolTip(locText);
    }
}
