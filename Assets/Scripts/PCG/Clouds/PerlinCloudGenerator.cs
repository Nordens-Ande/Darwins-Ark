using System;
using UnityEngine;

public class PerlinCloudGenerator : MonoBehaviour
{
    private Texture2D texture;
    private Color32[] pixels;

    [Header("Texture")]
    [SerializeField] private int resolution = 32;

    [Header("Noise")]
    [SerializeField] private float noiseScale = 4f;

    [Header("Cloud Shape")]
    [Range(0f, 1f)]
    [SerializeField] private float threshold = 0.5f;

    [Range(0f, 3f)]
    [SerializeField] private float opacityStrength = 1.5f;

    [Range(0.1f, 3f)]
    [SerializeField] private float edgeFadeStrength = 1f;

    [Range(0.1f, 3f)]
    [SerializeField] private float cloudWidth;

    [Range(0.1f, 3f)]
    [SerializeField] private float cloudHeight = 1f;

    [Header("Cloud Center")]
    [Range(0f, 1f)]
    [SerializeField] private float centerStrength = 0.6f;

    [Range(01f, 5f)]
    [SerializeField] private float centerFallOff = 2f;

    private void Awake()
    {
        texture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false
            );

        texture.filterMode = FilterMode.Bilinear;

        pixels = new Color32[resolution * resolution];
    }

    public Texture2D GenerateCloud(float shapeTime, int customSeed)
    {
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x  < resolution; x++)
            {
                float sampleX =
                    (float)x / resolution *
                    noiseScale;

                float sampleY =
                    (float)y / resolution*
                    noiseScale;

                float noiseA =
                    PerlinNoiseUI.Noise(
                        sampleX,
                        sampleY, 
                        customSeed
                    );

                float noiseB =
                    PerlinNoiseUI.Noise(
                        sampleX,
                        sampleY,
                        customSeed + 1000
                    );

                float blend =
                    Mathf.PingPong(shapeTime, 1f);

                float noise =
                    Mathf.Lerp(
                        noiseA,
                        noiseB,
                        blend
                    );

                float noiseAlpha =
                    Mathf.InverseLerp(
                        threshold,
                        1f,
                        noise
                    );

                // Fade toward edges so the cloud doesnt become square
                float nx =
                    ((x / (float)resolution) * 2f - 1f)
                    / cloudWidth;

                float ny =
                    ((y / (float)resolution) * 2f - 1f)
                    / cloudHeight;

                float distance =
                    Mathf.Sqrt(
                        nx * nx + 
                        ny * ny 
                    );

                // Make mask strongest in the middle (of the cloud)
                float centerMask =
                    Mathf.Clamp01(1f - distance);

                centerMask =
                    Mathf.Pow(centerMask, centerFallOff);

                float edgeFade =
                    Mathf.Clamp01(
                        1f - distance
                    );

                // Guarantee more cloud in the center
                float alpha =
                    Mathf.Max(
                        noiseAlpha,
                        centerMask * centerStrength
                    );

                // Makes cloud more/less opaque
                alpha *= opacityStrength;

                edgeFade =
                    Mathf.Pow(
                        edgeFade,
                        edgeFadeStrength
                    );

                alpha *= edgeFade;

                alpha = Mathf.Clamp01(alpha);

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha
                    )
                );
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply(false);

        return texture;
    }
}
