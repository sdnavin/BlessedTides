using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

public class QRCodeGenerator : MonoBehaviour
{
    public int qrCodeWidth = 256;
    public int qrCodeHeight = 256;
    public string url = "";
    public string gameId = "";
    public int slotNo;
    private void Start()
    {
        gameId = WebSocketClient.instance.gameDetails.gameId;
        url = url.Replace("{gameid}", gameId).Replace("{slotno}", ""+slotNo) ;
        ApplyTextureWithTransparency(GenerateQRCode(url));
    }

    public Image uiImage; // Assign your UI Image in the Inspector

    public void ApplyTextureWithTransparency(Texture2D texture)
    {
        if (texture == null || uiImage == null)
        {
            Debug.LogError("Texture or UI Image is null!");
            return;
        }

        // Create a copy of the texture to modify (so we don't alter the original asset)
        Texture2D transparentTexture = new Texture2D(texture.width, texture.height, texture.format, false);
        transparentTexture.SetPixels(texture.GetPixels()); // Copy original pixels

        // Iterate through pixels and set white to transparent
        Color[] pixels = transparentTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i] == Color.white) // Check if pixel is white
            {
                pixels[i] = new Color(1, 1, 1, 0); // Set to transparent
            }
        }

        // Apply the changes to the texture
        transparentTexture.SetPixels(pixels);
        transparentTexture.Apply();

        // Convert the modified Texture2D to a Sprite
        Sprite sprite = Sprite.Create(
            transparentTexture,
            new Rect(0, 0, transparentTexture.width, transparentTexture.height),
            new Vector2(0.5f, 0.5f)
        );

        // Assign the Sprite to the UI Image
        uiImage.sprite = sprite;
    }
    public Texture2D GenerateQRCode(string text)
    {
        // Create a BarcodeWriter with a custom Unity-compatible renderer
        var barcodeWriter = new BarcodeWriter<Color32[]>
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Width = qrCodeWidth,
                Height = qrCodeHeight,
                Margin = 1
            },
            Renderer = new UnityColor32Renderer() // Use the custom renderer
        };

        // Generate the QR code as a Color32 array
        var pixelData = barcodeWriter.Write(text);

        // Create a Texture2D from the Color32 array
        var texture = new Texture2D(qrCodeWidth, qrCodeHeight);
        texture.SetPixels32(pixelData);
        texture.Apply();

        return texture;
    }

    // Custom renderer for Unity
    private class UnityColor32Renderer : IBarcodeRenderer<Color32[]>
    {
        public Color32[] Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
        {
            int width = matrix.Width;
            int height = matrix.Height;
            Color32[] pixels = new Color32[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBlack = matrix[x, y];
                    pixels[y * width + x] = isBlack ? Color.black : Color.white;
                }
            }

            return pixels;
        }

        public Color32[] Render(BitMatrix matrix, BarcodeFormat format, string content)
        {
            return Render(matrix, format, content, null);
        }
    }
}
