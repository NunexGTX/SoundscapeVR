using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class State
{

    public State(string stateName){this.stateName = stateName;}
    public State RegisterAction(string condition, Action action, State nextState)
    {
        transitions[condition] = action;
        stateFlow[condition] = nextState;
        return this;
    }
    public string stateName;
    private Dictionary<string, Action> transitions = new();
    private Dictionary<string, State> stateFlow = new();
    public State Transition(string condition)
    {
        if(!transitions.ContainsKey(condition)) return this;
        transitions[condition].Invoke();
        return stateFlow[condition];
    }
}