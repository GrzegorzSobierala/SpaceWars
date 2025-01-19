using UnityEngine;
using System.IO;

public class CameraOpaqueTextureSaver : MonoBehaviour
{
    public string fileName = "CameraOpaqueTexture.png";

    void SaveCameraOpaqueTexture()
    {
        // Get the _CameraOpaqueTexture
        var opaqueTexture = Shader.GetGlobalTexture("_CameraOpaqueTexture") as RenderTexture;

        if (opaqueTexture == null)
        {
            Debug.LogError("_CameraOpaqueTexture is not available.");
            return;
        }

        // Create a temporary Texture2D to read the RenderTexture data
        RenderTexture activeRT = RenderTexture.active;
        RenderTexture.active = opaqueTexture;

        Texture2D texture2D = new Texture2D(opaqueTexture.width, opaqueTexture.height, TextureFormat.RGB24, false);
        texture2D.ReadPixels(new Rect(0, 0, opaqueTexture.width, opaqueTexture.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = activeRT;

        // Convert the Texture2D to PNG
        byte[] imageBytes = texture2D.EncodeToPNG();

        // Save the PNG file
        string path = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(path, imageBytes);

        Debug.Log($"_CameraOpaqueTexture saved to: {path}");

        // Clean up
        Destroy(texture2D);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Press "S" to save
        {
            SaveCameraOpaqueTexture();
        }
    }
}
