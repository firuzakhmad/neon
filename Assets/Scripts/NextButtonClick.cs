using UnityEngine;

public class ButtonOnClick : MonoBehaviour
{
    [Header("Interaction Settings")]
    public GameObject[] panels;  // Array of panels to cycle through
    private int currentPanelIndex = 0;  // Tracks the current panel index

    private void Start()
    {
        // Ensure that only the first panel is active at the start.
        UpdatePanelVisibility();

        // TODO: Disable all panel exept panel one at the start 
    }


    // This function handles the "Next" button click logic.
    public void NextButtonOnClick()
    {
        // Deactivate the current panel
        panels[currentPanelIndex].SetActive(false);

        // Increment the index to go to the next panel, wrapping around when necessary
        currentPanelIndex = (currentPanelIndex + 1) % panels.Length;

        // Activate the next panel
        UpdatePanelVisibility();
    }

    // This function handles the "Previous" button click logic.
    public void PreviousButtonOnClick()
    {
        // Deactivate the current panel
        panels[currentPanelIndex].SetActive(false);

        // Decrement the index to go to the previous panel, wrapping around when necessary
        currentPanelIndex = (currentPanelIndex - 1 + panels.Length) % panels.Length;

        // Activate the previous panel
        UpdatePanelVisibility();
    }

    // Helper function to update the active panel based on the current index
    private void UpdatePanelVisibility()
    {
        // Ensure we have a valid index
        if (panels.Length > 0 && currentPanelIndex >= 0 && currentPanelIndex < panels.Length)
        {
            // Activate the panel at the current index
            panels[currentPanelIndex].SetActive(true);
        }
    }
}