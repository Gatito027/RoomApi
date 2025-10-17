using Azure.AI.ContentSafety;
using Azure;

namespace RoomApi.Utils
{
    public class ContentSafetyValidator
    {
        private readonly ContentSafetyClient _client;

        public ContentSafetyValidator(ContentSafetyClient client)
        {
            _client = client;
        }

        public async Task<bool> TextContainsHarmfulContentAsync(string text)
        {
            var result = await _client.AnalyzeTextAsync(new AnalyzeTextOptions(text));
            return result.Value.CategoriesAnalysis.Any(c => c.Severity >= 1);
        }

        public async Task<bool> ImageContainsHarmfulContentAsync(Uri imageUrl)
        {
            var result = await _client.AnalyzeImageAsync(imageUrl);
            return result.Value.CategoriesAnalysis.Any(c => c.Severity >= 1);
        }
    }
}