using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TechNewsAggregator.Models;

namespace TechNewsAggregator.Services
{
    public class NewsScraperService
    {
        private readonly string _techCrunchUrl = "https://techcrunch.com/";
        private readonly string _theVergeUrl = "https://www.theverge.com/";
        private readonly string _firecrawlApiKey;
        private readonly string _firecrawlApiUrl = "https://api.firecrawl.dev/v1/scrape";
        private readonly string _claudeApiKey;
        private readonly string _claudeApiUrl = "https://api.anthropic.com/v1/messages";

        public NewsScraperService(string firecrawlApiKey, string claudeApiKey)
        {
            _firecrawlApiKey = firecrawlApiKey;
            _claudeApiKey = claudeApiKey;
        }

        public async Task<List<NewsItem>> ScrapeNewsSourcesAsync()
        {
            var newsItems = new List<NewsItem>();
            var markdownContentList = new List<string>();

            try
            {
                // Scrape TechCrunch
                var techCrunchMarkdown = await ScrapeWebsiteAsync(_techCrunchUrl);
                if (techCrunchMarkdown != null)
                {
                    markdownContentList.Add(techCrunchMarkdown);
                }

                // Scrape The Verge
                var theVergeMarkdown = await ScrapeWebsiteAsync(_theVergeUrl);
                if (theVergeMarkdown != null)
                {
                    markdownContentList.Add(theVergeMarkdown);
                }

                // Combine the markdown content from both sources
                var combinedMarkdownContent = string.Join("\n\n", markdownContentList);

                // Extract data from combined markdown using Claude
                if (!string.IsNullOrEmpty(combinedMarkdownContent))
                {
                    newsItems = await ExtractDataFromMarkdownAsync(combinedMarkdownContent);
                }
                else
                {
                    Console.WriteLine("Failed to extract any markdown content from the sources.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ScrapeAllSourcesAsync: {e.Message}");
            }

            return newsItems;
        }

        private async Task<string?> ScrapeWebsiteAsync(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        url = url,
                        formats = new[] { "markdown" }
                    };

                    var requestContent = new StringContent(
                        JsonConvert.SerializeObject(requestData),
                        Encoding.UTF8,
                        "application/json"
                    );

                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_firecrawlApiKey}");

                    var response = await client.PostAsync(_firecrawlApiUrl, requestContent);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<JObject>(responseString);
                   return result?["data"]?["markdown"]?.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ScrapeWebsiteAsync: {e.Message}");
                return null;
            }
        }

        private async Task<List<NewsItem>> ExtractDataFromMarkdownAsync(string markdownContent)
        {
            var newsItems = new List<NewsItem>();
            var claudePrompt = @"Analyze the following webpage content and extract information for multiple news articles. For each article, provide the news category, title, author, and published date. Return the information as a JSON array where each item has the following schema: { 'category': string, 'title': string, 'author': string, 'published_date': string }. Extract information for up to 20 articles if available. Return only the JSON array. Webpage content:" + markdownContent;
            
            try
            {
                using (var client = new HttpClient())
                {
                    var requestData = new
                    {
                        model = "claude-3-opus-20240229",
                        max_tokens = 1000,
                        messages = new[]
                        {
                            new { role = "user", content = claudePrompt }
                        }
                    };

                    var requestContent = new StringContent(
                        JsonConvert.SerializeObject(requestData),
                        Encoding.UTF8,
                        "application/json"
                    );

                    client.DefaultRequestHeaders.Add("x-api-key", _claudeApiKey);
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                    Console.WriteLine("Sending request to Claude API...");
                    var response = await client.PostAsync(_claudeApiUrl, requestContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Claude API request failed. Status code: {response.StatusCode}");
                        Console.WriteLine($"Error content: {errorContent}");
                        Console.WriteLine($"API Key used (first 4 characters): {_claudeApiKey.Substring(0, 4)}...");
                        throw new HttpRequestException($"Claude API request failed with status code: {response.StatusCode}");
                    }

                    var responseString = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<JObject>(responseString);
                    var content = result?["content"]?[0]?["text"]?.ToString();

                    if (content != null)
                    {
                        string jsonString = content.Substring(content.IndexOf("["));
                        var items = JsonConvert.DeserializeObject<List<JObject>>(content);

                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                var newsItem = new NewsItem
                                {
                                    Category = item["category"]?.ToString(),
                                    Title = item["title"]?.ToString(),
                                    Author = item["author"]?.ToString(),
                                    PublishedDate = DateTime.TryParse(item["published_date"]?.ToString(), out DateTime date) ? date : DateTime.Now,
                                };

                                newsItems.Add(newsItem);
                            }
                            Console.WriteLine($"Extracted {newsItems.Count} news items.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to extract content from Claude's response.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in ExtractDataFromMarkdownAsync: {e.Message}");
            }

            return newsItems;
        }
    }
}