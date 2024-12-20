using ActionFlow;
using ActionFlow.Models;
using ChatSessionFlow.models;
using FakeDataStorageManager;
using OpenAIConnector.ChatGPTRepository.models;
using ToolManagementFlow;
using ToolManagementFlow.Models;


namespace ChatSessionFlow
{
    public class ToolUseEffects : IFlowStateEffects
    {
        private FlowStateData<ChatSessionEntity> _flowStateData;
        private FlowStateData<ToolManagementStateEntity> _toolStateData;
        private List<IToolDefinition> _definedTools;//TODO: make dictionary?
        private FlowActionHandler _flowActionHandler;


        public ToolUseEffects(FlowActionHandler actionHandler, FlowStateData<ChatSessionEntity> flowStateData, FlowStateData<ToolManagementStateEntity> toolStateData, IEnumerable<IToolDefinition> definedTools)
        {
            _flowActionHandler = actionHandler;
            _flowStateData = flowStateData;
            _toolStateData = toolStateData;
            _definedTools = definedTools.ToList();
        }

        List<IFlowEffectBase> IFlowStateEffects.SideEffects => new List<IFlowEffectBase>
        {
            this.effect(OnChatResponseReceived_IfToolCallsExist_ResolveToolExecutionRequested, ChatSessionActions.ChatResponseReceived()),
            this.effect(OnToolExecutionRequested_ExecuteTools_ResolveToolResult,ChatSessionActions.ToolExecutionRequested()),
            this.effect(OnToolExecutionsCompleted_ResolveChatRequested, ChatSessionActions.ToolExecutionsCompleted())
        };

        //Effect Methods
        public FlowActionBase OnChatResponseReceived_IfToolCallsExist_ResolveToolExecutionRequested(FlowAction<OpenAIChatResponse> chatResponseAction)
        {
            List<OpenAIToolMessage> toolResults = new List<OpenAIToolMessage>();

            List<OpenAIToolCall> toolsCalled = new List<OpenAIToolCall>();
            if (chatResponseAction.Parameters != null && chatResponseAction.Parameters.HasToolCalls(out toolsCalled))
            {
                foreach (var toolCall in toolsCalled)
                {
                    var tool = _definedTools.FirstOrDefault(t => t.Name == toolCall.function.name);
                    if (tool != null)
                    {
                        Dictionary<string, string>? requestStringParameters = tool.GetToolRequestStringParameters(toolCall);
                        Dictionary<string, object>? requestObjectParameters = tool.GetToolRequestObjectParameters(toolCall);
                        Dictionary<string, List<string>>? requestStringArrayParameters = tool.GetToolRequestStringArrayParameters(toolCall);
                        Dictionary<string, List<object>>? requestObjectArrayParameters = tool.GetToolRequestObjArrayParameters(toolCall);

                        var toolRequestParams = new ToolRequestParameters(tool.Name, toolCall.id, requestStringParameters, requestObjectParameters, requestStringArrayParameters, requestObjectArrayParameters);
                        _flowActionHandler.ResolveAction(ChatSessionActions.ToolExecutionRequested(toolRequestParams));
                    }
                }
            }
            if(toolsCalled != null && toolsCalled.Count > 0)
            {
                return ChatSessionActions.ToolExecutionsCompleted(toolsCalled);
            }
            else
            {
                return ChatSessionActions.ChatInteractionCompleted();
            }
        }

        public FlowActionBase OnToolExecutionRequested_ExecuteTools_ResolveToolResult(FlowAction<ToolRequestParameters> toolRequestAction)
        {
            var currentContext = _flowStateData.CurrentState(ChatSessionSelectors.GetChatContext);

            var toolReqParams = toolRequestAction.Parameters;
            var tool = _definedTools.FirstOrDefault(t => t.Name == toolReqParams.ToolName);
            if (tool != null)
            {
                bool toolCallArgumentsValid = tool.RequestArgumentsValid(toolReqParams.StringParameters,toolReqParams.ObjectParameters, toolReqParams.StringArrayParameters, toolReqParams.ObjectArrayParameters);

                if (toolCallArgumentsValid)
                {
                    var toolResult = tool.ExecuteTool(currentContext, toolReqParams);
                    return ChatSessionActions.ToolExecutionSucceeded(new CompletedToolResult{toolName = toolReqParams.ToolName, toolMessage = toolResult, toolRequestParameters = toolReqParams});
                }
            }
            return ChatSessionActions.ToolExecutionFailed(new CompletedToolResult { toolName = toolReqParams.ToolName, 
                toolMessage = new OpenAIToolMessage($"ERROR: Arguments to '{toolReqParams.ToolName}' tool were invalid or missing", toolReqParams.ToolRequestId), toolRequestParameters = toolReqParams });
        }

        public FlowActionBase OnToolExecutionsCompleted_ResolveChatRequested(FlowAction<List<OpenAIToolCall>> chatResponseAction)
        {
            var currentContext = _flowStateData.CurrentState(ChatSessionSelectors.GetChatContext);
            var currentTools = _toolStateData.CurrentState(ToolManagementSelectors.GetToolset);

            OpenAIChatRequest chatRequest = new OpenAIChatRequest
            {
                model = "gpt-4o-mini", //TODO: make these a const or something - magic strings bad.
                messages = currentContext,
                temperature = 1,
                tools = currentTools
            };
            return ChatSessionActions.ChatRequested(chatRequest);
        }


    }
}
