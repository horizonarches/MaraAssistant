namespace PatientCareChatbotPortal.Services;

public sealed class ConversationHistoryEntry
{
    public string Role { get; }
    public string Content { get; }

    public ConversationHistoryEntry(string role, string content)
    {
        Role = role;
        Content = content;
    }
}
