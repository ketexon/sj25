using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Snowball : MonoBehaviour
{
    const float RADIUS_PER_SNOW = 0.001f;
    const float MASS_PER_SNOW = 0.001f;
    [System.NonSerialized] public List<Scrap> Scrap = new();
    [SerializeField] Transform snowSphere;

    public UnityEvent SnowCollectedEvent;

    float _radius = 0;
    public float Radius {
        get => _radius;
        set {
            _radius = value;
            snowSphere.localScale = Vector3.one * Radius;
        }
    }
    [System.NonSerialized] public float Mass = 0;

    List<GameObject> coreScrap = new();

    bool snowballShown = false;
    bool started = false;

    void Awake(){
        CalculateCoreStats();
    }

    // Spawn core
    void Start(){
        SpawnCore();
        Debug.Log($"Snowball spawned with radius {Radius}");

        // Scale snowSphere to match core
        snowSphere.localScale = Vector3.one * Radius;
        // Hide snow sphere
        snowSphere.gameObject.SetActive(false);

        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);
    }

    void OnGameStart(){
        started = true;
    }

    void OnTriggerEnter(Collider other) {
        if(!started) return;
        if(other.CompareTag("Snow")){
            CollectSnow();
            Destroy(other.gameObject);
        }
    }

    public void CollectSnow(){
        if(!snowballShown){
            snowSphere.gameObject.SetActive(true);
            snowballShown = true;
            // delete all core scrap
            foreach(var scrap in coreScrap){
                Destroy(scrap);
            }
            coreScrap.Clear();
        }
        Radius += RADIUS_PER_SNOW;
        Mass += MASS_PER_SNOW;
        SnowCollectedEvent.Invoke();
    }

    void CalculateCoreStats(){
        float maxLength = 0;
        float lengthSum = 0;
        foreach(var scrap in Scrap){
            maxLength = Mathf.Max(maxLength, scrap.Length);
            lengthSum += scrap.Length;
            Mass += scrap.Mass;
        }
        Radius = maxLength + Mathf.Pow(lengthSum / 2, 1f/3);
    }

    Vector3 SampleUnitSphere(){
        var phi = Random.Range(0f,2f*Mathf.PI);
        var costheta = Random.Range(-1f,1f);
        var u = Random.Range(0f,1f);

        var theta = Mathf.Acos(costheta);
        var r = Mathf.Pow(u, 1f/3);

        var x = r * Mathf.Sin(theta) * Mathf.Cos(phi);
        var y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
        var z = r * Mathf.Cos(theta);

        return new Vector3(x, y, z);
    }

    Vector3 SampleSphere(float radius){
        return SampleUnitSphere() * radius;
    }

    void SpawnCore(){
        foreach(var scrap in Scrap){
            var randomPoint = SampleSphere(Radius / 2);
            var randomDir = SampleSphere(1);
            var randomRot = Quaternion.LookRotation(randomDir, Vector3.up);
            var scrapGO = Instantiate(
                scrap.Prefab,
                transform
            );

            scrapGO.transform.localPosition = randomPoint + new Vector3(
                0,
                Radius / 2,
                Radius / 2
            );
            scrapGO.transform.localRotation = randomRot;

            coreScrap.Add(scrapGO);
        }
    }
}
