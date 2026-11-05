using UnityEngine;
using UnityEngine.UI;

public class CloudPreview : MonoBehaviour
{
    [SerializeField] private PerlinCloudGenerator generator;
    [SerializeField] private RawImage rawImage;
    [SerializeField] private int seed = 123;
   
    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Start()
    {
        rawImage.texture = generator.GenerateCloud(0f, seed);
    }
}
