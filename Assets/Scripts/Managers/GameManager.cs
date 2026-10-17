using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


public enum GameState
{
    Playing,
    Paused
}

public enum GameMode
{
    Game,
    Deity,
    Terraform,
    Planting
}

public interface IGameTransitionData<T>
{
    GameObject Source { get; }
    T PreviousValue { get; }
    T NewValue { get; }
}

public class GameStateChangedData : IGameTransitionData<GameState>
{
    public GameObject Source { get; private set; }

    public GameState PreviousValue { get; private set; }
    public GameState NewValue { get; private set; }
    
    public GameStateChangedData(GameObject source, GameState previousState, GameState newState)
    {
        Source = source;
        PreviousValue = previousState;
        NewValue = newState;
    }
}
public class GameModeChangedData : IGameTransitionData<GameMode>
{
    public GameObject Source { get; private set; }
    public GameMode PreviousValue { get; private set; }
    public GameMode NewValue { get; private set; }

    public GameModeChangedData(GameObject source, GameMode previousMode, GameMode newMode)
    {
        Source = source;
        PreviousValue = previousMode;
        NewValue = newMode;
    }
}

public class GameManager : SingletonManager<GameManager>
{
    [SerializeField, ReadOnly] private GameState currentState = GameState.Playing;
    [SerializeField, ReadOnly] private GameMode currentMode = GameMode.Game;

    public GameState CurrentState => currentState;
    public GameMode CurrentMode => currentMode;

    public event Action<GameStateChangedData> OnGameStateChanged;
    public event Action<GameModeChangedData> OnGameModeChanged;


    public bool ChangeState(GameObject source, GameState newState)
    {
        if (currentState == newState)
        {
            Debug.LogWarning($"{source.name} attempted to change GameState to the same current one, {newState.ToString()}.\nIs the transition working properly?");
            return false;
        }

        GameState previousState = currentState;
        currentState = newState;

        OnGameStateChanged?.Invoke(new GameStateChangedData(source, previousState, newState));
        Debug.Log($"New GameState has been changed by {source.name} to {newState.ToString()}");

        return true;
    }

    public bool ChangeMode(GameObject source, GameMode newMode)
    {
        if (currentMode == newMode)
        {
            Debug.LogWarning($"{source.name} attempted to change GameMode to the same current one, {newMode.ToString()}.\nIs the transition working properly?");
            return false;
        }

        GameMode previousMode = currentMode;
        currentMode = newMode;

        OnGameModeChanged?.Invoke(new GameModeChangedData(source, previousMode, newMode));
        Debug.Log($"New GameMode has been changed by {source.name} to {newMode.ToString()}");

        return true;
    }
}
