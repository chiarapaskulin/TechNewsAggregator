using System;

namespace TechNewsAggregator.Models
{
    public class NewsItem
    {
        public string Category { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? PublishedDate { get; set; }

        public NewsItem()
        {
            Category = string.Empty;
            Title = string.Empty;
            Author = string.Empty;
        }
    }
}