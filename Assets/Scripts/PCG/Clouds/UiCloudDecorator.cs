using UnityEngine;
using UnityEngine.UI;

public class UiCloudDecorator : MonoBehaviour
{
    [SerializeField] private PerlinCloudGenerator cloudGenerator;

    [Header("Cloud")]
    [SerializeField] private Vector2 extraSize = new Vector2(80f, 50f);
    [SerializeField] private float morphSpeed = 0.15f;
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Cloud Position")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Header("Generation")]
    [SerializeField] private int seed = 100;

    [Header("Rendering")]
    [SerializeField] private bool placeBehindElement = true;

    private float shapeTime;
    private float updateTimer;

    private RawImage cloudImage;

    private void Start()
    {
        CreateCloud();
    }

    private void Update()
    {
        shapeTime +=
            Time.deltaTime * morphSpeed;

        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0;
            UpdateCloud();
        }
    }

    private void UpdateCloud()
    {
        cloudImage.texture =
            cloudGenerator.GenerateCloud(
                shapeTime,
                seed
            );
    }

    private void CreateCloud()
    {
        RectTransform targetRect = GetComponent<RectTransform>();

        GameObject cloudObject =
            new GameObject("GeneratedCloud");

        cloudObject.transform.SetParent(
            transform.parent,
            false 
        );

        cloudImage =
            cloudObject.AddComponent<RawImage>();

        RectTransform cloudRect =
            cloudObject.GetComponent<RectTransform>();

        cloudRect.anchorMin = targetRect.anchorMin;

        cloudRect.anchorMax = targetRect.anchorMax;

        cloudRect.pivot = targetRect.pivot;

        cloudRect.anchoredPosition = 
            targetRect.anchoredPosition + offset;

        cloudRect.sizeDelta = 
            targetRect.sizeDelta + extraSize;

        cloudObject.transform.SetSiblingIndex(
            transform.GetSiblingIndex()
        );

        UpdateCloud();
    }
}
