using UnityEngine;

public class SpawnSnow : MonoBehaviour
{
    [SerializeField] public int Length = 50;
    [SerializeField] public Grid Grid;
    [SerializeField] GameObject snowPrefab;

    void Start()
    {
        for (int i = 0; i < Length; i++)
        {
            for (int j = 0; j < Length; j++)
            {
                GameObject snow = Instantiate(snowPrefab);
                snow.transform.parent = transform;
                snow.transform.localPosition = Grid.CellToWorld(new Vector3Int(i, 0, j));
                snow.transform.rotation = Quaternion.identity;
                snow.transform.localScale = Grid.cellSize;
            }
        }
    }
}
