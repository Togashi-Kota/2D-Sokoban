using UnityEngine;

public class Box : MonoBehaviour
{
    public Vector2Int gridPos;

    private void Start()
    {
        UpdatePosition();
    }

    public void Move(Vector2Int newPos)
    {
        gridPos = newPos;
        UpdatePosition();
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        gridPos = newPos;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position = new Vector3(gridPos.x, gridPos.y, 0);
    }
}
