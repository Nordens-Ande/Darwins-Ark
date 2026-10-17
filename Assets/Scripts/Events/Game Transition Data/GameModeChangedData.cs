using Assets.Enums.GameTransitions;
using Assets.Scripts.Events.Abstractions;
using UnityEngine;

namespace Assets.Scripts.Events.Data
{
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
}