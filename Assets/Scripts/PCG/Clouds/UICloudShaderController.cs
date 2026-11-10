using UnityEngine;
using UnityEngine.UI;

public class UICloudShaderController : MonoBehaviour
{
    [SerializeField] private float seed;

    private Material cloudMaterial;

    private void Awake()
    {
        Graphic graphic = GetComponent<Graphic>();

        cloudMaterial =
            new Material(graphic.material);

        graphic.material = cloudMaterial;

        cloudMaterial.SetFloat(
            "_Seed",
            seed
        );
    }

    private void OnDestroy()
    {
        if (cloudMaterial != null)
        {
            Destroy(cloudMaterial);
        }
    }
}
