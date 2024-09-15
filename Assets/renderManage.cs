using UnityEngine;

public class EnableRenderers : MonoBehaviour
{
    void Start()
    {
        // Find all GameObjects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Loop through each GameObject
        foreach (GameObject obj in allObjects)
        {
            // Check if the GameObject has a Renderer component (2D or 3D)
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Enable the renderer if it's disabled
                if (!renderer.enabled)
                {
                    Debug.Log("Enabling renderer for: " + obj.name);
                    renderer.enabled = true;
                }
            }

            // Check for 2D objects (SpriteRenderer)
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Enable the SpriteRenderer if it's disabled
                if (!spriteRenderer.enabled)
                {
                    Debug.Log("Enabling sprite renderer for: " + obj.name);
                    spriteRenderer.enabled = true;
                }
            }
        }
    }
}
