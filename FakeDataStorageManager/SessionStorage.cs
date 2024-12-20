using OpenAIConnector.ChatGPTRepository.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeDataStorageManager
{
    public class SessionStorage
    {
        private Dictionary<Guid, List<OpenAIChatMessage>> ChatSessionStorage { get; set; }

        public SessionStorage()
        {
            ChatSessionStorage = new Dictionary<Guid, List<OpenAIChatMessage>>();
        }

        public Guid StartSession()
        {
            var newSessionGuid = Guid.NewGuid();
            ChatSessionStorage.Add(newSessionGuid, new List<OpenAIChatMessage>());
            return newSessionGuid;
        }

        public void UpdateSession(Guid sessionGuid, List<OpenAIChatMessage> messages)
        {
            ChatSessionStorage[sessionGuid] = messages;
        }

    }
}
