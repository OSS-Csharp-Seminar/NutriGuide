using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NutriGuide.Application.DTOs.Ai;
using NutriGuide.Application.Interfaces;

namespace NutriGuide.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public AiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var groqSettings = configuration.GetSection("GroqSettings");

        var baseUrl = groqSettings["BaseUrl"]
                      ?? throw new InvalidOperationException("Groq BaseUrl is missing.");

        var apiKey = groqSettings["ApiKey"]
                     ?? throw new InvalidOperationException("Groq ApiKey is missing.");

        _model = groqSettings["Model"]
                 ?? throw new InvalidOperationException("Groq Model is missing.");

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<NutritionEstimateResult> EstimateNutritionAsync(string mealDescription)
    {
        var prompt = $$"""
            Analyze the following meal description and estimate its nutritional values.
            Respond ONLY with a valid JSON object, no explanation, no markdown, no extra text.

            Meal: "{{mealDescription}}"

            Respond with exactly this JSON structure:
        {
            "calories": 0,
            "protein_g": 0.0,
            "carbs_g": 0.0,
            "fat_g": 0.0,
            "fiber_g": 0.0,
            "aiNote": "brief note about the estimation in the same language as the meal description"
        }
        """;

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
            temperature = 0.1,
            max_tokens = 300
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/openai/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Groq API error: {error}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseDoc = JsonDocument.Parse(responseJson);

        var aiContent = responseDoc
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;

        var result = JsonSerializer.Deserialize<GroqNutritionResponse>(aiContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null)
            throw new InvalidOperationException("AI encountered an error while processing your request.");

        return new NutritionEstimateResult
        {
            Calories = result.Calories,
            Protein_g = result.Protein_g,
            Carbs_g = result.Carbs_g,
            Fat_g = result.Fat_g,
            Fiber_g = result.Fiber_g,
            AiNote = result.AiNote
        };
    }

    
    private class GroqNutritionResponse
    {
        public int Calories { get; set; }
        public decimal Protein_g { get; set; }
        public decimal Carbs_g { get; set; }
        public decimal Fat_g { get; set; }
        public decimal Fiber_g { get; set; }
        public string AiNote { get; set; } = string.Empty;
    }
}