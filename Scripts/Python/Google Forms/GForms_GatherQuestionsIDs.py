import tkinter as tk
from tkinter import filedialog
import csv
import json.encoder
#Google API
from google.oauth2 import service_account
from googleapiclient.discovery import build

def startMessage():
    print("*** Google Forms - Question info and IDs gatherer ***")
    print("This script will use the Google Forms API to get all the questions and identify all of their question IDs that we can use to send a Questionnaire POST Request so the user can answer the questions inside Unity")

    print()
    setupInstructions()
    print()
    input("Press Enter when you're ready to continue...")

def setupInstructions():
    print("--- SETUP INSTRUCTIONS ---")
    print("For this script, you'll need: ")
    print("   1 - Your Team's Google Forms API credentials.json")
    print("   2 - The Form ID")
    print()
    
    while True:
        needInstructions = input("Do you need instructions on how to get these [Y/N]? ").strip().lower()
        
        if needInstructions == "y":
            print("#1 - Getting Google Forms API credentials.json")
            print("   1. - Go to https://console.cloud.google.com")
            print("   2. - Click on the Project Name button on the right of the Google Cloud Logo")
            print("   3. - Click 'New Project' > give it a name you want > click 'Create'")
            print("   4. - Click on the Project Name button again and select your newly created project once its ready")
            print("   5. - In the left sidebar, go to APIs & Services > Library")
            print("   6. - Search for 'Google Forms API'")
            print("   7. - Click on it > click 'Enable'")
            print("   8. - In the left sidebar, go to APIs & Services > Credentials")
            print("   9. - Click '+ Create Credentials' > choose 'Service Account'")
            print("  10. - Fill in any name and ID you prefer")
            print("  11. - Click 'Create and Continue'")
            print("  12. - For the role, select 'Viewer'")
            print("  13. - Click 'Continue' > 'Done'")
            print("  14. - Still on the Credentials page, find your new service account in the list")
            print("  15. - Click on its name to open")
            print("  16. - Go to the 'Keys' tab")
            print("  17. - Click 'Add Key' > 'Create new key'")
            print("  18. - Choose JSON > click 'Create'")
            print("  19. - Save the donwloaded .json in a place you'll remember (You will pick it after)")
            print("  20. - On the Credentials page still, copy the Service Account email you just created")
            print("  21. - Open the Google Form on the browser in the Edit View")
            print("  22. - Click the three-dot menu at the top right > 'Add collaborators'")
            print("  23. - Paste the service account email > set role to 'Viewer' > click 'Send'")
            print()
            print("#2 - How to get the Google Forms ID")
            print("   1. - Open your form in Edit View")
            print("   2. - Get its link, you'll notice that its something like 'https://docs.google.com/forms/d/e/[FormID]/...'")
            print("   3. - Write down that FormID")
            break
        elif needInstructions == "n":
            break
        else:
            print("The input you gave is not a valid option, please try again...")
    

startMessage()

def selectFile(fileName, filetype, filetypename):
    root = tk.Tk()
    root.withdraw()

    file_path = filedialog.askopenfilename(
        title="Select your "+fileName,
        filetypes=[(filetypename, "*"+filetype), ("All files", "*.*")]
    )

    root.destroy()

    if file_path:
        return file_path
    else:
        print("No file selected.")

while True:
    credentialsJson = selectFile("credentials.json",".json","json files")
    if credentialsJson:
        break

print("Google Forms API credentials at: ",credentialsJson)
print()
FormID = ""
with open(credentialsJson, "r") as f:
    loadedCreds = json.loads(f.read())
    if(not loadedCreds.get("formId")):
        FormID = input("Your Google Form ID: ")
    else:
        FormID = loadedCreds["formId"]
print()

API_Scopes = ['https://www.googleapis.com/auth/forms.body.readonly'] #This will allow to read the details of the form via the API

'''This authenticates us to the Google API
Since we are using a service account (kinda like a fake one) that should have access to the form if all steps were followed
Then we don't need to log in in the browser'''
def authenticate(credentialsJson, scopes):
    token = service_account.Credentials.from_service_account_file(
        credentialsJson,
        scopes=scopes
    )
    return token

