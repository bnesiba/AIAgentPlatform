﻿using ChatSessionFlow.models;
using OpenAIConnector.ChatGPTRepository.models;
using OpenAIConnector.ChatGPTRepository;
using SessionStateFlow.package.Models;
using SessionStateFlow.package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolManagement;
using ToolManagement.ToolDefinitions.Models;


namespace ChatSessionFlow
{
    public class ToolUseEffects : IFlowStateEffects
    {
        private FlowStateData<ChatSessionEntity> _flowStateData;
        private ToolDefinitionManager _toolManager;
        private List<IToolDefinition> _definedTools;//TODO: make dictionary?
        private FlowActionHandler _flowActionHandler;


        public ToolUseEffects(FlowActionHandler actionHandler, FlowStateData<ChatSessionEntity> flowStateData, ToolDefinitionManager toolManager, IEnumerable<IToolDefinition> definedTools)
        {
            _flowActionHandler = actionHandler;
            _flowStateData = flowStateData;
            _toolManager = toolManager;
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
                        Dictionary<string, List<string>>? requestArrayParameters = tool.GetToolRequestArrayParameters(toolCall);

                        var toolRequestParams = new ToolRequestParameters(tool.Name, toolCall.id, requestStringParameters, requestArrayParameters);
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
                return ChatSessionActions.ToolsExecutionEmpty();
            }
        }

        public FlowActionBase OnToolExecutionRequested_ExecuteTools_ResolveToolResult(FlowAction<ToolRequestParameters> toolRequestAction)
        {
            var currentContext = _flowStateData.CurrentState(ChatSessionSelectors.GetChatContext);

            var toolReqParams = toolRequestAction.Parameters;
            var tool = _definedTools.FirstOrDefault(t => t.Name == toolReqParams.ToolName);
            if (tool != null)
            {
                bool toolCallArgumentsValid = tool.RequestArgumentsValid(toolReqParams.StringParameters, toolReqParams.ArrayParameters);

                if (toolCallArgumentsValid)
                {
                    var toolResult = tool.ExecuteTool(currentContext, toolReqParams);
                    return ChatSessionActions.ToolExecutionSucceeded(toolResult);
                }
            }
            return ChatSessionActions.ToolExecutionFailed(new OpenAIToolMessage($"ERROR: Arguments to '{toolReqParams.ToolName}' tool were invalid or missing", toolReqParams.ToolRequestId));
        }

        public FlowActionBase OnToolExecutionsCompleted_ResolveChatRequested(FlowAction<List<OpenAIToolCall>> chatResponseAction)
        {
            var currentContext = _flowStateData.CurrentState(ChatSessionSelectors.GetChatContext);
            
            OpenAIChatRequest chatRequest = new OpenAIChatRequest
            {
                model = "gpt-3.5-turbo", //TODO: make these a const or something - magic strings bad.
                //model = "gpt-4o",
                messages = currentContext,
                temperature = 1,
                tools = _toolManager.GetToolDefinitions()
            };
            return ChatSessionActions.ChatRequested(chatRequest);
        }


    }
}
