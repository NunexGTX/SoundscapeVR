using UnityEngine.UIElements;

public class ButtonHandler
{
    private Button button;
    private string id;
    private System.Action _currentAction;

    public ButtonHandler(Button btn, string id = "")
    {
        button = btn;
        this.id = id;
    }

    public void SetId(string id)
    {
        this.id = id;
        button.text = id;
    }

    public string GetId(){return id;}
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
