using UnityEngine;

public class WaterPromptUI : MonoBehaviour
{
    public static WaterPromptUI Instance;

    [Header("UI Panel")]
    public GameObject waterPromptPanel;

    private SoilPlot currentPlot;

    private void Awake()
    {
        Instance = this;

        if (waterPromptPanel != null)
            waterPromptPanel.SetActive(false);
    }

    public void Show(SoilPlot plot, Vector3 position)
    {
        currentPlot = plot;
        transform.position = position;

        if (waterPromptPanel != null)
            waterPromptPanel.SetActive(true);
    }

    public void OnYes()
    {
        if (currentPlot != null)
            currentPlot.WaterPlant();

        if (waterPromptPanel != null)
            waterPromptPanel.SetActive(false);
    }

    public void OnNo()
    {
        if (waterPromptPanel != null)
            waterPromptPanel.SetActive(false);
    }
}
