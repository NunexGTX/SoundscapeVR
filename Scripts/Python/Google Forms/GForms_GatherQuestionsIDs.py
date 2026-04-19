#from google.oauth2 import service_account
#from googleapiclient.discovery import build

def startMessage():
    print("*** Google Forms - Question info and IDs gatherer ***")
    print("This script will use the Google Forms API to get all the questions and identify all of their question IDs that we can use to send a Questionnaire POST Request so the user can answer the questions inside Unity")

    print()
    setupInstructions()

def setupInstructions():
    print("--- SETUP INSTRUCTIONS ---")
    print("For this script, you'll need: ")
    print("   1 - Your Team's Google Forms API credentials.json")
    print("   2 - The Form ID")

startMessage()