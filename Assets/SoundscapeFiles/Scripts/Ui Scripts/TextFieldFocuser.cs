using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

public class TextFieldFocuser : MonoBehaviour
{
    [SerializeField] private VrStateData vrStateData;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private GlobalNonNativeKeyboard keyboardManager;
    private List<Action> unbindingActions = new();
    private VisualElement _activeField;

    void OnEnable()
    {
        RegisterAllTextFields();
    }

    void OnDisable()
    {UnregisterAllCallbacks();}
    private void RegisterAllTextFields()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        var allFields = root.Query<VisualElement>()
            .Where(e => e is TextField || e is IntegerField || e is FloatField)
            .ToList();
        foreach (var field in allFields)
        {
            EventCallback<FocusInEvent> focusIn = _ => OnTextFieldFocused(field);
            EventCallback<FocusOutEvent> focusOut = _ => OnTextFieldUnfocused(field);

            field.RegisterCallback(focusIn);
            field.RegisterCallback(focusOut);

            unbindingActions.Add(() => field.UnregisterCallback(focusIn));
            unbindingActions.Add(() => field.UnregisterCallback(focusOut));
        }
    }

    private void UnregisterAllCallbacks()
    {
        foreach(Action act in unbindingActions)
            act.Invoke();
        unbindingActions.Clear();
    }


    private void OnTextFieldFocused(VisualElement field)
    {
        if (!vrStateData.IsInVr)
        {
            playerManager.JustStopPlayer(false);
            return;
        }
        _activeField = field;
        if(field is TextField textField)
        {
            keyboardManager.ShowKeyboard(textField.text);
        } else if(field is IntegerField integerField)
        {
            keyboardManager.ShowKeyboard(integerField.value.ToString());
        } else if(field is FloatField floatField)
        {
            Debug.Log($"[SoundscapeVR] focusing on float field with value {floatField.value}");
            keyboardManager.ShowKeyboard(floatField.value.ToString());
        }
        if(!keyboardManager.keyboard.isOpen) return;
        keyboardManager.keyboard.onTextUpdated.AddListener(OnKeyboardTextUpdate);
        keyboardManager.keyboard.onClosed.AddListener(OnKeyboardClosed);

    }
    private void OnTextFieldUnfocused(VisualElement field)
    {
        if (!vrStateData.IsInVr)
        {
            playerManager.JustStopPlayer(true);
            return;
        }
        /*if (_activeField == field)
        {
            OnKeyboardClosed();
        }*/
    }

    private void OnKeyboardTextUpdate(KeyboardTextEventArgs args)
    {
        string keyboardText = args.keyboardText;

        if (_activeField is TextField tf)
        {
            tf.value = keyboardText;
        }
        else if (_activeField is IntegerField inf)
        {
            if (int.TryParse(keyboardText, out int result))
            {
                inf.value = result;
            }
        }
        else if (_activeField is FloatField flf)
        {
            if (float.TryParse(keyboardText, out float result))
            {
                flf.value = result;
            }
        }
    }
    private void OnKeyboardClosed(KeyboardTextEventArgs args)
    {
        OnKeyboardClosed();
    }
    private void OnKeyboardClosed()
    {
        keyboardManager.keyboard.onTextUpdated.RemoveListener(OnKeyboardTextUpdate);
        keyboardManager.keyboard.onTextUpdated.RemoveListener(OnKeyboardClosed);
        _activeField = null;
    }

    public void UpdateFields()
    {
        UnregisterAllCallbacks();
        RegisterAllTextFields();
    }
}