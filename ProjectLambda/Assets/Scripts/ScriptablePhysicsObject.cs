using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptablePhysicsObject : CustomPhysicsObject {
    public static readonly string NO_CURRENT_ACTION = string.Empty;

    Dictionary<string, ScriptableAction> actions;
    string current_action = NO_CURRENT_ACTION;

    protected virtual void onActionFinished(string name, bool was_interupted) {
        Debug.Log("action finished");
    }
    protected override void awake()
    {
        base.awake();
        actions = new Dictionary<string, ScriptableAction>();
    }
    protected override void update() {
        base.update();
        if (current_action == NO_CURRENT_ACTION) {
            return;
        }
        bool res = actions[current_action].continueAction();
        if (res) {
            actions[current_action].endAction();
            onActionFinished(current_action, false);
            current_action = NO_CURRENT_ACTION;
        }
    }
    protected override void fixedUpdate()
    {
        base.fixedUpdate();
    }

    protected bool addAction(string name, ScriptableAction action) {
        if (actions.ContainsKey(name))
        {
            return false;
        }
        actions.Add(name, action);
        return true;
    }
    protected string getCurrentAction() {
        return current_action;
    }
    protected void startAction(string name, bool interupt) {
        if (!actions.ContainsKey(name)) {
            return;
        }
        if (current_action != NO_CURRENT_ACTION && !interupt) {
            return;
        }
        current_action = name;
        actions[current_action].startAction();
    }
    protected void interuptAction() {
        actions[current_action].interuptAction();
        onActionFinished(current_action, true);
        current_action = NO_CURRENT_ACTION;
    }
}
public interface IScriptableAction {
    void startAction();
    bool continueAction();
    void interuptAction();
    void endAction();
}
public abstract class ScriptableAction : IScriptableAction
{
    protected ScriptablePhysicsObject parent;
    public ScriptableAction(ScriptablePhysicsObject p){
        parent = p;
    }
    public abstract void startAction();
    public abstract bool continueAction();
    public abstract void endAction();
    public abstract void interuptAction();
}