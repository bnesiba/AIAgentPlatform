﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAIConnector.ChatGPTRepository.models;

namespace ToolManagement.ToolDefinitions
{
    public class SendEmail: ToolDefinition
    {
        public string Name => "SendEmail";

        public string Description => "Send an email from the preconfigured address";

        public List<ToolProperty> InputParameters => new List<ToolProperty>()
        {
            new ToolProperty()
            {
                name = "ToAddress",
                type = "string",
                description = "The email address to send the email to"
            },
            new ToolProperty()
            {
                name = "Subject",
                type = "string",
                description = "The subject of the email"
            },
            new ToolProperty()
            {
                name = "Content",
                type = "string",
                description = "The body of the email"
            }

        };

        public OpenAIToolMessage ExecuteTool(List<OpenAIChatMessage> chatContext, OpenAIToolCall toolCall)
        {
            return new OpenAIToolMessage("stuff", toolCall.id);
        }
    }
}