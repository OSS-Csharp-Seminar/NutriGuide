using Microsoft.Extensions.Configuration;
using NutriGuide.Application.DTOs.Ai;
using NutriGuide.Application.DTOs.WeeklyReport;
using NutriGuide.Application.Interfaces;
using NutriGuide.Domain.Models;
using System.Text;
using System.Text.Json;


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
                      Recommended meal should be realistic and commonly eaten.

                      Today's meals: {mealsDescription}

                      Consumed so far:
                      - Calories: {totalCalories} / {target.Calories} kcal
                      - Protein: {totalProtein}g / {target.Protein_g}g
                      - Carbs: {totalCarbs}g / {target.Carbs_g}g
                      - Fat: {totalFat}g / {target.Fat_g}g

                      Suggest one specific meal that would help reach today's targets.
                      Keep the response concise, 2-3 sentences maximum.
                      Respond in English language.
                      """;

        return await SendMessageAsync(prompt, temperature: 0.9);
    }

    public async Task<string> GenerateTargetMissRecommendationAsync(List<MealLog> todaysMeals, DailyTarget target)
    {
        var mealsDescription = todaysMeals.Any()
        ? string.Join(", ", todaysMeals.Select(m => m.RawInput))
        : "nothing yet";

        var totalCalories = todaysMeals.Sum(m => m.Calories ?? 0);
        var totalProtein = todaysMeals.Sum(m => m.Protein_g ?? 0);
        var remainingCalories = target.Calories - totalCalories;
        var remainingProtein = target.Protein_g - totalProtein;

        var gaps = new List<string>();
        if (remainingCalories > 0) gaps.Add($"about {remainingCalories} kcal");
        if (remainingProtein > 0) gaps.Add($"about {remainingProtein}g protein");

        string prompt;
        if (gaps.Count == 0)
        {

            prompt = $"""
                  You are a nutrition analyst. The user has already met their calorie and protein targets for today.
                  Today they ate: {mealsDescription}
                  Briefly congratulate them and, if appropriate, suggest a light option only if they're still hungry.
                  Keep it to 1-2 sentences. Respond in English language.
                  """;
        }
        else
        {
            prompt = $"""
                  You are a nutrition analyst. The user is ending their day still short on some targets.

                  Today they ate: {mealsDescription}

                  They still need {string.Join(" and ", gaps)} to reach their remaining targets.
                  Their other targets are already met, so do not push more of those.

                  Suggest one realistic snack or small meal to help close the gap — it can be sweet or savory,
                  and should be varied and specific. Avoid defaulting to common suggestions like plain yogurt.
                  Keep the response concise, 2-3 sentences maximum.
                  Respond in English language.
                  """;
        }

        return await SendMessageAsync(prompt, temperature: 0.9);
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
    
    public async Task<WellnessAnalysisResult> AnalyzeWellnessAsync(
        string symptoms, List<MealLog> last48HoursMeals)
    {
        var mealsDescription = last48HoursMeals.Any()
            ? string.Join(", ", last48HoursMeals.Select(m =>
                $"{m.RawInput} ({m.LoggedAt:dd.MM HH:mm})"))
            : "No meals logged in the last 48 hours.";

        var jsonStructure = """
                            {
                                "analysis": "<2-3 sentences analyzing the nutritional connection to symptoms>",
                                "suggestedMeal": "<a meal suggestion if one would genuinely help, otherwise an empty string>"
                            }
                            """;

        var prompt = $"""
                      You are a nutrition assistant. The user is not feeling well and needs advice.

                      User symptoms: "{symptoms}"

                      Meals in the last 48 hours: {mealsDescription}

                      Guidelines for your analysis:
                      - If the user reports feeling good or well rather than a problem, affirm it warmly, 
                        note their diet appears to be supporting them, and do not invent issues or force a corrective meal.
                      - If they report a problem, first judge whether the recent meals look balanced and adequate. 
                        If they do, say so plainly and note that the symptoms may not be diet-related. 
                        Do NOT invent a nutritional cause when none is evident.
                      - Only point to a nutritional factor if there is a clear, well-established link. 
                        Avoid speculative or obscure mechanisms.
                      - Suggest a meal only when it would genuinely help; otherwise the suggestion can be a light general option or encouragement.
                      - Be honest rather than reassuring or alarming.

                      Then suggest one realistic meal or snack that could genuinely help — it must include actual food, not only a drink.

                      Respond ONLY with a valid JSON object, no explanation, no markdown, no extra text.

                      {jsonStructure}

                      Respond in English language.
                      """;

        var responseText = await SendMessageAsync(prompt);
        var cleaned = ExtractJson(responseText);

        WellnessAnalysisResult? result;
        try
        {
            result = JsonSerializer.Deserialize<WellnessAnalysisResult>(cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("AI returned data that could not be read. Please try again.");
        }

        if (result == null)
            throw new InvalidOperationException("AI returned invalid data.");

        return result;
    }

    public async Task<string> GenerateWeeklySummaryAsync(WeeklyReportDto report)
    {
        var weightLine = report.ThisWeekWeight_kg is not null && report.WeightChange_kg is not null
            ? $"Weight: {report.ThisWeekWeight_kg} kg ({(report.WeightChange_kg >= 0 ? "+" : "")}{report.WeightChange_kg} kg since start). Goal: {report.Goal}."
            : "No weight recorded this week.";

        var prompt = $"""
                      You are a supportive nutrition coach summarizing a user's week. Use only the figures provided — do not invent numbers.

                      Days with logged meals: {report.DaysLogged}
                      Daily averages: {report.AvgCalories} kcal, protein {report.AvgProtein_g}g, carbs {report.AvgCarbs_g}g, fat {report.AvgFat_g}g, fiber {report.AvgFiber_g}g.
                      Calorie target: {report.TargetCalories} kcal/day (averaged {report.CalorieAdherence_pct}% of target).
                      {weightLine}

                      Write a short, encouraging summary of the week in 2-4 sentences. Mention how close they were to their calorie target, one nutritional strength or area to improve, and whether their weight trend fits their goal. Be honest but constructive. Respond in English, plain text only.
                      """;

        return await SendMessageAsync(prompt);
    }
}