﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAIConnector.ChatGPTRepository.models;

namespace ToolManagement.ToolDefinitions
{
    public class UserQuery: IToolDefinition
    {
        public string Name => "UserPersonalInfo";

        public string Description => "Get remembered or pre-entered information about the user, including demographics, interests, and more";

        public List<ToolProperty> InputParameters => new List<ToolProperty>()
        {
            new ToolProperty()
            {
                name = "query",
                type = "string",
                description = "The information that you need",
                IsRequired = true
            }

        };

        public OpenAIToolMessage ExecuteTool(List<OpenAIChatMessage> chatContext, OpenAIToolCall toolCall)
        {
            return new OpenAIToolMessage("stuff", toolCall.id);
        }

    }
}
