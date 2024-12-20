using ActionFlow;
using ActionFlow.Models;
using OpenAIConnector.ChatGPTRepository.models;
using ToolManagementFlow.Models;

namespace StoryEvaluatorFlow.Tools
{
    public class EvaluateNewStory : IToolDefinition
    {
        private FlowActionHandler _flowActionHandler;

        //static accessor for Tool Management
        public static string ToolName => "EvaluateNewStory";

        public string Name => ToolName;

        public string Description => "Run the evaluation process for a new story, causing it to be stored and searchable";

        public EvaluateNewStory(FlowActionHandler flowActionHandler)
        {
            _flowActionHandler = flowActionHandler;
        }

        public List<ToolProperty> InputParameters => new List<ToolProperty>()
        {
            new ToolProperty()
            {
                name = "StoryToEvaluate",
                type = "string",
                description = "The story to run the evaluation process for",
                IsRequired = true
            },

        };

        public OpenAIToolMessage ExecuteTool(List<OpenAIChatMessage> chatContext, ToolRequestParameters toolParams)
        {
            var storyToEvaluate = toolParams.GetStringParam("StoryToEvaluate");
            _flowActionHandler.ResolveAction(ChatSessionFlow.ChatSessionActions.ChatSubContextStart());//TODO: make 'using'-able? 
            _flowActionHandler.ResolveAction(StoryEvaluatorActions.InitStoryEval(storyToEvaluate));
            _flowActionHandler.ResolveAction(ChatSessionFlow.ChatSessionActions.ChatSubContextComplete());
            return new OpenAIToolMessage($"EvaluateNewStory: " + "Evaluation Executed!", toolParams.ToolRequestId);
        }

    }
}