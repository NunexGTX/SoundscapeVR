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
    private Button returnBtn;
    private VisualElement formRoot;
    private List<List<FormElement>> formSections = new();
    private string formTitle = "";
    private string formDesc= "";
    private int formPosition = -1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        formPosition = -1;
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        returnBtn = root.Q<Button>("returnBtn");
        returnBtn.clicked += Return;
        if(library.form == null) return;
        formRoot = root.Q<VisualElement>("formRoot");
        formSections = SeparateElements(library.form.questions);
        formTitle = library.form.formTitle;
        formDesc = library.form.description;
        Button nextBtn = root.Q<Button>("nextBtn");
        nextBtn.clicked += NextSection;
        Label title = root.Q<Label>("formTitle");
        title.text = library.form.formTitle;
    }

    void OnDisable()
    {
        returnBtn.clicked -= Return;
    }

    private void Return()
    {
        mainMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    private void NextSection()
    {
        if(formSections.Count == formPosition) return;
        if(formPosition >= formSections.Count - 1) return;
        formPosition++;
        for(int i = formRoot.childCount - 1; i >= 0; i--)
            formRoot.RemoveAt(i);
        BuildFormSection();
    }

    private void PreviousSection()
    {
        if(formSections.Count < 1) return;
        if(formPosition < 0) return;
        formPosition--;
        for(int i = formRoot.childCount - 1; i >= 0; i--)
            formRoot.RemoveAt(i);
        BuildFormSection();
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
                        formRoot.Add(dropdown);
                        break;
                    case FormElement.CHECKBOX:
                        if(extras.Count > 0)
                        {
                            VisualElement container = checkboxContainerElement.Instantiate();
                            container.Q<Label>("questionLabel").text = question;
                            foreach(string value in extras)
                            {
                                VisualElement check = checkboxElement.Instantiate();
                                check.Q<Label>("optionLabel").text = value;
                                container.Add(check);
                            }
                            formRoot.Add(container);
                        }
                        break;
                    case FormElement.PARAGRAPH:
                        VisualElement paragraphInput = textInputElement.Instantiate();
                        paragraphInput.Q<TextField>("input").label = question;
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
                        formRoot.Add(radio);
                        break;
                    case FormElement.SCALE:
                        VisualElement scale = scaleElement.Instantiate();
                        RadioButtonGroup scaleGroup = scale.Q<RadioButtonGroup>("scale");
                        scaleGroup.label = question;
                        if(extras.Count > 0)
                        {
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
                        }
                        formRoot.Add(scale);
                        break;
                    case FormElement.TEXT:
                        VisualElement textInput = textInputElement.Instantiate();
                        textInput.Q<TextField>("input").label = question;
                        formRoot.Add(textInput);
                        break;
                    default:
                        VisualElement textThing = labelElement.Instantiate();
                        textThing.Q<Label>("title").text = id;
                        textThing.Q<Label>("description").text = question;
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
