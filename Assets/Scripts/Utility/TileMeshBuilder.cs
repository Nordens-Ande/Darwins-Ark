using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utility
{
    public static class TileMeshBuilder
    {
        #region AddTriangle Methods
        public static void AddTriangle(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3 a, Vector3 b, Vector3 c, Color colorA, Color colorB, Color colorC)
        {
            int index = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);

            colors.Add(colorA);
            colors.Add(colorB);
            colors.Add(colorC);

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }
        public static void AddTriangle(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3 a, Vector3 b, Vector3 c, params Color[] vertexColors)
        {
            if (vertexColors.Length < 3)
            {
                Color[] newVertexColors = new Color[4];
                for (int i = 0; i < newVertexColors.Length; i++)
                {
                    int possibleIndex = vertexColors.Length - 1 >= i ? i : vertexColors.Length - 1;
                    newVertexColors[i] = vertexColors[possibleIndex];
                }
                vertexColors = newVertexColors;
            }
            else if (vertexColors.Length > 3)
            {
                Debug.LogWarning($"Recieved more than three colors.\nTriangle creation will still proceed at index:{vertices.Count} however only the first three colors will be utilized.");
            }

            AddTriangle(vertices, triangles, colors, a, b, c, vertexColors[0], vertexColors[1], vertexColors[3]);
        }
        public static void AddTriangle(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3 a, Vector3 b, Vector3 c, Color color)
        {
            AddTriangle(vertices, triangles, colors, a, b, c, color, color, color);
        }


        public static void AddTriangle(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c)
        {
            AddTriangle(vertices, triangles, null, a, b, c, Color.pink, Color.pink, Color.pink);
        }
        #endregion


        #region AddFace Methods
        public static void AddFace(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color colorA, Color colorB, Color colorC, Color colorD)
        {
            int index = vertices.Count;

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);

            if (colors != null)
            {
                colors.Add(colorA);
                colors.Add(colorB);
                colors.Add(colorC);
                colors.Add(colorD);
            }

            triangles.Add(index + 0);
            triangles.Add(index + 1);
            triangles.Add(index + 2);

            triangles.Add(index + 0);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }
        public static void AddFace(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3 a, Vector3 b, Vector3 c, Vector3 d, params Color[] vertexColors)
        {
            if (vertexColors.Length < 4)
            {
                Color[] newVertexColors = new Color[4];
                for (int i = 0; i < newVertexColors.Length; i++)
                {
                    int possibleIndex = vertexColors.Length - 1 >= i ? i : vertexColors.Length - 1;
                    newVertexColors[i] = vertexColors[possibleIndex];
                }
                vertexColors = newVertexColors;
            }
            else if (vertexColors.Length > 4)
            {
                Debug.LogWarning($"Recieved more than four colors.\nFace creation will still proceed at index:{vertices.Count} however only the first four colors will be utilized.");
            }

            AddFace(vertices, triangles, colors, a, b, c, d, vertexColors[0], vertexColors[1], vertexColors[2], vertexColors[3]);
        }
        public static void AddFace(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            AddFace(vertices, triangles, colors, a, b, c, d, color, color, color, color);
        }


        public static void AddFace(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3[] corners, Color colorA, Color colorB, Color colorC, Color colorD)
        {
            if (corners.Length < 4)
            {
                Debug.LogError($"Unable to create face: Recieved less than four corners at index:{vertices.Count}.");
                return;
            }
            else if (corners.Length > 4)
            {
                Debug.LogWarning($"Recieved more than four corners.\nFace creation will still proceed at index:{vertices.Count} however only the first four corners will be utilized.");
            }

            AddFace(vertices, triangles, colors, corners[0], corners[1], corners[2], corners[3], colorA, colorB, colorC, colorD);
        }
        public static void AddFace(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3[] corners, params Color[] vertexColors)
        {
            AddFace(vertices, triangles, colors, corners[0], corners[1], corners[2], corners[3], vertexColors);
        }
        public static void AddFace(List<Vector3> vertices, List<int> triangles, List<Color> colors, Vector3[] corners, Color color)
        {
            AddFace(vertices, triangles, colors, corners[0], corners[1], corners[2], corners[3], color, color, color, color);
        }


        public static void AddFace(List<Vector3> vertices, List<int> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            AddFace(vertices, triangles, null, a, b, c, d, Color.pink, Color.pink, Color.pink, Color.pink);
        }
        public static void AddFace(List<Vector3> vertices, List<int> triangles, Vector3[] corners)
        {
            AddFace(vertices, triangles, null, corners[0], corners[1], corners[2], corners[3], Color.pink);
        }
        #endregion


        #region AddSlopeTop Methods

        public static void AddSlopeTop(
            List<Vector3> vertices, 
            List<int> triangles, 
            List<Color> colors,
            Vector3 tilePos,
            float currentHeight,
            float nwHeight,
            float neHeight,
            float seHeight,
            float swHeight,
            Color color)
        {
            //tilePos.y -= currentHeight;

            Vector3 nw = new Vector3(-0.5f, nwHeight, 0.5f) + tilePos;
            Vector3 ne = new Vector3(0.5f, neHeight, 0.5f) + tilePos;
            Vector3 se = new Vector3(0.5f, seHeight, -0.5f) + tilePos;
            Vector3 sw = new Vector3(-0.5f, swHeight, -0.5f) + tilePos;

            AddFace(vertices, triangles, colors, nw, ne, se, sw, color);

            // West side.
            if (!Mathf.Approximately(nwHeight, swHeight))
            {
                AddTriangle(vertices, triangles, colors, nw, sw, new Vector3(-0.5f, currentHeight, 0.5f) + tilePos, color);
            }

            // East side.
            if (!Mathf.Approximately(neHeight, seHeight))
            {
                AddTriangle(vertices, triangles, colors, ne, new Vector3(0.5f, currentHeight, 0.5f) + tilePos, se, color);
            }

            // North side.
            if (!Mathf.Approximately(nwHeight, neHeight))
            {
                AddTriangle(vertices, triangles, colors, nw, new Vector3(-0.5f, currentHeight, 0.5f) + tilePos, ne, color);
            }

            // South side.
            if (!Mathf.Approximately(swHeight, seHeight))
            {
                AddTriangle(vertices, triangles, colors, sw, se, new Vector3(-0.5f, currentHeight, -0.5f) + tilePos, color);
            }
        }

        #endregion
    }
}

