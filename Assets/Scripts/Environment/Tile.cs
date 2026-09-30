using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public class Tile
    {
        public Vector3 position;
        public Vector3 localPosition;

        public Vector2Int GridPosition => new Vector2Int((int)position.x, (int)position.z);

        //plant varibles
        [SerializeField] private Plant currentPlant = null; //this is the plant that is currently on this tile, if any
        public Plant CurrentPlant => currentPlant;
        public bool HasPlant => currentPlant != null;

        public bool isWater => position.y < 0;

        public Tile(Vector3 position, Vector3 localPosition)
        {
            this.position = position;
            this.localPosition = localPosition;
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
