using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public class Tile
    {
        public Vector3 position;
        public Vector3 localPosition;

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
    }
}