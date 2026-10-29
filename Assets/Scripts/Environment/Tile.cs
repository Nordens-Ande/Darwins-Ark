using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public class Tile
    {
        public Vector3 position;
        public Vector3 localPosition;

        private TileType type;

        public Vector2Int GridPosition => new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));

        //plant varibles
        [SerializeField] private Plant currentPlant = null; //this is the plant that is currently on this tile, if any

        //rain
        private bool isWetFromRain;
        private bool runRainTimer;
        private float wetTimer;
        private float wetDuration;

        //wind
        private float windStrength;
        private Vector3 windDirection;

        public TileType Type => type;
        public Plant CurrentPlant => currentPlant;

        public bool IsWetFromRain => isWetFromRain;

        public float WindStrength
        {
            get => windStrength;
            set => windStrength = value;
        }
        public Vector3 WindDirection
        {
            get => windDirection;
            set => windDirection = value;
        }

        public bool HasPlant => currentPlant != null;

        [Obsolete] public bool isWater => position.y < 0;

        public Tile(Vector3 position, Vector3 localPosition, TileType type)
        {
            this.position = position;
            this.localPosition = localPosition;
            this.type = type;

            isWetFromRain = false;
            runRainTimer = false;
            wetTimer = 0;
            wetDuration = 60; // 1 minute

            windStrength = 0;
            windDirection = Vector3.zero;
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

        public void RainStarted()
        {
            isWetFromRain = true;
        }

        public void RainStopped()
        {
            runRainTimer = true;
            wetTimer = 0;
        }

        public void ApplyWindEffect(float windStrength, Vector2 windDirection, float maxWindStrength) // maybe wrong need double check later!
        {
            Vector3 windDir = new Vector3(windDirection.x, 0, windDirection.y);
            Vector3 addedWind = windDir * windStrength;
            Vector3 currentWind = this.windDirection * this.windStrength;
            Vector3 totalWind = addedWind + currentWind;
            this.windDirection = totalWind.normalized;
            this.windStrength = totalWind.magnitude;
            this.windStrength = Mathf.Min(maxWindStrength, this.windStrength);
            //Debug.Log($"Tile {position.x}, {position.y}, {position.z}; wind strength: {this.windStrength}, dir = {this.windDirection}");
        }

        public void Update(float dt)
        {
            if(isWetFromRain)
            {
                if(runRainTimer)
                {
                    if(wetTimer >= wetDuration)
                    {
                        isWetFromRain = false;
                        runRainTimer = false;
                        wetTimer = 0;
                        return;
                    }
                    wetTimer += dt;
                }
            }
            if (windStrength <= 0) return;
            windStrength -= 1 * Time.deltaTime;
            if(windStrength <= 0)
            {
                windStrength = 0;
                windDirection = Vector3.zero;
            }
            //if(position.x == 0 && position.z == 0)
            //    Debug.Log(windStrength);
        }
    }
}
