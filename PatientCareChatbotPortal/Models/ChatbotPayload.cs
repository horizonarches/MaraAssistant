using static PatientCareChatbotPortal.Services.AppStateService;

namespace PatientCareChatbotPortal.Models;

public sealed class ChatbotPayload
{
    public string Response { get; set; } = string.Empty;
    public PatientCondition PatientCondition { get; set; } = new PatientCondition();
    public RoomCondition RoomCondition { get; set; } = new RoomCondition();
}
