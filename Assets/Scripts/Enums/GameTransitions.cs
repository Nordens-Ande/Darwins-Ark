using UnityEngine;

namespace Assets.Enums.GameTransitions
{
    // Defines what state the game is, is it running or paused?
    public enum GameState
    {
        Playing,
        Paused
    }

    // Mode defines what "player" mode the game is in, i.e is the player in terraform, planting or deity meny etc.
    public enum GameMode
    {
        Game,
        Deity,
        Terraform,
        Planting
    }
}