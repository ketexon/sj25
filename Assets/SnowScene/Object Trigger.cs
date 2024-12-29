using UnityEngine;

public class ObjectTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the other collider has the "Object2" tag
        if (other.CompareTag("Object"))
        {
            // Make Object #1 disappear by destroying it
            Destroy(gameObject);  // Destroys this object (Object #1)
        }
    }
}

//using UnityEngine;

//public class ObjectTrigger : MonoBehaviour
//{
//    [SerializeField] private float scaleFactor = 1.1f;  // Factor by which the object will enlarge

//    private void OnTriggerEnter(Collider other)
//    {
//        // Check if the other object is the rolling object (tagged as "Player")
//        if (other.CompareTag("Object"))
//        {
//            // Get the ObjectRoll script from the rolling object
//            ObjectRoll rollScript = other.GetComponent<ObjectRoll>();

//            // If the ObjectRoll script is found, enlarge the object
//            if (rollScript != null)
//            {
//                // Increase the size of the object by the scaleFactor
//                other.transform.localScale *= scaleFactor;
//            }

//            // Destroy the snow cube (the object this script is attached to)
//            Destroy(gameObject);
//        }
//    }
//}
