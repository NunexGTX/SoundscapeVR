using UnityEngine.UIElements;

public class ButtonHandler
{
    private Button button;
    private System.Action _currentAction;

    public ButtonHandler(Button btn)
    {
        button = btn;
    }
    public void SetAction(System.Action newAction)
    {
        if (_currentAction != null)
            button.clicked -= _currentAction;

        _currentAction = newAction;
        button.clicked += _currentAction;
    }

    public void Clear()
    {
        if (_currentAction != null)
            button.clicked -= _currentAction;
        _currentAction = null;
    }
    
}
