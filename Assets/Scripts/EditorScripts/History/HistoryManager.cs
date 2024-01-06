using System.Collections.Generic;

public class HistoryManager
{
    private readonly List<HistoryAction> _history = new ();
    private int _currentAction = 0; // 0 means current, if it's negative then we can redo
    private LogicScript _logic;
    
    public HistoryManager(LogicScript logic)
    {
        _logic = logic;
    }

    public void ClearHistory()
    {
        _history.Clear();
        _currentAction = 0;
    }
    
    public void AddAction(HistoryAction action)
    {
        if (_currentAction != 0) // if we're not at the end of the history
        {
            _history.RemoveRange(_currentAction, _history.Count - _currentAction);
            _currentAction = 0;
        }
        
        _history.Add(action);
    }
    
    public void Undo()
    {
        if (_currentAction >= _history.Count)
            return;
        
        _history[_currentAction].Undo(_logic);
        _currentAction++;
    }
    
    public void Redo()
    {
        if (_currentAction <= 0)
            return;
        
        _currentAction--;
        _history[_currentAction].Redo(_logic);
    }
}