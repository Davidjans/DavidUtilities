using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ExampleData", menuName = "Example/ExampleData")]
public class ExampleData : ScriptableObject
{
    public int exampleInt;
    public string exampleString;
    public bool exampleBool;
    public float exampleFloat;
    public Vector2 exampleVector2;
    public GameObject examplePrefab;
    public AudioClip exampleAudioCLip;
    public List<ExampleData2> exampleData2 = new List<ExampleData2>();
}


public class ExampleData2
{
    public string text;
    public string texttwo;
    public int numberone; 
    public int numberTwo ; 
}