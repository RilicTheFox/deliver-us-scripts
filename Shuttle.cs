using UnityEngine;

/// <summary>
/// This class is for ferrying data between days during gameplay.
/// It is unfortunately not an actual shuttle.
/// </summary>
public class Shuttle : MonoBehaviour
{
    public static Shuttle Instance;

    public float stress = -1;
    public int previousDay = -1;
    public int money;
    public Ending.Types endingType;

    private void Awake()
    {
        // If Instance isn't null and isn't me, destroy myself.
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this);
    }

    public static void CreateShuttle()
    {
        GameObject shuttleGameObject = new("Shuttle");
        shuttleGameObject.AddComponent<Shuttle>();
        Instantiate(shuttleGameObject);
    }
}
