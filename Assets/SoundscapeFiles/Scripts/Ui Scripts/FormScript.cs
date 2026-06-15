using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class FormScript : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject audioVideoMenu;
    [SerializeField] private SoundscapeWrapper ssWrapper;
    [SerializeField] private SoundscapeLibrary library;
    [SerializeField] private VisualTreeAsset radioButtonElement;
    [SerializeField] private VisualTreeAsset labelElement;
    [SerializeField] private VisualTreeAsset textInputElement;
    [SerializeField] private VisualTreeAsset imageElement;
    [SerializeField] private VisualTreeAsset scaleElement;
    [SerializeField] private VisualTreeAsset checkboxContainerElement;
    [SerializeField] private VisualTreeAsset checkboxElement;
    [SerializeField] private VisualTreeAsset dropdownElement;
    public const string SoundscapeInit = "LoadSoundscape:";
    private Button returnBtn;
    private VisualElement formRoot;
    private List<List<FormElement>> formSections = new();
    private string formTitle = "";
    private string formDesc= "";
    private int formPosition = -1;
    private List<Action> unbindingActions = new();
    private FormAnswersHolder answersHolder;
    private Label notAllMandatory;
    private Coroutine allMandatory;
    public int disappearAllMandatoryTimeout = 5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        formPosition = -1;
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        returnBtn = root.Q<Button>("returnBtn");
        returnBtn.clicked += Return;
        if(library.form == null) return;
        formRoot = root.Q<VisualElement>("formRoot");
        
        if(Application.internetReachability != NetworkReachability.NotReachable)
        {
            
            root.Q<Label>("formTitle").text = "No internet connection. Try again later.";
            root.Q<Button>("nextBtn").visible = false;
            Debug.LogError($"[SoundscapeVR] Error: Can't load form as there is no internet!");
            return;
        }
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
        audioVideoMenu?.SetActive(true);
    }

    private void NextSection()
    {
        if(formSections.Count == formPosition) return;
        if (!answersHolder.AreRequiredComplete(formPosition))
        {
            notAllMandatory.style.display = DisplayStyle.Flex;
            if(allMandatory != null) StopCoroutine(allMandatory);
            allMandatory = StartCoroutine(DisappearAllMandatory());
            return;
        }
        if(formPosition >= formSections.Count - 1)
        {
            SubmitForm();
            return;
        }
        formPosition++;
        hideAllMandatoryMessage();
        RemoveSection();
        BuildFormSection();
    }

    IEnumerator DisappearAllMandatory()
    {
        yield return new WaitForSeconds(disappearAllMandatoryTimeout);
        hideAllMandatoryMessage();
    }

    void hideAllMandatoryMessage()
    {
        if(notAllMandatory != null) notAllMandatory.style.display = DisplayStyle.None;
        if (allMandatory != null) StopCoroutine(allMandatory);
    }

    private void PreviousSection()
    {
        if(formSections.Count < 1) return;
        if(formPosition < 0) return;
        formPosition--;
        hideAllMandatoryMessage();
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
        GoogleForm.submitForm(answersHolder,library.form.formViewId);
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
                    case FormElement.IMAGE:
                        VisualElement imageInstance = imageElement.Instantiate();
                        Image imageField = imageInstance.Q<Image>("image");
                        _ = LoadImageAsync(extras[0], imageField);
                        formRoot.Add(imageInstance);
                        break;
                    case FormElement.GRID_RADIO:
                        if (extras.Count > 1)
                        {
                            string rowSubLabel = extras[0];
                            List<string> gridChoices = extras.GetRange(1, extras.Count - 1);
                            VisualElement gridContainer = formRoot.Q<VisualElement>(name: question);

                            if (gridContainer == null)
                            {
                                // Create the main container asset if it's the first row encountered
                                gridContainer = labelElement.Instantiate();
                                gridContainer.name = question;
                                gridContainer.Q<Label>("title").text = "";
                                gridContainer.Q<Label>("description").text = question;
                                formRoot.Add(gridContainer);
                            }

                            VisualElement rowInstance = radioButtonElement.Instantiate();
                            RadioButtonGroup rowRadioGroup = rowInstance.Q<RadioButtonGroup>("radioButtons");

                            rowRadioGroup.label = rowSubLabel;
                            rowRadioGroup.style.fontSize = 12;
                            rowRadioGroup.choices = gridChoices;

                            if (answersHolder.IsQuestionAnswered(id))
                            {
                                string savedAnswer = answersHolder.GetSingleAnswer(id);
                                int savedIndex = gridChoices.IndexOf(savedAnswer);
                                if (savedIndex >= 0)
                                {
                                    rowRadioGroup.value = savedIndex;
                                }
                            }

                            EventCallback<ChangeEvent<int>> gridCallback = (ChangeEvent<int> evt) => 
                            {
                                if (evt.newValue >= 0 && evt.newValue < gridChoices.Count)
                                {
                                    answersHolder.AddSingleAnswer(id, gridChoices[evt.newValue]);
                                }
                            };
                            
                            rowRadioGroup.RegisterValueChangedCallback(gridCallback);
                            unbindingActions.Add(() => { rowRadioGroup.UnregisterCallback(gridCallback); });
                            gridContainer.Add(rowInstance);
                            Debug.Log($"Added row {ele.ToString()}");
                        }
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
                            } else
                            {
                                textThing.Q<Label>("description").text += $"\nError: Could not load soundscape {title} as it does not exist.";
                            }
                        }
                        formRoot.Add(textThing);  
                        break;
                }
            }
        }
        //Add allmandatory warning
        if (!answersHolder.AreRequiredComplete(formPosition))
        {
            notAllMandatory = new Label();
            notAllMandatory.text = "*Please fill all mandatory questions";
            notAllMandatory.style.display = DisplayStyle.None;
            notAllMandatory.style.color = new StyleColor(Color.red);
            notAllMandatory.style.fontSize = new StyleLength(14f);
            notAllMandatory.style.paddingLeft = new Length(6, LengthUnit.Pixel);
            formRoot.Add(notAllMandatory);
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
    
    private async Task LoadImageAsync(string url, Image targetImageField)
    {
        if (string.IsNullOrEmpty(url) || targetImageField == null) return;

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            // Send the request and wait asynchronously until it finishes
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield(); // Yield control back to Unity, preventing freezing
            }

            // Handle network errors or bad responses
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load image from URL: {url}. Error: {request.error}");
                return;
            }

            // Extract the downloaded Texture2D
            Texture2D downloadedTexture = DownloadHandlerTexture.GetContent(request);

            if (downloadedTexture != null)
            {
                // Assign the texture directly to the UI Toolkit Image component
                if(targetImageField != null) targetImageField.image = downloadedTexture;
                
                // 1. Force the image to match the horizontal width of the UI container
                targetImageField.style.width = Length.Percent(100);

                // 2. Calculate the aspect ratio (Width / Height)
                float aspectRatio = (float)downloadedTexture.width / downloadedTexture.height;

                // 3. Use USS AspectRatio property to force UI Toolkit to compute height dynamically
                // Formula handled internally by Unity layout engine: height = width / aspectRatio
                targetImageField.style.aspectRatio = aspectRatio;

                // 4. Ensure height isn't overridden by stale layout rules
                targetImageField.style.height = StyleKeyword.Null;
                targetImageField.style.flexGrow = 0;
            }
        }
    }
}
