using ActionFlow.Models;
using ChatSessionFlow.models;
using OpenAIConnector.ChatGPTRepository.models;
using ToolManagementFlow.Models;

namespace ChatSessionFlow
{
    public static class ChatSessionActions
    {
        public static FlowAction<InitialMessage> InitAssistantChat(string initalMsg = "", Guid? chatSessionId = null) => new FlowAction<InitialMessage> { Name = "UserInitialMessage", Parameters = new InitialMessage { message = initalMsg, sessionId = chatSessionId ?? Guid.NewGuid() } };
        public static FlowAction<InitialMessage> InitTooledAssistantChat(string initalMsg = "", Guid? chatSessionId = null) => new FlowAction<InitialMessage> { Name = "TooledAssistantInit", Parameters = new InitialMessage { message = initalMsg, sessionId = chatSessionId ?? Guid.NewGuid() } };
        public static FlowAction<OpenAIChatRequest> ChatRequested(OpenAIChatRequest? request = null) => new FlowAction<OpenAIChatRequest> { Name = "AIChatRequested", Parameters = request };
        public static FlowAction<OpenAIChatResponse> ChatResponseReceived(OpenAIChatResponse? response = null) => new FlowAction<OpenAIChatResponse> { Name = "AIChatResponse", Parameters = response };
        public static FlowAction<ToolRequestParameters> ToolExecutionRequested(ToolRequestParameters? toolRequest = null) => new FlowAction<ToolRequestParameters> { Name = "ToolExecutionRequested", Parameters = toolRequest };
        public static FlowAction<CompletedToolResult> ToolExecutionSucceeded(CompletedToolResult? toolResult = null) => new FlowAction<CompletedToolResult> { Name = "ToolExecutionSucceeded", Parameters = toolResult };
        public static FlowAction<CompletedToolResult> ToolExecutionFailed(CompletedToolResult? toolResult = null) => new FlowAction<CompletedToolResult> { Name = "ToolExecutionFailed", Parameters = toolResult };
        public static FlowAction<List<OpenAIToolCall>> ToolExecutionsCompleted(List<OpenAIToolCall>? toolsRequested = null) => new FlowAction<List<OpenAIToolCall>> { Name = "ToolExecutionCompleted", Parameters = toolsRequested ?? new List<OpenAIToolCall>() };
        public static FlowAction<List<OpenAIToolCall>> ChatInteractionCompleted() => new FlowAction<List<OpenAIToolCall>> { Name = "ChatInteractionCompleted", Parameters = new List<OpenAIToolCall>() };
        //public static FlowAction ResponseValidatonRequested() => new FlowAction { Name = "ResponseValidationRequested" };

        //sub-session actions - used for temporary contexts like tools that use chat (ex. EvaluateNewStory tool)
        public static FlowAction ChatSubContextStart() => new FlowAction { Name = "ChatSubContextStart"};
        public static FlowAction ChatSubContextComplete() => new FlowAction { Name = "ChatSubContextComplete" };

        //stored session actions - used for long-ish term session storage
        public static FlowAction<Guid> ChatSessionUpdated(Guid? sessionId) => new FlowAction<Guid> { Name = "ChatSessionUpdated", Parameters = sessionId ?? new Guid() };

    }
}
