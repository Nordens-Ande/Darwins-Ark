using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public class Tile
    {
        public Vector3 pos;

        public Vector3[] vertices;
        public int[] triangles;

        public Tile(Vector3 pos)
        {
            this.pos = pos;
        }

        public float GetHeight()
        {
            return pos.y;
        }
    }
}