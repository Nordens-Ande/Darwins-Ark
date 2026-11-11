using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICloudSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material cloudMaterial;

    [Header("What To Decorate")]
    [SerializeField] private bool decorateButtons = true;
    [SerializeField] private bool decoratePanels = true;

    [Header("Cloud Fill")]
    [SerializeField] private bool fillInside = true;

    [Tooltip("Higher value = fewer clouds inside the UI element")]
    [SerializeField] private float pixelsPerFillCloud = 100f;

    [SerializeField] private float fillPadding = 10f;

    [Tooltip("Objects containing this word are considered panels.")]
    [SerializeField] private string panelNameContains = "Panel";

    [Header("Panel Detection")]
    [SerializeField]
    private Vector2 minimumPanelSize =
        new Vector2(100f, 60f);

    [Header("Cloud Count")]
    [SerializeField] private int minimumClouds = 3;

    [Tooltip("Higher value = fewer clouds")]
    [SerializeField] private float pixelsPerCloud = 120f;

    [Header("Cloud Size")]
    [SerializeField]
    private Vector2 minCloudSize =
        new Vector2(60f, 40f);

    [SerializeField]
    private Vector2 maxCloudSize =
        new Vector2(120f, 80f);

    [Header("Cloud Placement")]
    [SerializeField] private float edgeOffset = 15f;

    private readonly List<Material> generatedMaterials =
        new List<Material>();

    private readonly List<CloudDecoration> decorations =
        new List<CloudDecoration>();


    private class CloudDecoration
    {
        public RectTransform target;
        public RectTransform sortingTarget;
        public RectTransform cloudGroup;
    }


    private void Start()
    {
        GenerateAllClouds();
    }


    private void LateUpdate()
    {
        foreach (CloudDecoration decoration in decorations)
        {
            if (decoration.target == null ||
                decoration.sortingTarget == null ||
                decoration.cloudGroup == null)
            {
                continue;
            }

            bool shouldBeVisible =
                decoration.target.gameObject.activeInHierarchy &&
                decoration.sortingTarget.gameObject.activeInHierarchy;

            if (decoration.cloudGroup.gameObject.activeSelf != shouldBeVisible)
            {
                decoration.cloudGroup.gameObject.SetActive(
                    shouldBeVisible
                );
            }

            if (!shouldBeVisible)
                continue;

            //UpdateCloudGroupPosition(decoration);
        }
    }


    private void GenerateAllClouds()
    {
        if (decoratePanels)
        {
            SpawnCloudsForPanels();
        }

        if (decorateButtons)
        {
            SpawnCloudsForButtons();
        }
    }


    // -------------------------------------------------------
    // BUTTONS
    // -------------------------------------------------------

    private void SpawnCloudsForButtons()
    {
        Button[] buttons =
            GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            RectTransform target =
                button.GetComponent<RectTransform>();

            if (target == null)
                continue;

            RectTransform sortingTarget =
                FindCorrectSortingTarget(target);

            CreateCloudsForTarget(
                target,
                sortingTarget
            );
        }
    }


    // -------------------------------------------------------
    // PANELS
    // -------------------------------------------------------

    private void SpawnCloudsForPanels()
    {
        Image[] images =
            GetComponentsInChildren<Image>(true);

        foreach (Image image in images)
        {
            // Buttons are handled separately.
            if (image.GetComponent<Button>() != null)
                continue;

            RectTransform target =
                image.rectTransform;

            if (target == null)
                continue;

            // Avoid decorating icons and random Images.
            bool looksLikePanel =
                image.gameObject.name.IndexOf(
                    panelNameContains,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;

            if (!looksLikePanel)
                continue;

            if (target.rect.width < minimumPanelSize.x ||
                target.rect.height < minimumPanelSize.y)
            {
                continue;
            }

            CreateCloudsForTarget(
                target,
                target
            );
        }
    }


    // -------------------------------------------------------
    // FIND WHAT CLOUDS MUST BE BEHIND
    // -------------------------------------------------------

    private RectTransform FindCorrectSortingTarget(
        RectTransform target)
    {
        Transform current =
            target.parent;

        while (current != null &&
               current != transform)
        {
            RectTransform currentRect =
                current as RectTransform;

            Image image =
                current.GetComponent<Image>();

            if (currentRect != null &&
                image != null)
            {
                bool looksLikePanel =
                    current.name.IndexOf(
                        panelNameContains,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0;

                if (looksLikePanel)
                {
                    return currentRect;
                }
            }

            current =
                current.parent;
        }

        // No panel found.
        // Put clouds directly behind button.
        return target;
    }


    // -------------------------------------------------------
    // CREATE CLOUD GROUP
    // -------------------------------------------------------

    private void CreateCloudsForTarget(
    RectTransform target,
    RectTransform sortingTarget)
    {
        if (target == null || sortingTarget == null)
            return;

        RectTransform parent =
            sortingTarget.parent as RectTransform;

        if (parent == null)
            return;

        GameObject cloudGroupObject =
            new GameObject(
                target.name + "_Clouds",
                typeof(RectTransform)
            );

        RectTransform cloudGroup =
            cloudGroupObject.GetComponent<RectTransform>();

        cloudGroup.SetParent(
            parent,
            false
        );

        // ------------------------------------------------
        // PLACE CLOUD GROUP AT TARGET POSITION
        // ------------------------------------------------

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 centerWorld =
            (corners[0] + corners[2]) * 0.5f;

        Vector2 centerLocal;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(
                null,
                centerWorld
            ),
            null,
            out centerLocal
        );

        cloudGroup.anchorMin =
            new Vector2(0.5f, 0.5f);

        cloudGroup.anchorMax =
            new Vector2(0.5f, 0.5f);

        cloudGroup.pivot =
            new Vector2(0.5f, 0.5f);

        cloudGroup.anchoredPosition =
            centerLocal;

        cloudGroup.sizeDelta =
            target.rect.size;

        cloudGroup.localScale =
            Vector3.one;

        cloudGroup.localRotation =
            Quaternion.identity;

        // Clouds behind panel/button
        cloudGroup.SetSiblingIndex(
            sortingTarget.GetSiblingIndex()
        );

        CloudDecoration decoration =
            new CloudDecoration
            {
                target = target,
                sortingTarget = sortingTarget,
                cloudGroup = cloudGroup
            };

        decorations.Add(decoration);

        SpawnClouds(cloudGroup);
    }


    // -------------------------------------------------------
    // CORRECT UI POSITION
    // -------------------------------------------------------

    private void UpdateCloudGroupPosition(
        CloudDecoration decoration)
    {
        RectTransform parent =
            decoration.cloudGroup.parent as RectTransform;

        if (parent == null)
            return;

        Bounds bounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                parent,
                decoration.target
            );

        decoration.cloudGroup.localPosition =
            new Vector3(
                bounds.center.x,
                bounds.center.y,
                0f
            );

        decoration.cloudGroup.sizeDelta =
            new Vector2(
                bounds.size.x,
                bounds.size.y
            );
    }


    // -------------------------------------------------------
    // CLOUD COUNT
    // -------------------------------------------------------

    private void SpawnClouds(
    RectTransform cloudGroup)
    {
        float width =
            cloudGroup.rect.width;

        float height =
            cloudGroup.rect.height;

        // -------------------------
        // EDGE CLOUDS
        // -------------------------

        float perimeter =
            width * 2f +
            height * 2f;

        int edgeCloudCount =
            Mathf.Max(
                minimumClouds,
                Mathf.RoundToInt(
                    perimeter / pixelsPerCloud
                )
            );

        for (int i = 0; i < edgeCloudCount; i++)
        {
            SpawnEdgeCloud(
                cloudGroup,
                i,
                edgeCloudCount
            );
        }

        // -------------------------
        // FILL CLOUDS
        // -------------------------

        if (fillInside)
        {
            float area =
                width * height;

            int fillCloudCount =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        area /
                        (pixelsPerFillCloud *
                         pixelsPerFillCloud)
                    )
                );

            for (int i = 0; i < fillCloudCount; i++)
            {
                SpawnFillCloud(
                    cloudGroup
                );
            }
        }
    }


    // -------------------------------------------------------
    // INDIVIDUAL CLOUD
    // -------------------------------------------------------
    private void SpawnEdgeCloud(
    RectTransform cloudGroup,
    int index,
    int totalClouds)
    {
        RectTransform cloudRect =
            CreateCloud(
                cloudGroup,
                $"EdgeCloud_{index}"
            );

        float t =
            index /
            (float)totalClouds;

        cloudRect.anchoredPosition =
            GetPositionAroundEdge(
                cloudGroup.rect.size,
                t
            );
    }


    // -------------------------------------------------------
    // POSITION AROUND EDGE
    // -------------------------------------------------------

    private Vector2 GetPositionAroundEdge(
        Vector2 size,
        float t)
    {
        float width =
            size.x;

        float height =
            size.y;

        float perimeter =
            width * 2f +
            height * 2f;

        float distance =
            t * perimeter;

        float halfWidth =
            width * 0.5f;

        float halfHeight =
            height * 0.5f;


        // Top
        if (distance < width)
        {
            return new Vector2(
                -halfWidth + distance,
                halfHeight + edgeOffset
            );
        }

        distance -= width;


        // Right
        if (distance < height)
        {
            return new Vector2(
                halfWidth + edgeOffset,
                halfHeight - distance
            );
        }

        distance -= height;


        // Bottom
        if (distance < width)
        {
            return new Vector2(
                halfWidth - distance,
                -halfHeight - edgeOffset
            );
        }

        distance -= width;


        // Left
        return new Vector2(
            -halfWidth - edgeOffset,
            -halfHeight + distance
        );
    }

    private RectTransform CreateCloud(
    RectTransform cloudGroup,
    string cloudName)
    {
        GameObject cloud =
            new GameObject(
                cloudName,
                typeof(RectTransform),
                typeof(RawImage)
            );

        RectTransform cloudRect =
            cloud.GetComponent<RectTransform>();

        cloudRect.SetParent(
            cloudGroup,
            false
        );

        cloudRect.anchorMin =
            new Vector2(0.5f, 0.5f);

        cloudRect.anchorMax =
            new Vector2(0.5f, 0.5f);

        cloudRect.pivot =
            new Vector2(0.5f, 0.5f);

        RawImage image =
            cloud.GetComponent<RawImage>();

        image.raycastTarget = false;
        image.color = Color.white;

        Material instance =
            new Material(cloudMaterial);

        generatedMaterials.Add(instance);

        image.material =
            instance;

        instance.SetFloat(
            "_Seed",
            UnityEngine.Random.Range(
                0f,
                1000f
            )
        );

        instance.SetColor(
            "_CloudColor",
            Color.white
        );

        float width =
            UnityEngine.Random.Range(
                minCloudSize.x,
                maxCloudSize.x
            );

        float height =
            UnityEngine.Random.Range(
                minCloudSize.y,
                maxCloudSize.y
            );

        cloudRect.sizeDelta =
            new Vector2(
                width,
                height
            );

        return cloudRect;
    }

    private void SpawnFillCloud(
    RectTransform cloudGroup)
    {
        RectTransform cloudRect =
            CreateCloud(
                cloudGroup,
                "FillCloud"
            );

        float halfWidth =
            cloudGroup.rect.width * 0.5f;

        float halfHeight =
            cloudGroup.rect.height * 0.5f;

        float x =
            UnityEngine.Random.Range(
                -halfWidth + fillPadding,
                halfWidth - fillPadding
            );

        float y =
            UnityEngine.Random.Range(
                -halfHeight + fillPadding,
                halfHeight - fillPadding
            );

        cloudRect.anchoredPosition =
            new Vector2(x, y);
    }

    private void OnDestroy()
    {
        foreach (Material material
                 in generatedMaterials)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
    }

}
