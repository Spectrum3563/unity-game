using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

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

    private GameObject selectedTowerPrefab;

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        // Start with Ranged Tower selected.
        selectedTowerPrefab = rangedTowerPrefab;
    }

    private void Update()
    {
        HandleTowerSelection();

        if (selectedTowerPrefab == null)
            return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        mouseWorld.z = 0f;

        Vector3Int cell = grid.WorldToCell(mouseWorld);

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceTower(cell);
        }
    }

    private void HandleTowerSelection()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            selectedTowerPrefab = rangedTowerPrefab;
            Debug.Log("Selected Ranged Tower");
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            selectedTowerPrefab = slowTowerPrefab;
            Debug.Log("Selected Slow Tower");
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            selectedTowerPrefab = dotTowerPrefab;
            Debug.Log("Selected DOT Tower");
        }
    }

    private void TryPlaceTower(Vector3Int cell)
    {
        if (occupiedCells.Contains(cell))
            return;

        if (selectedTowerPrefab == null)
            return;

        TowerBase towerBase =
            selectedTowerPrefab.GetComponent<TowerBase>();

        if (towerBase == null)
        {
            Debug.LogError(
                "Selected tower prefab does not have a TowerBase component."
            );
            return;
        }

        TilePlacementType placementType = GetPlacementType(cell);

        if (!towerBase.IsPlacementAllowed(placementType))
            return;

        Vector3 position = grid.GetCellCenterWorld(cell);

        GameObject towerObject = Instantiate(
            selectedTowerPrefab,
            position,
            Quaternion.identity
        );

        TowerBase placedTower =
            towerObject.GetComponent<TowerBase>();

        placedTower.occupiedCell =
            new Vector2Int(cell.x, cell.y);

        occupiedCells.Add(cell);
    }

    private TilePlacementType GetPlacementType(Vector3Int cell)
    {
        bool wallExists =
            wallTilemap != null &&
            wallTilemap.HasTile(cell);

        bool pathExists =
            pathTilemap != null &&
            pathTilemap.HasTile(cell);

        if (wallExists && !pathExists)
            return TilePlacementType.Wall;

        if (pathExists)
            return TilePlacementType.Path;

        return TilePlacementType.Invalid;
    }
}