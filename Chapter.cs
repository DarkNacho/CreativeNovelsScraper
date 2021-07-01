using HtmlAgilityPack;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace CreativeNovelsScraper
{

    class ContentJson
    {
        [JsonPropertyName("rendered")]
        public string Rendered { get; set; }
        [JsonPropertyName("protected")]
        public bool IsProtected { get; set; } = false;
    }
    /// <summary>
    /// Minimal wrapper of content for getting the full chapter
    /// </summary>
    class ChapterJson
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("link")]
        public string Link { get; set; }
        [JsonPropertyName("title")]
        public ContentJson Title { get; set; } = new ContentJson();
        [JsonPropertyName("slug")]
        public string Slug { get; set; }
        [JsonPropertyName("content")]
        public ContentJson Content { get; set; } = new ContentJson();

    }
    class Chapter
    {
        private ChapterJson _chapter;
        public Chapter(string link, string title = "")
        {
            _chapter = new ChapterJson();
            Link = link;
            Title = title;
        }
        public string Link { get => _chapter.Link ; set => _chapter.Link = value; }
        public string Title { get => _chapter.Title.Rendered ; set => _chapter.Title.Rendered = value; }
        public string Slug { get => _chapter.Slug ; }
        public string Content { get => _chapter.Content.Rendered; }

        /// <summary>
        /// Get the real content of the chapter and parse it making it readable without a browser.
        /// </summary>
        /// <returns>The Chapter String Contnet</returns>
        public string FillContentAndParse()
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(Link).Result;
                var links = response.Headers.TryGetValues("Link", out var values) ? values.ToArray() : null;
                var l = Regex.Match(links[1], @"<(.*?)>").Value;
                l = l.Substring(1, l.Length - 2);
                
                //ask for real content to api..
                response = client.GetAsync(l).Result;
                _chapter = JsonSerializer.Deserialize<ChapterJson>((response.Content.ReadAsStringAsync().Result));

                /*
                var doc = new HtmlDocument();
                doc.LoadHtml(_chapter.Content.Rendered);
                _chapter.Content.Rendered = doc.DocumentNode.InnerText;
                */

                return Content;
                
            }
        }
    }
}
