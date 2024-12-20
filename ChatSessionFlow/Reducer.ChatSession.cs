using OpenAIConnector.ChatGPTRepository.models;
using ChatSessionFlow.models;
using ActionFlow.Models;

namespace ChatSessionFlow
{
    //Reducer
    public class ChatSessionReducer : IFlowStateReducer<ChatSessionEntity>
    {
        public ChatSessionEntity InitialState => new ChatSessionEntity();

        public List<IFlowReductionBase<ChatSessionEntity>> Reductions => new List<IFlowReductionBase<ChatSessionEntity>>
        {
            this.reduce(CurrentContext_OnInitialMsgReceived_AddToContext,ChatSessionActions.InitAssistantChat()),
            this.reduce(CurrentContext_OnChatResponseReceived_AddToContext, ChatSessionActions.ChatResponseReceived()),
            this.reduce(CurrentContext_OnToolExecutionCompleted_AddToContext, ChatSessionActions.ToolExecutionSucceeded()),
            this.reduce(RequestStack_OnChatRequested_PushRequestToStack, ChatSessionActions.ChatRequested()),
            this.reduce(RequestStack_OnChatRequestReceived_PopRequestFromStack, ChatSessionActions.ToolExecutionsCompleted(), ChatSessionActions.ChatInteractionCompleted()),
            this.reduce(CurrentContext_OnChatRequestReceived_IncrementCount, ChatSessionActions.ChatRequested()),
            this.reduce(CurrentSession_OnChatSessionStart_StartNewSession, ChatSessionActions.ChatSubContextStart()),
            this.reduce(CurrentSession_OnChatSessionCompleted_CompleteSession, ChatSessionActions.ChatSubContextComplete())

        };


        //Reducer Methods
        public ChatSessionEntity CurrentContext_OnInitialMsgReceived_AddToContext(FlowAction<InitialMessage> initAction,ChatSessionEntity currentState)
        {
            currentState.CurrentContext.Add(new OpenAIUserMessage(initAction.Parameters.message));
            return currentState;
        }

        public ChatSessionEntity CurrentContext_OnChatResponseReceived_AddToContext(FlowAction<OpenAIChatResponse> responseAction,ChatSessionEntity currentState)
        {
            currentState.CurrentContext.Add(responseAction.Parameters.choices[0].message);
            return currentState;
        }

        public ChatSessionEntity CurrentContext_OnToolExecutionCompleted_AddToContext(FlowAction<CompletedToolResult> toolExecutedAction, ChatSessionEntity currentState)
        {
            currentState.CurrentContext.Add(toolExecutedAction.Parameters.toolMessage);
            return currentState;
        }

        public ChatSessionEntity RequestStack_OnChatRequested_PushRequestToStack(FlowAction<OpenAIChatRequest> chatRequestedAction, ChatSessionEntity currentState)
        {
            currentState.ChatRequestStack.Push(chatRequestedAction.Parameters);
            return currentState;
        }

        public ChatSessionEntity RequestStack_OnChatRequestReceived_PopRequestFromStack(FlowAction<List<OpenAIToolCall>> chatRequestedAction, ChatSessionEntity currentState)
        {
            currentState.ChatRequestStack.Pop();
            return currentState;
        }

        public ChatSessionEntity CurrentContext_OnChatRequestReceived_IncrementCount(FlowAction<OpenAIChatRequest> chatRequestAction, ChatSessionEntity currentState)
        {
            currentState.NumberOfChats++;
            return currentState;
        }

        public ChatSessionEntity CurrentSession_OnChatSessionStart_StartNewSession(FlowAction newSessionAction, ChatSessionEntity currentState)
        {
            currentState.SessionContexts.TryAdd(currentState.CurrentSession, currentState.CurrentContext);
            currentState.ChatSessionStack.Push(currentState.CurrentSession);
            currentState.CurrentSession = Guid.NewGuid();
            currentState.CurrentContext = new List<OpenAIChatMessage>();
            return currentState;
        }

        public ChatSessionEntity CurrentSession_OnChatSessionCompleted_CompleteSession(FlowAction completeSessionAction, ChatSessionEntity currentState)
        {
            currentState.CurrentSession = currentState.ChatSessionStack.Pop();
            currentState.CurrentContext = currentState.SessionContexts[currentState.CurrentSession];
            return currentState;
        }
    }
}
