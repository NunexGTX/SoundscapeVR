using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class FormScript : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
    [SerializeField] private VisualTreeAsset radioButtonElement;
    [SerializeField] private VisualTreeAsset labelElement;
    [SerializeField] private VisualTreeAsset textInputElement;
    [SerializeField] private VisualTreeAsset scaleElement;
    [SerializeField] private VisualTreeAsset checkboxContainerElement;
    [SerializeField] private VisualTreeAsset checkboxElement;
    [SerializeField] private VisualTreeAsset dropdownElement;
    [SerializeField] private GoogleForm formSubmiter;
    public const string SoundscapeInit = "LoadSoundscape:";
    private Button returnBtn;
    private VisualElement formRoot;
    private List<List<FormElement>> formSections = new();
    private string formTitle = "";
    private string formDesc= "";
    private int formPosition = -1;
    private List<Action> unbindingActions = new();
    private FormAnswersHolder answersHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        formPosition = -1;
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        returnBtn = root.Q<Button>("returnBtn");
        returnBtn.clicked += Return;
        if(library.form == null) return;
        formRoot = root.Q<VisualElement>("formRoot");
        if(library.form == null || string.IsNullOrWhiteSpace(library.form.formTitle)) return;
        formSections = SeparateElements(library.form.questions);
        formTitle = library.form.formTitle;
        formDesc = library.form.description;
        Button nextBtn = root.Q<Button>("nextBtn");
        nextBtn.clicked += NextSection;
        Label title = root.Q<Label>("formTitle");
        title.text = library.form.formTitle;
        answersHolder = new(){pageCount = formSections.Count};
        unbindingActions = new();
    }

    void OnDisable()
    {
        returnBtn.clicked -= Return;
    }

    private void Return()
    {
        RemoveSection();
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    private void NextSection()
    {
        if(formSections.Count == formPosition) return;
        if(!answersHolder.AreRequiredComplete(formPosition)) return;
        if(formPosition >= formSections.Count - 1)
        {
            SubmitForm();
            return;
        }
        formPosition++;
        RemoveSection();
        BuildFormSection();
    }

    private void PreviousSection()
    {
        if(formSections.Count < 1) return;
        if(formPosition < 0) return;
        formPosition--;
        RemoveSection();
        BuildFormSection();
    }

    private void SubmitForm()
    {
        if(!answersHolder.AreRequiredComplete()) return;
        RemoveSection();
        
        VisualElement introduction = labelElement.Instantiate();
        introduction.Q<Label>("title").text = formTitle;
        introduction.Q<Label>("description").text = "Form has been submited. Thanks for participating.";
        formRoot.Add(introduction);
        if(formSubmiter != null) formSubmiter.submitForm(answersHolder);
    }

    private void RemoveSection()
    {
        foreach(Action unregister in unbindingActions)
            unregister.Invoke();
        for(int i = formRoot.childCount - 1; i >= 0; i--)
            formRoot.RemoveAt(i);
        unbindingActions.Clear();
        
        TextFieldFocuser focuser = gameObject.GetComponent<TextFieldFocuser>();
        if(focuser == null) return;
        focuser.UpdateFields();
    }

    private void BuildFormSection()
    {
        //add title and description if its the first element
        if(formPosition == 0)
        {
            VisualElement introduction = labelElement.Instantiate();
            introduction.Q<Label>("title").text = formTitle;
            introduction.Q<Label>("description").text = formDesc;
            formRoot.Add(introduction);
        }
        //add each element
        if(formPosition > -1 && formPosition < formSections.Count)
        {
            foreach(FormElement ele in formSections[formPosition])
            {
                string type = ele.type;
                string question = ele.description;
                string id = ele.id;
                List<string> extras = ele.extra;
                if (ele.required)
                {
                    answersHolder.RegisterRequiredQuestion(formPosition, id);
                    question += "*";
                }
                switch (type)
                {
                    case FormElement.DROP_DOWN:
                        VisualElement dropdown = dropdownElement.Instantiate();
                        dropdown.Q<Label>("questionLabel").text = question;
                        DropdownField dropdownField = dropdown.Q<DropdownField>("dropdown");
                        if(extras.Count > 0)
                        {
                            dropdownField.choices = extras;
                        }
                        if(answersHolder.IsQuestionAnswered(id))
                            dropdownField.value = answersHolder.GetSingleAnswer(id);
                        formRoot.Add(dropdown);

                        EventCallback<ChangeEvent<string>> fillField = (ChangeEvent<string> evt) => {answersHolder.AddSingleAnswer(id, evt.newValue);};
                        dropdownField.RegisterValueChangedCallback(fillField);
                        unbindingActions.Add(() => {dropdownField.UnregisterCallback(fillField);});
                        break;
                    case FormElement.CHECKBOX:
                        if(extras.Count > 0)
                        {
                            VisualElement container = checkboxContainerElement.Instantiate();
                            container.Q<Label>("questionLabel").text = question;
                            List<string> prevValues = answersHolder.GetRepeatableAnswers(id);
                            foreach(string value in extras)
                            {
                                VisualElement check = checkboxElement.Instantiate();
                                check.Q<Label>("optionLabel").text = value;
                                container.Add(check);
                                Toggle toggle = check.Q<Toggle>("Toggle");
                                if(prevValues.Contains(value)) toggle.value = true;
                                
                                EventCallback<ChangeEvent<bool>> checkCallback = (ChangeEvent<bool> evt) => {answersHolder.ToggleRepeatableAnswer(id, value, evt.newValue);};
                                toggle.RegisterValueChangedCallback(checkCallback);
                                unbindingActions.Add(() => {toggle.UnregisterCallback(checkCallback);});
                            }
                            formRoot.Add(container);
                        }
                        break;
                    case FormElement.PARAGRAPH:
                        VisualElement paragraphInput = textInputElement.Instantiate();
                        TextField field = paragraphInput.Q<TextField>("input");
                        field.label = question;
                        if(answersHolder.IsQuestionAnswered(id))
                            field.value = answersHolder.GetSingleAnswer(id);

                        EventCallback<ChangeEvent<string>> paragraphCallback = (ChangeEvent<string> evt) => {answersHolder.AddSingleAnswer(id, evt.newValue);};
                        field.RegisterValueChangedCallback(paragraphCallback);
                        unbindingActions.Add(() => {field.UnregisterCallback(paragraphCallback);});

                        formRoot.Add(paragraphInput);
                        break;
                    case FormElement.RADIO:
                        VisualElement radio = radioButtonElement.Instantiate();
                        RadioButtonGroup radioGroup = radio.Q<RadioButtonGroup>("radioButtons");
                        radioGroup.label = question;
                        if(extras.Count > 0)
                        {
                            radioGroup.choices = extras;
                        }
                        if(answersHolder.IsQuestionAnswered(id))
                            radioGroup.value = extras.IndexOf(answersHolder.GetSingleAnswer(id));

                        EventCallback<ChangeEvent<int>> radioCallback = (ChangeEvent<int> evt) => {answersHolder.AddSingleAnswer(id, extras[evt.newValue]);};
                        radioGroup.RegisterValueChangedCallback(radioCallback);
                        unbindingActions.Add(() => {radioGroup.UnregisterCallback(radioCallback);});

                        formRoot.Add(radio);
                        break;
                    case FormElement.SCALE:
                        if(extras.Count > 0)
                        {
                            VisualElement scale = scaleElement.Instantiate();
                            RadioButtonGroup scaleGroup = scale.Q<RadioButtonGroup>("scale");
                            scaleGroup.label = question;
                            int min = int.Parse(extras[0]);
                            int max = int.Parse(extras[1]);
                            List<string> choices = new();
                            for(int i = min; i <= max; i++)
                            {
                                if(i == min && extras.Count > 2)
                                    choices.Add($"{i} {extras[2]}");
                                else if(i == max && extras.Count > 3)
                                    choices.Add($"{i} {extras[3]}");
                                else
                                    choices.Add($"{i}");
                            }
                            scaleGroup.choices = choices;
                            if(answersHolder.IsQuestionAnswered(id))
                                scaleGroup.value = int.Parse(answersHolder.GetSingleAnswer(id)) - min;

                            EventCallback<ChangeEvent<int>> scaleCallback = (ChangeEvent<int> evt) => {answersHolder.AddSingleAnswer(id, (min + evt.newValue).ToString());};
                            scaleGroup.RegisterValueChangedCallback(scaleCallback);
                            unbindingActions.Add(() => {scaleGroup.UnregisterCallback(scaleCallback);});

                            formRoot.Add(scale);
                        }
                        break;
                    case FormElement.TEXT:
                        VisualElement textInput = textInputElement.Instantiate();
                        TextField textField = textInput.Q<TextField>("input");
                        textField.label = question;
                        if(answersHolder.IsQuestionAnswered(id))
                            textField.value = answersHolder.GetSingleAnswer(id);

                        EventCallback<ChangeEvent<string>> textCallback = (ChangeEvent<string> evt) => {answersHolder.AddSingleAnswer(id, evt.newValue);};
                        textField.RegisterValueChangedCallback(textCallback);
                        unbindingActions.Add(() => {textField.UnregisterCallback(textCallback);});

                        formRoot.Add(textInput);
                        break;
                    default:
                        VisualElement textThing = labelElement.Instantiate();
                        string title = id;
                        if (id.StartsWith(SoundscapeInit))
                        {
                            title = id[15..];
                            Debug.Log($"Found request for ss: {title}");
                        }
                        
                        textThing.Q<Label>("title").text = title;
                        textThing.Q<Label>("description").text = question;

                        if(id.StartsWith(SoundscapeInit)){
                            Soundscape found = library.soundscapeList.GetByTitle(title);
                            if(found != null)
                            {
                                Action ssButtonStart = () => ssWrapper.SetAndStartSoundscape(found);
                                Button btn = new()
                                {
                                    text = $"Start {title}"
                                };
                                btn.clicked += ssButtonStart;
                                unbindingActions.Add(() => btn.clicked -= ssButtonStart);
                                textThing.Add(btn);
                            }
                        }
                        formRoot.Add(textThing);  
                        break;
                }
            }
        }
        //add next and back buttons, and a finish button
        if(formPosition < formSections.Count)
        {
            Button nextButton = new Button();
            nextButton.clicked += NextSection;
            if(formPosition == -1) nextButton.text = "Start";
            else if(formPosition == formSections.Count - 1) nextButton.text = "Finish";
            else nextButton.text = "Next";
            formRoot.Add(nextButton);
        }
        if(formPosition > -1)
        {
            Button prevButton = new Button();
            prevButton.clicked += PreviousSection;
            if(formPosition == 0) prevButton.text = "Return";
            else prevButton.text = "Previous";
            formRoot.Add(prevButton);
        }
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<ScrollView>("ScrollView").scrollOffset = new(0, 0);

        TextFieldFocuser focuser = gameObject.GetComponent<TextFieldFocuser>();
        if(focuser == null) return;
        focuser.UpdateFields();
    }

    private List<List<FormElement>> SeparateElements(List<FormElement> formElements)
    {
        List<List<FormElement>> result = new();
        List<FormElement> section = new();
        foreach(FormElement ele in formElements)
        {
            if (ele.type.Equals(FormElement.SECTION_BREAK))
            {
                result.Add(section);
                section = new();
            }
            section.Add(ele);
        }
        if(section.Count > 0) result.Add(section);
        return result;
    }
}
