using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using MCPForUnity.Editor.Tools;
using MCPForUnity.Editor.Helpers;

namespace MyProject.Editor.CustomTools
{
    [McpForUnityTool(
        name: "capture_screenshot",
        Description = "Capture screenshots in Unity, saving them as PNGs"
    )]
    public static class CaptureScreenshotTool
    {
        // Define parameters as a nested class for clarity
        public class Parameters
        {
            [ToolParameter("Screenshot filename without extension, e.g., screenshot_01")]
            public string filename { get; set; }

            [ToolParameter("Width of the screenshot in pixels", Required = false)]
            public int? width { get; set; }

            [ToolParameter("Height of the screenshot in pixels", Required = false)]
            public int? height { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            // Parse parameters
            var parameters = @params.ToObject<Parameters>();

            if (string.IsNullOrEmpty(parameters.filename))
            {
                return new ErrorResponse("filename is required");
            }

            try
            {
                int width = parameters.width ?? Screen.width;
                int height = parameters.height ?? Screen.height;

                string absolutePath = Path.Combine(Application.dataPath, "Screenshots",
                    parameters.filename + ".png");
                Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));

                // Find camera
                Camera camera = Camera.main ?? Object.FindFirstObjectByType<Camera>();
                if (camera == null)
                {
                    return new ErrorResponse("No camera found in the scene");
                }

                // Capture screenshot
                RenderTexture rt = new RenderTexture(width, height, 24);
                camera.targetTexture = rt;
                camera.Render();

                RenderTexture.active = rt;
                Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
                screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenshot.Apply();

                // Cleanup
                camera.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(rt);

                // Save
                byte[] bytes = screenshot.EncodeToPNG();
                File.WriteAllBytes(absolutePath, bytes);
                Object.DestroyImmediate(screenshot);

                return new SuccessResponse($"Screenshot saved to {absolutePath}", new
                {
                    path = absolutePath,
                    width = width,
                    height = height
                });
            }
            catch (System.Exception ex)
            {
                return new ErrorResponse($"Failed to capture screenshot: {ex.Message}");
            }
        }
    }
}
