//class to stop player movement when they focus on a text or integer field
using UnityEngine;
using UnityEngine.UIElements;

public class FieldMovementStopped: MonoBehaviour
{
    [SerializeField] private VrStateData vrStateData;
    [SerializeField] private PlayerManager playerManager;
    void Update()
    {
        if(GetComponent<UIDocument>() == null) return;
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        if(root == null) return;
        bool uiHasFocus = root.panel.focusController.focusedElement is TextField || root.panel.focusController.focusedElement is IntegerField;

        if (uiHasFocus)
        {
            FocusOnText();
        }
        else
        {
            UnfocusOnText();
        }
    }

    private void FocusOnText()
    {
        if (vrStateData.IsInVr)
        {
            TouchScreenKeyboard.Open("");
        } else
        {
            playerManager.JustStopPlayer(false);
        }
    }
    private void UnfocusOnText()
    {
        if(vrStateData.IsInVr){}else
        {
            playerManager.JustStopPlayer(true);
        }
    }
}