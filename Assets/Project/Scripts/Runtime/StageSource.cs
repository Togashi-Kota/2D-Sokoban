using UnityEngine;
using UnityEngine.Tilemaps;

public class StageSource : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public Tilemap goalTilemap;

    public Tilemap objectTilemap;

    [Header("Tile")]
    public TileBase playerTile;
    public TileBase boxTile;

    [Header("Camera")]
    public GameObject targetCamera;

    [Header("【出力先】StageData")]
    public StageData outputStageData;

    /* テスト用 */
    [Header("【！】以下の項目はテスト用です")]
    public string stageName;

    private void Awake()
    {
        Destroy(this.gameObject);
    }
}