token = authenticate(credentialsJson, API_Scopes)

def QuestionsGatherer():
    #The the form's raw data
    formData = FormsContentRetriever(FormID,token)

    resultDict = {}
    #Gather the Questions
    Title = formData.get('info', {}).get('title')
    description = formData.get('info', {}).get('description')
    uri: str = formData.get("responderUri")
    formId = uri.replace("https://docs.google.com/forms/d/e/", "").replace("/viewform", "")
    resultDict["formTitle"] = Title
    resultDict["formViewId"] = formId
    resultDict["description"] = description
    Questions = []

    items = formData.get('items', [])
    for item in items:
        # Check if the item is a question (ignores layout elements like images/headers)
        if 'questionItem' in item:
            question = item['questionItem']['question']
            q_id = "entry." + str(int(question.get('questionId'),16)) #Temos de converter o entry id de hexadecimal para int para podermos responder com o post request
            q_name = item.get('title', "Untitled Question")
            _choiceQuestion: dict = question.get('choiceQuestion', {})

            # get type of question
            if 'textQuestion' in question:
                if 'paragraph' in question['textQuestion']:
                    q_type = 'PARAGRAPH'
                else: 
                    q_type = 'TEXT'
            elif 'scaleQuestion' in question:
                q_type = "SCALE"
            elif 'choiceQuestion' in question:
                q_type = _choiceQuestion['type']
            else:
                continue

            #after determining type, we get any extra information that may be relevant
            q_extra = []
            if(_choiceQuestion):
                q_extra = [entry["value"] for entry in _choiceQuestion["options"]]
            elif(q_type == "SCALE"):
                scaler = question["scaleQuestion"]
                q_extra = [str(scaler["low"]), str(scaler["high"])]
                if(scaler["lowLabel"]):
                    q_extra.extend([scaler["lowLabel"], scaler["highLabel"]])

            itemDict: dict = {
                "id": q_id,
                "type": q_type,
                "description": q_name,
                "extra": q_extra
            }
            Questions.append(itemDict)
        elif 'textItem' in item or 'pageBreakItem' in item:
            itemDict: dict = {
                "id": item.get("title", ""),
                "type": "TITLE" if 'textItem' in item else "BREAK",
                "description": item.get("description", ""),
                "extra": ""
            }
            Questions.append(itemDict)

    resultDict["questions"] = Questions
    #Print the Questions
    showQuestionsJson(resultDict)
    #Save questions to a .csv
    saveQuestionstoJSON(resultDict)


def FormsContentRetriever(formID, token):
    service = build('forms', 'v1', credentials=token)
    formData = service.forms().get(formId=formID).execute()
    print("Form Data: ",formData)
    print()
    return formData

def showQuestionsJson(formDict: dict):
    print(f"GForm: {formDict["formTitle"]}\n")
    print(json.dumps(formDict, indent=4))

def showQuestions(title, questions):
    print(f"GForm: {title}\n")
    print(f"{'ID':<20} | {'Type':<20} | {'Question Name'}") #:<15 and :<20 are a fast way to insert multiple space characters
    print("-" * 60) #Prints 60 dashes

    for question in questions:
        q_id = question[0]
        q_type = question[1]
        q_name = question[2]

        print(f"{q_id:<20} | {q_type:<20} | {q_name}") #Formats these and prints them on the console

def saveQuestionstoCSV(form_title, questions):
    print("Saving the data to ./GFormQuestions_{form_title}.csv")
    with open(f"GFormQuestions_{form_title}.csv", "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["ID","Type","Question Name"])
        writer.writerows(questions)

def saveQuestionstoJSON(jsonDict: dict):
    choice: str = ""
    while(choice != "y" and choice != "n"):
        choice = input("Attempt to write to streaming assets? [Y/n] ").strip().lower()
    file_name = "./Assets/StreamingAssets/configs/questionnaire_config.json" if choice.lower() == "y" else "./questionnaire_config.json"
    print(f"Saving the data to {file_name}")
    with open(file_name, "w", newline="") as f:
        f.write(json.dumps(jsonDict, indent=4))

QuestionsGatherer()