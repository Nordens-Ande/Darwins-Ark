using UnityEngine;

public class WindDeityManager : MonoBehaviour
{
    public static WindDeityManager Instance;

    public enum WindDirection
    {
        None, 
        North,
        South,
        East,
        West
    }

    [SerializeField] private WindDirection windDirection = WindDirection.None;
    [SerializeField] private int windLength = 4;

    public WindDirection CurrentDirection => windDirection;
    public int WindLength => windLength;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Vector2Int GetWindDirection()
    {
        switch (windDirection)
        {
            case WindDirection.North:
                return new Vector2Int(0, 1);

            case WindDirection.South:
                return new Vector2Int(0, -1);

            case WindDirection.East:
                return new Vector2Int(1, 0);

            case WindDirection.West:
                return new Vector2Int(-1, 0);

            default:
                return Vector2Int.zero;
        }
    }
}
