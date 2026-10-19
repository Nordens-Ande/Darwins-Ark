using UnityEngine;

namespace Assets.Scripts.Events.Abstractions
{
    public interface IGameTransitionData<T>
    {
        GameObject Source { get; }
        T PreviousValue { get; }
        T NewValue { get; }
    }
}