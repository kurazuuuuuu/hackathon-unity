using Newtonsoft.Json.Linq;
using MCPForUnity.Editor.Helpers;
using MCPForUnity.Editor.Tools;

namespace MyProject.Editor.CustomTools
{
    [McpForUnityTool("my_custom_tool")]
    public static class MyCustomTool
    {
        public class Parameters
        {
            [ToolParameter("Value to process")]
            public string param1 { get; set; }

            [ToolParameter("Optional integer payload", Required = false)]
            public int? param2 { get; set; }
        }

        public static object HandleCommand(JObject @params)
        {
            var parameters = @params.ToObject<Parameters>();

            if (string.IsNullOrEmpty(parameters.param1))
            {
                return new ErrorResponse("param1 is required");
            }

            DoSomethingAmazing(parameters.param1, parameters.param2);

            return new SuccessResponse("Custom tool executed successfully!", new
            {
                parameters.param1,
                parameters.param2
            });
        }

        private static void DoSomethingAmazing(string param1, int? param2)
        {
            // Your implementation
        }
    }
}