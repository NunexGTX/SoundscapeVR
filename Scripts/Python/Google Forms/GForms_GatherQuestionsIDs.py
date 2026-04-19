#from google.oauth2 import service_account
#from googleapiclient.discovery import build

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
            print("   1. - Open your form either in Viewer or in Edit View")
            print("   2. - Get its link, you'll notice that its something like 'https://docs.google.com/forms/d/e/[FormID]/...'")
            print("   3. - Write down that FormID")
            break
        elif needInstructions == "n":
            break
        else:
            print("The input you gave is not a valid option, please try again...")
    

startMessage()

