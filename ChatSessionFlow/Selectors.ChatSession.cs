using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionFlow.Models;
using ChatSessionFlow.models;
using OpenAIConnector.ChatGPTRepository.models;

namespace ChatSessionFlow
{
    public static class ChatSessionSelectors
    {
        public static FlowDataSelector<ChatSessionEntity, int> GetChatCount = new FlowDataSelector<ChatSessionEntity, int>((stateData) => stateData.NumberOfChats);

        public static FlowDataSelector<ChatSessionEntity, OpenAIChatRequest?> GetCurrentChatRequest = new FlowDataSelector<ChatSessionEntity, OpenAIChatRequest?>((stateData) => stateData.ChatRequestStack.FirstOrDefault());
        public static FlowDataSelector<ChatSessionEntity, List<OpenAIChatMessage>> GetChatContext 
            = new FlowDataSelector<ChatSessionEntity, List<OpenAIChatMessage>>((stateData) => stateData.CurrentContext);

        public static FlowDataSelector<ChatSessionEntity, OpenAIChatMessage> GetLatestMessage = 
            new FlowDataSelector<ChatSessionEntity, OpenAIChatMessage>((stateData) => stateData.CurrentContext.LastOrDefault());

        public static FlowDataSelector<ChatSessionEntity, Guid> GetSessionId = new FlowDataSelector<ChatSessionEntity, Guid>((stateData) => stateData.CurrentSession);
    }
}
