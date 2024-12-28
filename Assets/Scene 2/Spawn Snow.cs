using UnityEngine;

public class SpawnSnow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject prefab;

    void Start()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                GameObject snow = Instantiate(prefab);
                snow.transform.parent = transform;
                snow.transform.localPosition = new Vector3(i * 0.1f, 0, j * 0.1f);
                snow.transform.rotation = Quaternion.identity;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
