using UnityEngine;

public class ExampleScene : MonoBehaviour
{
    private static ExampleScene _instance;
    public static ExampleScene instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<ExampleScene>();
            return _instance;
        }
        private set => _instance = value;
    }

    public ExampleData data;
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
}
