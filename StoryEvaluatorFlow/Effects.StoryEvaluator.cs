﻿using OpenAIConnector.ChatGPTRepository.models;
using OpenAIConnector.ChatGPTRepository;
using ToolManagement;
using ActionFlow.Models;
using ActionFlow;
using ChatSessionFlow.models;
using StoryEvaluatorFlow;
using StoryEvaluatorFlow.Models;
using ToolManagementFlow.Models;
using FakeDataStorageManager;
using StoryEvaluatorFlow.Tools;
using ToolManagementFlow;

namespace ChatSessionFlow
{
    public class StoryEvaluatorEffects : IFlowStateEffects
    {
        private FlowStateData<StoryEvaluatorEntity> _storyEvaluatorState;
        private FlowStateData<ToolManagementStateEntity> _toolStateData;
        private FlowActionHandler _flowActionHandler;
        private TotallyRealDatabase<StoryEvaluatorEntity> _totallyADatabase;



        public StoryEvaluatorEffects(FlowActionHandler flowHandler, FlowStateData<StoryEvaluatorEntity> storyState, FlowStateData<ToolManagementStateEntity> toolState, TotallyRealDatabase<StoryEvaluatorEntity> storyDatabase)
        {
            _flowActionHandler = flowHandler;
            _storyEvaluatorState = storyState;
            _toolStateData = toolState;
            _totallyADatabase = storyDatabase;

        }

        List<IFlowEffectBase> IFlowStateEffects.SideEffects => new List<IFlowEffectBase>
        {
           this.effect(OnInitialStoryMsg_CreateChatRequest_ResolveChatRequestedAndStoryEvalComplete, StoryEvaluatorActions.InitStoryEval()),
           this.effect(OnToolExecutionRequested_WhenInitiatingNewEvaluation_ResolveInitStoryEval, ChatSessionActions.ToolExecutionRequested()),
           this.effect(OnInitialStoryChat_CreateChatRequest_ResolveChatRequested,StoryEvaluatorActions.InitStoryChat())
        };

        //Effect Methods
        public FlowActionBase OnInitialStoryMsg_CreateChatRequest_ResolveChatRequestedAndStoryEvalComplete(FlowAction<InitialMessage> initialMsg)
        {
            //set tools
            _flowActionHandler.ResolveAction(ToolManagementActions.SetToolset(new List<string> {
                SetCharacterList.ToolName,
                SetStoryTags.ToolName, 
                SetGeneralInfo.ToolName, 
                SetStorySummary.ToolName 
            }));
            var tools = _toolStateData.CurrentState(ToolManagementSelectors.GetToolset);

            List<OpenAIChatMessage> newContext = new List<OpenAIChatMessage>();
            newContext.Add(new OpenAISystemMessage("You are a story tracker. You evaluate stories and extract/provide information about the story such as authors, characters, content-tags, and themes"));
            newContext.Add(new OpenAIUserMessage(initialMsg.Parameters.message));
            OpenAIChatRequest chatRequest = new OpenAIChatRequest
            {
                model = "gpt-4o-mini", //TODO: make these a const or something - magic strings bad.
                messages = newContext,
                temperature = 1,
                tools = tools
                //tool_choice = _toolManager.GetDefaultToolDefinitions().First() //If this is commented out, all tools still seem to get used correctly.
            };

            _flowActionHandler.ResolveAction(ChatSessionActions.ChatRequested(chatRequest));

            var storyData = _storyEvaluatorState.CurrentState(StoryEvaluatorSelectors.GetStoryEvaluation);
            _totallyADatabase.AddStory(storyData, initialMsg.Parameters.message);

            return StoryEvaluatorActions.StoryEvalCompleted();
        }

        public FlowActionBase OnToolExecutionRequested_WhenInitiatingNewEvaluation_ResolveInitStoryEval(
            FlowAction<ToolRequestParameters> toolRequest)
        {
            if (toolRequest.Parameters.ToolName == EvaluateNewStory.ToolName)
            {
                var storyToEvaluate = toolRequest.Parameters.GetStringParam("StoryToEvaluate") ?? string.Empty; //TODO: Get parameter strings from the tools somehow
                return StoryEvaluatorActions.InitStoryEval(storyToEvaluate);
            }
            return StoryEvaluatorActions.StoryEvalNoop("StoryEvaluatorEffects-OnToolExecutionRequested_WhenInitiatingNewEvaluation_ResolveInitStoryEval");
        }

        public FlowActionBase OnInitialStoryChat_CreateChatRequest_ResolveChatRequested(
            FlowAction<InitialMessage> initialMessage)
        {

            //_toolManager.UseStoryChatTools();
            _flowActionHandler.ResolveAction(ToolManagementActions.SetToolset(new List<string>{
                SearchForStories.ToolName,
                EvaluateNewStory.ToolName
            }));
            return ChatSessionActions.InitAssistantChat(initialMessage.Parameters.message, initialMessage.Parameters.sessionId);
        
        }

    }
}
