using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public enum TileType
    {
        Ocean,
        Beach,
        Grass,
        River,
        Concrete
    }

    public class Tile
    {
        public Vector3 position;
        public Vector3 localPosition;

        TileType type;

        //plant varibles
        [SerializeField] private Plant currentPlant = null; //this is the plant that is currently on this tile, if any

        public TileType Type => type;
        public Plant CurrentPlant => currentPlant;
        public bool HasPlant => currentPlant != null;

        [Obsolete] public bool isWater => position.y < 0;

        public Tile(Vector3 position, Vector3 localPosition, TileType type)
        {
            this.position = position;
            this.localPosition = localPosition;
            this.type = type;
        }

        public float GetHeight()
        {
            return position.y;
        }

        ///plant functions
        public void SetPlant(Plant plant)
        {
            currentPlant = plant;
        }

        public void ClearPlant()
        {
            currentPlant = null;
        }
    }
}
