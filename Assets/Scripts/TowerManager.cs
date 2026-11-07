using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerManager : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private Grid grid;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap pathTilemap;

    [Header("Tower Prefabs")]
    [SerializeField] private GameObject rangedTowerPrefab;
    [SerializeField] private GameObject slowTowerPrefab;
    [SerializeField] private GameObject dotTowerPrefab;

    [Header("UI and Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Canvas overlayCanvas;

    [Header("Placement Preview")]
    [SerializeField, Range(0.1f, 0.2f)] private float previewOpacity = 0.15f;

    [Header("Placement Costs")]
    [SerializeField] private int rangedPlacementCost = 20;
    [SerializeField] private int slowPlacementCost = 100;
    [SerializeField] private int dotPlacementCost = 80;

    [Header("Upgrade Costs")]
    [SerializeField] private int rangedLevel2Cost = 50;
    [SerializeField] private int rangedLevel3Cost = 100;
    [SerializeField] private int slowLevel2Cost = 120;
    [SerializeField] private int slowLevel3Cost = 200;
    [SerializeField] private int dotLevel2Cost = 120;
    [SerializeField] private int dotLevel3Cost = 180;

    private readonly Dictionary<Vector3Int, TowerBase> towersByCell =
        new Dictionary<Vector3Int, TowerBase>();

    private Camera mainCamera;
    private GameObject selectedTowerPrefab;
    private TowerBase selectedTowerBase;
    private TowerBase selectedUpgradeTower;
    private GameObject previewObject;
    private SpriteRenderer[] previewRenderers;
    private Color[] previewOriginalColors;
    private TMP_Text actionPriceText;
    private RectTransform actionPriceRect;
    private TMP_Text actionLevelText;
    private RectTransform actionLevelRect;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (grid == null || playerHealth == null || overlayCanvas == null)
        {
            Debug.LogError(
                "TowerManager requires a Grid, PlayerHealth, and UI Canvas reference.",
                this
            );
            enabled = false;
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("TowerManager could not find the Main Camera.", this);
            enabled = false;
            return;
        }

        RectTransform canvasRect = overlayCanvas.transform as RectTransform;
        if (canvasRect == null)
        {
            Debug.LogError("TowerManager's Canvas has no RectTransform.", this);
            enabled = false;
            return;
        }

        TMP_Text fontTemplate = overlayCanvas.GetComponentInChildren<TMP_Text>(
            true
        );
        if (fontTemplate == null || fontTemplate.font == null)
        {
            Debug.LogError(
                "TowerManager requires a TextMeshPro text with a font under its Canvas.",
                this
            );
            enabled = false;
            return;
        }

        CreateActionLabels(fontTemplate);
        CacheExistingTowers();
        SelectBuildTower(rangedTowerPrefab);
    }

    private void Update()
    {
        if (!StartMenuUI.CanPlaceTowers)
        {
            if (previewObject != null)
                previewObject.SetActive(false);

            actionPriceText.gameObject.SetActive(false);
            actionLevelText.gameObject.SetActive(false);
            return;
        }

        HandleTowerSelection();

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Vector2 mousePosition = mouse.position.ReadValue();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mousePosition);
        mouseWorld.z = 0f;
        Vector3Int cell = grid.WorldToCell(mouseWorld);

        if (selectedUpgradeTower != null)
        {
            UpdateUpgradePrompt();
        }
        else if (selectedTowerBase != null)
        {
            UpdatePlacementPreview(cell);
        }
        else
        {
            actionPriceText.gameObject.SetActive(false);
            actionLevelText.gameObject.SetActive(false);
        }

        if (mouse.leftButton.wasPressedThisFrame)
            HandleCellClick(cell);
    }

    private void OnDestroy()
    {
        if (previewObject != null)
            Destroy(previewObject);
    }

    private void CreateActionLabels(TMP_Text fontTemplate)
    {
        actionPriceText = CreateActionLabel(
            "Tower Action Price",
            fontTemplate,
            22f,
            new Vector2(110f, 56f),
            out actionPriceRect
        );
        actionLevelText = CreateActionLabel(
            "Tower Action Level",
            fontTemplate,
            18f,
            new Vector2(80f, 32f),
            out actionLevelRect
        );
        actionPriceText.gameObject.SetActive(false);
        actionLevelText.gameObject.SetActive(false);
    }

    private TextMeshProUGUI CreateActionLabel(
        string objectName,
        TMP_Text fontTemplate,
        float fontSize,
        Vector2 size,
        out RectTransform labelRect
    )
    {
        GameObject labelObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );
        labelObject.transform.SetParent(overlayCanvas.transform, false);

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = fontTemplate.font;
        label.fontSharedMaterial = fontTemplate.fontSharedMaterial;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.raycastTarget = false;
        label.text = string.Empty;

        labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.zero;
        labelRect.pivot = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = size;
        return label;
    }

    private void CacheExistingTowers()
    {
        TowerBase[] existingTowers = FindObjectsByType<TowerBase>(
            FindObjectsInactive.Exclude
        );

        foreach (TowerBase tower in existingTowers)
        {
            Vector3Int cell = grid.WorldToCell(tower.transform.position);
            towersByCell[cell] = tower;
        }
    }

    private void HandleTowerSelection()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.digit0Key.wasPressedThisFrame)
            SelectBuildTower(null);
        else if (keyboard.digit1Key.wasPressedThisFrame)
            SelectBuildTower(rangedTowerPrefab);
        else if (keyboard.digit2Key.wasPressedThisFrame)
            SelectBuildTower(slowTowerPrefab);
        else if (keyboard.digit3Key.wasPressedThisFrame)
            SelectBuildTower(dotTowerPrefab);
    }

    private void SelectBuildTower(GameObject towerPrefab)
    {
        selectedUpgradeTower = null;
        selectedTowerPrefab = towerPrefab;
        selectedTowerBase = selectedTowerPrefab != null
            ? selectedTowerPrefab.GetComponent<TowerBase>()
            : null;

        if (selectedTowerPrefab != null && selectedTowerBase == null)
        {
            Debug.LogError(
                "Selected tower prefab does not have a TowerBase component.",
                selectedTowerPrefab
            );
            selectedTowerPrefab = null;
        }

        DestroyPreview();
        actionPriceText?.gameObject.SetActive(false);
        actionLevelText?.gameObject.SetActive(false);
    }

    private void UpdatePlacementPreview(Vector3Int cell)
    {
        if (!EnsurePreview())
            return;

        bool canPlace = CanPlaceAt(cell, out int cost);
        previewObject.SetActive(true);
        previewObject.transform.position = grid.GetCellCenterWorld(cell);

        Color previewColor = canPlace
            ? new Color(1f, 1f, 1f, previewOpacity)
            : new Color(1f, 0.3f, 0.3f, previewOpacity);

        for (int i = 0; i < previewRenderers.Length; i++)
        {
            Color originalColor = previewOriginalColors[i];
            previewRenderers[i].color = new Color(
                previewColor.r * originalColor.r,
                previewColor.g * originalColor.g,
                previewColor.b * originalColor.b,
                previewOpacity * originalColor.a
            );
        }

        actionPriceText.text = cost.ToString();
        actionPriceText.color = canPlace ? Color.white : Color.red;
        actionLevelText.text = "Lv" + selectedTowerBase.level;
        actionLevelText.color = Color.white;
        PositionCellLabel(cell, new Vector2(0.35f, 0.35f), actionPriceText, actionPriceRect);
        PositionCellLabel(cell, new Vector2(-0.35f, -0.35f), actionLevelText, actionLevelRect);
        actionPriceText.gameObject.SetActive(true);
        actionLevelText.gameObject.SetActive(true);
    }

    private bool EnsurePreview()
    {
        if (previewObject != null)
            return true;

        if (selectedTowerPrefab == null || selectedTowerBase == null)
            return false;

        previewObject = Instantiate(selectedTowerPrefab);
        previewObject.name = selectedTowerPrefab.name + " Preview";

        TowerBase previewTowerBase = previewObject.GetComponent<TowerBase>();
        if (previewTowerBase != null)
            previewTowerBase.SetPlacementPreviewMode();

        Collider2D[] colliders = previewObject.GetComponentsInChildren<Collider2D>(
            true
        );
        foreach (Collider2D towerCollider in colliders)
            towerCollider.enabled = false;

        previewRenderers = previewObject.GetComponentsInChildren<SpriteRenderer>(
            true
        );
        if (previewRenderers.Length == 0)
        {
            Debug.LogError(
                "Selected tower prefab has no SpriteRenderer for its placement preview.",
                selectedTowerPrefab
            );
            DestroyPreview();
            return false;
        }

        previewOriginalColors = new Color[previewRenderers.Length];
        for (int i = 0; i < previewRenderers.Length; i++)
            previewOriginalColors[i] = previewRenderers[i].color;

        return true;
    }

    private void DestroyPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);

        previewObject = null;
        previewRenderers = null;
        previewOriginalColors = null;
    }

    private void UpdateUpgradePrompt()
    {
        if (selectedUpgradeTower == null)
        {
            actionPriceText.gameObject.SetActive(false);
            actionLevelText.gameObject.SetActive(false);
            return;
        }

        Vector3Int cell = grid.WorldToCell(
            selectedUpgradeTower.transform.position
        );

        if (selectedUpgradeTower.level >= 3)
        {
            actionPriceText.text = "MAX";
            actionPriceText.color = Color.white;
        }
        else
        {
            int cost = GetUpgradeCost(selectedUpgradeTower);
            actionPriceText.text = "\u25B2\n" + cost;
            actionPriceText.color = playerHealth.Score >= cost
                ? Color.white
                : Color.red;
        }

        actionLevelText.text = "Lv" + selectedUpgradeTower.level;
        actionLevelText.color = Color.white;
        PositionCellLabel(cell, new Vector2(0.35f, 0.35f), actionPriceText, actionPriceRect);
        PositionCellLabel(cell, new Vector2(-0.35f, -0.35f), actionLevelText, actionLevelRect);
        actionPriceText.gameObject.SetActive(true);
        actionLevelText.gameObject.SetActive(true);
    }

    private void PositionCellLabel(
        Vector3Int cell,
        Vector2 cellOffset,
        TMP_Text labelText,
        RectTransform labelRect
    )
    {
        Vector3 worldPosition = grid.GetCellCenterWorld(cell);
        worldPosition += grid.transform.TransformVector(
            new Vector3(
                grid.cellSize.x * cellOffset.x,
                grid.cellSize.y * cellOffset.y,
                0f
            )
        );

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z < 0f)
        {
            labelText.gameObject.SetActive(false);
            return;
        }

        RectTransform canvasRect = (RectTransform)overlayCanvas.transform;
        Camera canvasCamera = overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : overlayCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvasCamera,
                out Vector2 localPosition
            ))
        {
            labelText.gameObject.SetActive(false);
            return;
        }

        labelRect.anchoredPosition = localPosition - canvasRect.rect.min;
    }

    private void HandleCellClick(Vector3Int cell)
    {
        if (towersByCell.TryGetValue(cell, out TowerBase tower) && tower != null)
        {
            if (selectedUpgradeTower == tower)
                TryUpgradeTower(tower);
            else
                selectedUpgradeTower = tower;

            if (previewObject != null)
                previewObject.SetActive(false);

            UpdateUpgradePrompt();
            return;
        }

        selectedUpgradeTower = null;

        if (selectedTowerBase != null)
            TryPlaceTower(cell);
    }

    private bool CanPlaceAt(Vector3Int cell, out int cost)
    {
        cost = selectedTowerBase != null
            ? GetPlacementCost(selectedTowerBase.towerType)
            : 0;

        if (selectedTowerBase == null ||
            towersByCell.ContainsKey(cell) ||
            !selectedTowerBase.IsPlacementAllowed(GetPlacementType(cell)) ||
            playerHealth.Score < cost)
        {
            return false;
        }

        return true;
    }

    private void TryPlaceTower(Vector3Int cell)
    {
        if (!CanPlaceAt(cell, out int cost))
            return;

        if (!playerHealth.TrySpendScore(cost))
            return;

        GameObject towerObject = Instantiate(
            selectedTowerPrefab,
            grid.GetCellCenterWorld(cell),
            Quaternion.identity
        );
        TowerBase placedTower = towerObject.GetComponent<TowerBase>();

        if (placedTower == null)
        {
            playerHealth.AddScore(cost);
            Destroy(towerObject);
            Debug.LogError(
                "Placed tower prefab does not have a TowerBase component.",
                selectedTowerPrefab
            );
            return;
        }

        placedTower.occupiedCell = new Vector2Int(cell.x, cell.y);
        towersByCell.Add(cell, placedTower);
    }

    private void TryUpgradeTower(TowerBase tower)
    {
        int cost = GetUpgradeCost(tower);
        if (cost <= 0 || !playerHealth.TrySpendScore(cost))
            return;

        tower.LevelUp();
        UpdateUpgradePrompt();
    }

    private int GetPlacementCost(TowerBase.TowerType towerType)
    {
        switch (towerType)
        {
            case TowerBase.TowerType.Ranged:
                return rangedPlacementCost;

            case TowerBase.TowerType.Slow:
                return slowPlacementCost;

            case TowerBase.TowerType.Dot:
                return dotPlacementCost;

            default:
                return 0;
        }
    }

    private int GetUpgradeCost(TowerBase tower)
    {
        if (tower.level < 1 || tower.level >= 3)
            return 0;

        switch (tower.towerType)
        {
            case TowerBase.TowerType.Ranged:
                return tower.level == 1 ? rangedLevel2Cost : rangedLevel3Cost;

            case TowerBase.TowerType.Slow:
                return tower.level == 1 ? slowLevel2Cost : slowLevel3Cost;

            case TowerBase.TowerType.Dot:
                return tower.level == 1 ? dotLevel2Cost : dotLevel3Cost;

            default:
                return 0;
        }
    }

    private TilePlacementType GetPlacementType(Vector3Int cell)
    {
        bool wallExists = wallTilemap != null && wallTilemap.HasTile(cell);
        bool pathExists = pathTilemap != null && pathTilemap.HasTile(cell);

        if (wallExists && !pathExists)
            return TilePlacementType.Wall;

        if (pathExists)
            return TilePlacementType.Path;

        return TilePlacementType.Invalid;
    }
}
