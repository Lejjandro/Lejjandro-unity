using UnityEngine;

public class mainCamera : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float cameraX = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.scaledPixelWidth, 0, 0)).x;
        float cameraY = Camera.main.ScreenToWorldPoint(new Vector3(0, Camera.main.scaledPixelHeight, 0)).y;

        transform.localScale = new Vector3(cameraX, cameraY, 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
