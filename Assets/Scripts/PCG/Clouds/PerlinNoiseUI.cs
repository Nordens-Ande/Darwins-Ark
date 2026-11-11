using UnityEngine;

// TASK 2 PERLIN NOISE - LOVISA

public static class PerlinNoiseUI 
{
    private const int hashXMultiplier = 374761393;
    private const int hashYMultiplier = 668265263;
    private const int hashSeedMultiplier = 1442695041;
    private const int hashMixMultiplier = 1274126177;

    private const int hashShift1 = 13;
    private const int hashShift2 = 16;
    private const int angleMask = 1023;

    private static float Fade(float t)
    {
        // Perlin fade curve:
        // 6t^5 - 15t^4 + 10^3
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + t * (b - a);
    }

    private static readonly Vector2[] Gradients =
    {
        new Vector2(1f, 0f),
        new Vector2(-1f, 0f),
        new Vector2(0f, 1f),
        new Vector2(0f, -1f),

        new Vector2(0.7071f, 0.7071f),
        new Vector2(-0.7071f, 0.7071f),
        new Vector2(0.7071f, -0.7071f),
        new Vector2(-0.7071f, -0.7071f)
    };

    private static Vector2 Gradient(int x, int y, int seed)
    {
        int hash =
            x * hashXMultiplier +
            y * hashYMultiplier +
            seed * hashSeedMultiplier;

        hash = (hash ^ (hash >> hashShift1)) * hashMixMultiplier;
        hash ^= hash >> hashShift2;

        int index =
            Mathf.Abs(hash) % Gradients.Length;

        return Gradients[index];
    }

    public static float Noise(float x, float y, int seed)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);

        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float localX = x - x0;
        float localY = y - y0;

        Vector2 g00 = Gradient(x0, y0, seed);
        Vector2 g10 = Gradient(x1, y0, seed);
        Vector2 g01 = Gradient(x0, y1, seed);
        Vector2 g11 = Gradient(x1, y1, seed);

        float d00 = Vector2.Dot(
            g00,
            new Vector2(localX, localY)
        );

        float d10 = Vector2.Dot(
            g10,
            new Vector2(localX - 1, localY)
        );

        float d01 = Vector2.Dot(
            g01,
            new Vector2(localX, localY - 1)
        );

        float d11 = Vector2.Dot(
            g11,
            new Vector2(localX - 1, localY - 1)
        );

        float u = Fade(localX);
        float v = Fade(localY);

        float bottom =
            Lerp(d00, d10, u);

        float top =
            Lerp(d01, d11, u);

        float value =
            Lerp(bottom, top, v);

        return value * 0.5f + 0.5f;
    }
}
