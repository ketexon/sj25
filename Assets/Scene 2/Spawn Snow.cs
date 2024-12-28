using UnityEngine;

public class SpawnSnow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject prefab;

    void Start()
    {
        for (var i = 0; i < 10; i++)
        {
            GameObject snow = Instantiate(prefab);
            snow.transform.parent = transform;
            snow.transform.localPosition = new Vector3(i * 0.1f, 0, 0);
            snow.transform.rotation = Quaternion.identity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
