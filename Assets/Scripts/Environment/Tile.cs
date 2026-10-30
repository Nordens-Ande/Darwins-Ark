using System;
using UnityEngine;


namespace Assets.Scripts.Environment
{
    public class Tile
    {
        private readonly Chunk owner = null;
        private readonly Vector2Int index;

        private Vector3 position;
        private Vector3 localPosition;

        private TileType type;
        private float waterHeight = 0f;

        public Vector2Int GridPosition => new Vector2Int(Mathf.RoundToInt(Position.x), Mathf.RoundToInt(Position.z));

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

        public Vector3 Position => position;
        public Vector3 LocalPosition => localPosition;
        public float Height
        {
            get => Position.y;
            set
            {
                owner.isDirty = true;

                position.y = value;
                localPosition.y = Position.y - owner.position.y;

                //CleanUpTile();
            }
        }
        public float WaterHeight
        {
            get => Position.y;
            set
            {
                owner.isDirty = true;

                waterHeight = value;
                if (waterHeight < position.y)
                    Debug.LogWarning($"Tile {index.ToString()} at position {position} in chunk {owner.position} water height was set to be below its actual height.\nPlease check that you are setting the correct tile.");
                if (type != TileType.Ocean && type != TileType.River && type != TileType.Lake)
                    Debug.LogWarning($"Tile {index.ToString()} at position {position} in chunk {owner.position} is not a water type tile.\nAre you sure you should modify the water height?");

                //CleanUpTile();
            }
        }

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

        [Obsolete] public bool isWater => Position.y < 0;

        public Tile(Chunk owner, Vector2Int index, Vector3 position, Vector3 localPosition, TileType type)
        {
            this.owner = owner;
            this.index = index;
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

        public void SetType(TileType newType)
        {
            owner.isDirty = true;
            type = newType;
        }
        //public void SetOnlyValidType(TileType newType)
        //{
        //    bool landType = type == TileType.Grass || type == TileType.Beach || type == TileType.;

        //    bool succesfull = newType switch
        //    {
        //        TileType.Beach
        //    };
        //}

        public float GetHeight()
        {
            return Position.y;
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

        // This method is called every time the tile gets dirty, currently the tile itself will call this, but it could be more applicable in future to only have the chunk do it
        public void CleanUpTile()
        {
            if (currentPlant != null)
                currentPlant.transform.position = new Vector3(currentPlant.transform.position.x, Position.y, currentPlant.transform.position.z);
        }
    }
}
