using UnityEngine;
using UnityEngine.Tilemaps;

public class StageView : MonoBehaviour
{
    [Header("Runtime Tilemaps")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Tilemap wallTilemap;
    [SerializeField] private Tilemap goalTilemap;

    [Header("Tiles")]
    [SerializeField] private TileBase floorTile;
    [SerializeField] private TileBase wallTile;
    [SerializeField] private TileBase goalTile;


    public void Build(StageData stageData)
    {
        if (stageData == null)
        {
            Debug.LogWarning("StageView.Build に null が渡されました。");
            return;
        }

        if (floorTilemap == null || wallTilemap == null || goalTilemap == null)
        {
            Debug.LogWarning("StageView の Tilemap 参照が不足しています。");
            return;
        }

        ClearAll();

        for (int y = 0; y < stageData.height; y++)
        {
            for (int x = 0; x < stageData.width; x++)
            {
                int index = x + y * stageData.width;
                TileType tileType = stageData.tiles[index];
                Vector3Int pos = new Vector3Int(x, y, 0);

                switch (tileType)
                {
                    case TileType.Floor:
                        floorTilemap.SetTile(pos, floorTile);
                        break;

                    case TileType.Wall:
                        wallTilemap.SetTile(pos, wallTile);
                        break;

                    case TileType.Goal:
                        goalTilemap.SetTile(pos, goalTile);
                        break;
                }
            }
        }
    }

    public void ClearAll()
    {
        if (floorTilemap != null) floorTilemap.ClearAllTiles();
        if (wallTilemap != null) wallTilemap.ClearAllTiles();
        if (goalTilemap != null) goalTilemap.ClearAllTiles();
    }
}
