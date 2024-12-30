using System.Collections.Generic;
using Kutie.Extensions;
using UnityEngine;

public class RandomUsername : MonoBehaviour
{
    [SerializeField] List<string> adjectives;
    [SerializeField] List<string> nouns;
    [SerializeField] TMPro.TMP_InputField inputField;

    void Awake(){
        var adjective = adjectives.Sample();
        var noun = nouns.Sample();

        inputField.text = $"{adjective} {noun}";
    }
}
