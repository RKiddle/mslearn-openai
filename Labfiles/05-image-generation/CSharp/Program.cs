using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

string? aoaiEndpoint = null;
string? aoaiKey = null;
try
{
    // Get config settings from AppSettings
    IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
    IConfigurationRoot configuration = builder.Build();
    aoaiEndpoint = configuration["AzureOAIEndpoint"] ?? "";
    aoaiKey = configuration["AzureOAIKey"] ?? "";

    // Get prompt for image to be generated
    Console.Clear();
    Console.WriteLine("Enter a prompt to request an image:");
    string prompt = Console.ReadLine() ?? "";

    // Call the DALL-E model
    using (var client = new HttpClient())
    {
        var contentType = new MediaTypeWithQualityHeaderValue("application/json");
        var api = "openai/deployments/dalle3/images/generations?api-version=2024-02-15-preview";
        client.BaseAddress = new Uri(aoaiEndpoint);
        client.DefaultRequestHeaders.Accept.Add(contentType);
        client.DefaultRequestHeaders.Add("api-key", aoaiKey);
        var data = new
        {
            prompt,
            n = 1,
            size = "1024x1024"
        };

        var jsonData = JsonSerializer.Serialize(data);
        var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(api, contentData);

        // Get the revised prompt and image URL from the response
        var stringResponse = await response.Content.ReadAsStringAsync();
        JsonNode contentNode = JsonNode.Parse(stringResponse)!;
        JsonNode? dataCollectionNode = contentNode?["data"] ?? contentNode?["images"];
        if (dataCollectionNode == null)
        {
            throw new Exception("Data collection node is null.");
        }
        JsonNode dataNode = dataCollectionNode[0]!;
        JsonNode? revisedPrompt = dataNode["revised_prompt"];
        if (revisedPrompt == null)
        {
            throw new Exception("Revised prompt is null.");
        }
        JsonNode? urlNode = dataNode["url"];
        if (urlNode == null)
        {
            throw new Exception("URL node is null.");
        }
        JsonNode url = urlNode;
        Console.WriteLine(revisedPrompt.ToJsonString());
        Console.WriteLine(url.ToJsonString().Replace(@"\u0026", "&"));

    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
