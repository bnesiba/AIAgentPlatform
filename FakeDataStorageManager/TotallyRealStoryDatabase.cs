using System.Reflection.Metadata;
using Newtonsoft.Json;

namespace FakeDataStorageManager
{
    public class TotallyRealStoryDatabase<T> : ITotallyAStoryDatabase where T : ITagSearchable
    {
        public Dictionary<string, List<Guid>> TagToStoryIds { get; set; }
        public Dictionary<Guid, T> StoryIdToStory { get; set; }

        public Dictionary<Guid, string> StoryFullText { get; set; }

        public TotallyRealStoryDatabase()
        {
            TagToStoryIds = new Dictionary<string, List<Guid>>();
            StoryIdToStory = new Dictionary<Guid, T>();
            StoryFullText = new Dictionary<Guid, string>();
        }

        public void AddStory(T story, string fullText)
        {
            var storyId = Guid.NewGuid();
            StoryIdToStory.Add(storyId, story);
            StoryFullText.Add(storyId, fullText);
            foreach (var tag in story.SearchTags)
            {
                if (!TagToStoryIds.ContainsKey(tag))
                {
                    TagToStoryIds[tag] = new List<Guid>();
                }
                TagToStoryIds[tag].Add(storyId);
            }
        }

        public List<T> GetStoriesDataByTags(List<string> tags)
        {
            var storyIds = new HashSet<Guid>();
            foreach (var tag in tags)
            {
                if (TagToStoryIds.ContainsKey(tag))
                {
                    storyIds.UnionWith(TagToStoryIds[tag]);
                }
            }
            return storyIds.Select(id => StoryIdToStory[id]).ToList();
        }

        public List<T> GetStoriesDataByTag(string tag)
        {
            return GetStoriesDataByTags(new List<string> { tag });
        }

        public string GetStoryFullText(Guid storyId)
        {
            return StoryFullText[storyId];
        }

        //Note: simple implementation for demo - obviously couldn't work this way at scale
        public List<T> GetStoriesDataByAuthor(string author)
        {
            return StoryIdToStory.Values.Where(story => story.Authors.Contains(author)).ToList();
        }

        public List<string> GetSerializedResultsByTagsAndAuthors(List<string> searchTags, List<string> authors)
        {
            HashSet<string> results = new HashSet<string>();
            foreach (var story in GetStoriesDataByTags(searchTags))
            {
                results.Add(JsonConvert.SerializeObject(story));
            }

            foreach (var author in authors)
            {
                foreach (var story in GetStoriesDataByAuthor(author))
                {
                    results.Add(JsonConvert.SerializeObject(story));
                }
            }

            return results.ToList();
        }
    }

    public interface ITotallyAStoryDatabase
    {
        public List<string> GetSerializedResultsByTagsAndAuthors(List<string> searchTags, List<string> authors);
    }

    public interface ITagSearchable
    {
        public HashSet<string> SearchTags { get; set; }
        public List<string> Authors { get; set; }
    }
}
