using OpenAI;
using System.Text.Json;
using PatientCareChatbotPortal.Models;
using OpenAI.Responses;
using OpenAI.Audio;

namespace PatientCareChatbotPortal.Services;

public sealed class ChatbotService
{
    #pragma warning disable OPENAI001
    private static readonly JsonSerializerOptions PayloadJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ResponsesClient _responsesClient;
    private readonly SpeechGenerationAudioClient _audioClient;
    private readonly AudioService _audioService;
    private List<ConversationHistoryEntry> _conversationHistory = new List<ConversationHistoryEntry>();
    public ChatbotService(ResponsesClient responsesClient, SpeechGenerationAudioClient audioClient, AudioService audioService)
    {
        _responsesClient = responsesClient;
        _audioClient = audioClient;
        _audioService = audioService;
    }
    public async Task<string> GetTaggedResponseAsync(string prompt, AppStateService state, CancellationToken cancellationToken = default)
    {
        var safePrompt = prompt.Trim();

        string systemPromptText;
#if DEBUG
        // In debug mode, read from the file system directly
    string resourcePath = System.IO.Path.Combine("C:\\Users\\horiz\\OneDrive\\Documents\\Masters\\AI in Healthcare\\HRP\\PatientCareChatbotPortal\\Resources\\Raw\\", "systemPrompt.txt");
        systemPromptText = await File.ReadAllTextAsync(resourcePath);
#else
        // In release mode, use the packaged app
        using (var stream = await FileSystem.OpenAppPackageFileAsync("systemPrompt.txt"))
        using (var reader = new StreamReader(stream))
        {
            systemPromptText = await reader.ReadToEndAsync();
        }
#endif

        MessageResponseItem systemPrompt = ResponseItem.CreateSystemMessageItem(systemPromptText);
        MessageResponseItem userPrompt = ResponseItem.CreateUserMessageItem(safePrompt);

        string patientSummaryJson = JsonSerializer.Serialize(state.CurrentPatientSummary);
        MessageResponseItem patientSummaryPrompt = ResponseItem.CreateDeveloperMessageItem(patientSummaryJson);
        
        string patientConditionJson = JsonSerializer.Serialize(state.CurrentPatientCondition);
        MessageResponseItem patientConditionPrompt = ResponseItem.CreateDeveloperMessageItem(patientConditionJson);
        
        string roomConditionJson = JsonSerializer.Serialize(state.CurrentRoomCondition);
        MessageResponseItem roomConditionPrompt = ResponseItem.CreateDeveloperMessageItem(roomConditionJson);

        BinaryData responseJsonSchema = BinaryData.FromString(
            """
            {
              "type": "object",
              "properties": {
                "response": { "type": "string" },
                "patientCondition": {
                  "type": "object",
                  "properties": {
                    "_pain_level":   { "type": "number" },
                    "_stress_level": { "type": "number" },
                    "_sleep_quality":{ "type": "number" }
                  },
                  "required": ["_pain_level", "_stress_level", "_sleep_quality"],
                  "additionalProperties": false
                },
                "roomCondition": {
                  "type": "object",
                  "properties": {
                    "_light":              { "type": "number" },
                    "_sound":              { "type": "string" },
                    "_temperature_f":      { "type": "number" },
                    "_temperature_c":      { "type": "number" },
                    "_temperatureUnit":    { "type": "string" },
                    "_bedInclineDegrees":  { "type": "number" },
                    "_currentPhotoSet":    { "type": "string" }
                  },
                  "required": ["_light", "_sound", "_temperature_f", "_temperature_c", "_temperatureUnit", "_bedInclineDegrees", "_currentPhotoSet"],
                  "additionalProperties": false
                }
              },
              "required": ["response", "patientCondition", "roomCondition"],
              "additionalProperties": false
            }
            """
        );

        CreateResponseOptions options = new CreateResponseOptions
        {
            Model = "gpt-5.4",
            InputItems =
            {
                systemPrompt,
                patientSummaryPrompt,
                patientConditionPrompt,
                roomConditionPrompt
            },
            TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "PatientCareChatbotResponseFormat",
                    jsonSchema: responseJsonSchema,
                    jsonSchemaIsStrict: true
                ),
            },
            StoredOutputEnabled = false
        };

        var conversationHistoryLength = _conversationHistory.Count;
        var maxHistoryItems = 10;

        foreach (var entry in _conversationHistory.Skip(Math.Max(0, conversationHistoryLength - maxHistoryItems)))
        {
            switch (entry.Role)
            {
                case "user":
                    options.InputItems.Add(ResponseItem.CreateUserMessageItem(entry.Content));
                    break;
                case "assistant":
                    options.InputItems.Add(ResponseItem.CreateAssistantMessageItem(entry.Content));
                    break;
                default:
                    options.InputItems.Add(ResponseItem.CreateDeveloperMessageItem(entry.Content));
                    break;
            }
        }

        options.InputItems.Add(userPrompt);

        ResponseResult response = await _responsesClient.CreateResponseAsync(
            options,
            cancellationToken: cancellationToken
        );

        string returnedResponseText = response.GetOutputText();
        var deserializedResponseContent = TryDeserializePayload(returnedResponseText);
        string assistantResponseText = string.IsNullOrWhiteSpace(deserializedResponseContent?.Response)
            ? returnedResponseText
            : deserializedResponseContent.Response;

        _conversationHistory.Add(new ConversationHistoryEntry("user", safePrompt));
        _conversationHistory.Add(new ConversationHistoryEntry("assistant", assistantResponseText));

        var speechUpdates = _audioClient.Client.GenerateSpeechStreamingAsync(
            assistantResponseText,
            GeneratedSpeechVoice.Sage,
            cancellationToken: cancellationToken);

        await _audioService.PlayGeneratedSpeechAsync(speechUpdates, cancellationToken);

        await _audioService.PlayBackgroundAudioAsync(deserializedResponseContent?.RoomCondition._sound ?? "NONE");

        var json = deserializedResponseContent is null
            ? returnedResponseText
            : JsonSerializer.Serialize(deserializedResponseContent);

        return json;
    }

    private static ChatbotPayload? TryDeserializePayload(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(value);

            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                return JsonSerializer.Deserialize<ChatbotPayload>(value, PayloadJsonOptions);
            }

            if (doc.RootElement.ValueKind == JsonValueKind.String)
            {
                string? innerJson = doc.RootElement.GetString();
                if (!string.IsNullOrWhiteSpace(innerJson))
                {
                    return JsonSerializer.Deserialize<ChatbotPayload>(innerJson, PayloadJsonOptions);
                }
            }

            return null;
        }
        catch (JsonException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to deserialize chatbot response payload: {ex.Message}");
            return null;
        }
    }
}
#pragma warning restore OPENAI001
