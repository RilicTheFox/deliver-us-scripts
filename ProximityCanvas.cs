using UnityEngine;

public class ProximityCanvas : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    private void Update()
    {
        transform.rotation = Quaternion.identity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerCharacter>() == null)
            return;

        canvas.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<PlayerCharacter>() == null)
            return;

        canvas.enabled = false;
    }
}
