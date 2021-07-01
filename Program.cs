using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CreativeNovelsScraper
{

    class Program
    {
        /// <summary>
        /// Get all the chapter from "https://creativenovels.com/novel/*
        /// </summary>
        /// <param name="link">Link from the novel</param>
        /// <returns>A list from all the chapters avalible for the novel ready to be filled and parsed</returns>
        private static IEnumerable<Chapter> GetChapters(string link)
        {
            if (!Regex.IsMatch(link, @"https://creativenovels.com/novel/.*?")) throw new Exception("Bad link, make sure it is ok, example: https://creativenovels.com/novel/the-villains-need-to-save-the-world");

            // Variables to be retrieved
            string response = null;

            HtmlWeb web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.97 Safari/537.36";
            HtmlDocument doc = web.Load(link);
            // Get Novel ID
            HtmlNode idNode = doc.DocumentNode.SelectSingleNode("//div[@id='chapter_list_novel_page']");
            string novelId = idNode.Attributes["class"].Value;

            //Get Chapter List
            using (var client = new HttpClient())
                response = client.PostAsync("https://creativenovels.com/wp-admin/admin-ajax.php",
                    new FormUrlEncodedContent(new[]
                    {   new KeyValuePair<string, string>("action", "crn_chapter_list"),
                        new KeyValuePair<string, string>("view_id", novelId)
                    }
                    )).Result.Content.ReadAsStringAsync().Result;

            //Parse Chapter List
            MatchCollection chapterMatches = Regex.Matches(response, @"(https.*?)\.data\.(.*?)\.data\.(.*?)\.data\.(available|locked)\.end_data\.");
            foreach (Match chapterMatch in chapterMatches)
            {
                string chapterTitle = System.Net.WebUtility.HtmlDecode(chapterMatch.Groups[2].Value);
                yield return new Chapter(chapterMatch.Groups[1].Value, chapterTitle);
            }

        }
        static void Main(string[] args)
        {
            int i;
            Console.WriteLine("Insert the novel link from CreativenNovels");
            var link = Console.ReadLine();
            var response = GetChapters(link).ToArray();
           
            for (i = 0; i < response.Length; ++i)
                Console.WriteLine($"ID: [{i}] {response[i].Title}");

            Console.WriteLine("Select Chapter ID for save the chapter");
            Console.WriteLine("Selec any invalid ID to exit");

            do
            {
                i = Convert.ToInt32(Console.ReadLine());
                response[i].FillContentAndParse();
                File.WriteAllText(response[i].Slug + ".html", response[i].Content);

            }
            while (i >= 0 && i < response.Length);
        }
    }
}
