using System.Collections.Generic;

public class FormAnswersHolder
{
    private List<KeyValuePair<string, string>> repeatableAnswers = new();
    private Dictionary<string, string> singleAnswers = new();
    private List<KeyValuePair<int, string>> requiredQuestions = new();
    public int pageCount = 1;

    public bool AreRequiredComplete(){return AreRequiredComplete(int.MaxValue);}
    public bool AreRequiredComplete(int idxLimit)
    {
        foreach(KeyValuePair<int, string> question in requiredQuestions)
        {
            if(question.Key > idxLimit) continue;
            string qId = question.Value;
            if(!IsQuestionAnswered(qId)) return false;
        }
        return true;
    }

    public bool IsQuestionAnswered(string qId)
    {
        if(singleAnswers.ContainsKey(qId)) return true;
        foreach(KeyValuePair<string, string> repeatQuestion in repeatableAnswers)
        {
            if(repeatQuestion.Key.Equals(qId)) return true;
        }
        return false;
    }

    public string GetSingleAnswer(string qId){return singleAnswers[qId];}
    public List<string> GetRepeatableAnswers(string qId)
    {
        List<string> matches = new();
        foreach(KeyValuePair<string, string> repeatQuestion in repeatableAnswers)
        {
            if(repeatQuestion.Key.Equals(qId)) matches.Add(repeatQuestion.Value);
        }
        return matches;
    }

    public void RegisterRequiredQuestion(int sectionIdx, string qId)
    {
        foreach(KeyValuePair<int, string> registry in requiredQuestions)
        {
            if(registry.Key == sectionIdx && registry.Value.Equals(qId)) return;
        }
        requiredQuestions.Add(new(sectionIdx, qId));
    }

    public void AddSingleAnswer(string qId, string value)
    {
        if(string.IsNullOrWhiteSpace(value)) RemoveSingleAnswer(qId);
        else singleAnswers[qId] = value;
    }
    public void ToggleRepeatableAnswer(string qId, string value, bool toggleOn)
    {
        //remove any previous instances of this
        repeatableAnswers.RemoveAll(entry => {return entry.Key.Equals(qId) && entry.Value.Equals(value);});

        //if we're toggling it off (unselecting it) then this is where we end;
        if(!toggleOn) return;
        
        //otherwise, we set the value
        repeatableAnswers.Add(new(qId, value));
        
    }

    public void RemoveSingleAnswer(string qId)
    {
        singleAnswers.Remove(qId);
    }

    public List<KeyValuePair<string, string>> GetAnswerList()
    {
        List<KeyValuePair<string, string>> result = new();
        result.AddRange(repeatableAnswers);
        foreach(KeyValuePair<string, string> dictEntry in singleAnswers)
        {
            result.Add(new(dictEntry.Key, dictEntry.Value));
        }
        return result;
    }
}