using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using NutriGuide.Application.DTOs.Ai;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Models;


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
        You are a professional nutrition analyst
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

        var aiContent = await SendMessageAsync(prompt, temperature: 0.1, maxTokens: 300);

        var cleaned = ExtractJson(aiContent);

        GroqNutritionResponse? result;
        try
        {
            result = JsonSerializer.Deserialize<GroqNutritionResponse>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("AI returned data that could not be read. Please try rephrasing the meal.");
        }

        if (result == null)
            throw new InvalidOperationException("AI returned invalid data.");

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

    private static string ExtractJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        var text = raw.Trim();

        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            if (firstNewline >= 0)
                text = text[(firstNewline + 1)..];
            if (text.EndsWith("```"))
                text = text[..^3];
            text = text.Trim();
        }

        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
            text = text[start..(end + 1)];

        return text;
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
    
    
    
    public async Task<string> GenerateNextMealRecommendationAsync(List<MealLog> todaysMeals, DailyTarget target)
    {
        var mealsDescription = string.Join(", ", todaysMeals.Select(m => m.RawInput));
        var totalCalories = todaysMeals.Sum(m => m.Calories ?? 0);
        var totalProtein = todaysMeals.Sum(m => m.Protein_g ?? 0);
        var totalCarbs = todaysMeals.Sum(m => m.Carbs_g ?? 0);
        var totalFat = todaysMeals.Sum(m => m.Fat_g ?? 0);

        var prompt = $"""
                      You are a nutrition analyst. Based on what the user has eaten today, suggest their next meal.
                      Rocommended meal should be realistic and commonly eaten.

                      Today's meals: {mealsDescription}

                      Consumed so far:
                      - Calories: {totalCalories} / {target.Calories} kcal
                      - Protein: {totalProtein}g / {target.Protein_g}g
                      - Carbs: {totalCarbs}g / {target.Carbs_g}g
                      - Fat: {totalFat}g / {target.Fat_g}g

                      Suggest one specific meal that would help reach today's targets.
                      Keep the response concise, 2-3 sentences maximum.
                      Respond in Enlish language.
                      """;

        return await SendMessageAsync(prompt);
    }

    public async Task<string> GenerateTargetMissRecommendationAsync(List<MealLog> todaysMeals, DailyTarget target)
    {
        var totalCalories = todaysMeals.Sum(m => m.Calories ?? 0);
        var totalProtein = todaysMeals.Sum(m => m.Protein_g ?? 0);
        var remainingCalories = target.Calories - totalCalories;
        var remainingProtein = target.Protein_g - totalProtein;

        var prompt = $"""
                      You are a nutrition analyst. The user is at the end of the day and hasn't reached their targets.

                      Remaining to reach targets:
                      - Calories: {remainingCalories} kcal
                      - Protein: {remainingProtein}g

                      Suggest a light evening snack or meal to help close the gap.
                      Keep the response concise, 2-3 sentences maximum.
                      The recommendation should help the user improve their nutritional balance before the end of the day.
                      Respond in English language.
                      """;

        return await SendMessageAsync(prompt);
    }

    private async Task<string> SendMessageAsync(string prompt, double temperature = 0.7, int? maxTokens = null)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
            new { role = "user", content = prompt }
        },
            temperature,
            max_tokens = maxTokens
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

        return responseDoc
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()!;
    }
}