using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Events.Data
{
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
}
