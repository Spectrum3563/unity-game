using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerPlacementController : MonoBehaviour
{
    public Grid grid;
    public Tilemap wallTilemap;
    public Tilemap pathTilemap;
    public TowerBase selectedTowerPrefab;
    public TowerBase ghostTower;
    public Color validColor = new Color(0.25f, 1f, 0.35f, 0.3f);
    public Color invalidColor = new Color(1f, 0.2f, 0.2f, 0.3f);

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (selectedTowerPrefab == null)
        {
            return;
        }

        if (ghostTower == null)
        {
            ghostTower = Instantiate(selectedTowerPrefab);
            ghostTower.enabled = false;
            ghostTower.transform.SetParent(transform);
            var sr = ghostTower.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                var color = sr.color;
                color.a = 0.3f;
                sr.color = color;
            }
        }

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector3Int cell = grid.WorldToCell(mouseWorld);
        Vector3 cellCenter = grid.GetCellCenterWorld(cell);
        ghostTower.transform.position = cellCenter;

        TilePlacementType placementType = GetPlacementType(cell);
        bool allowed = selectedTowerPrefab.IsPlacementAllowed(placementType) && !IsCellOccupied(cell);

        var renderer = ghostTower.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = allowed ? validColor : invalidColor;
        }

        if (Input.GetMouseButtonDown(0) && allowed)
        {
            PlaceTower(cell);
        }
    }

    private TilePlacementType GetPlacementType(Vector3Int cell)
    {
        bool wallExists = wallTilemap != null && wallTilemap.HasTile(cell);
        bool pathExists = pathTilemap != null && pathTilemap.HasTile(cell);

        if (wallExists && !pathExists)
        {
            return TilePlacementType.Wall;
        }

        if (pathExists)
        {
            return TilePlacementType.Path;
        }

        return TilePlacementType.Invalid;
    }

    private void PlaceTower(Vector3Int cell)
    {
        TowerBase tower = Instantiate(selectedTowerPrefab);
        tower.transform.position = grid.GetCellCenterWorld(cell);
        tower.occupiedCell = new Vector2Int(cell.x, cell.y);
        tower.transform.SetParent(GameObject.Find("Towers")?.transform ?? transform);
    }

    private bool IsCellOccupied(Vector3Int cell)
    {
        TowerBase[] towers = FindObjectsByType<TowerBase>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (TowerBase tower in towers)
        {
            if (tower.occupiedCell == new Vector2Int(cell.x, cell.y))
            {
                return true;
            }
        }

        return false;
    }
}
