using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Task04
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputUrl = Console.ReadLine();

            MainAsync(inputUrl);

            Console.ReadLine();
        }

        static async Task MainAsync(string inputUrl)
        {
            var listOfLinkedUrls = await GetLinkedUrlsAsync(inputUrl);
            if (listOfLinkedUrls.Count == 0)
            {
                Console.WriteLine($"Input URL {inputUrl} could not load " +
                    $"or it doesn't have any linked URLs.");
                return;
            }

            List<Task<int>> tasks = new List<Task<int>>();         
            foreach (var url in listOfLinkedUrls)
                tasks.Add(ProcessUrlAsync(url));

            await Task.WhenAll(tasks);
        }

        private static async Task<int> ProcessUrlAsync(string url)
        {
            int size = await GetUrlSizeAsync(url);
            DisplaySize(url, size);
            return size;
        }

        private static async Task<List<string>> GetLinkedUrlsAsync(string url)
        {
            string source = await ReadUrlAsync(url);

            var listOfLinks = new List<string>();
            if (source != null)
            {
                MatchCollection matches = Regex.Matches(source, @"<a href=""http(\S*)""");
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                        listOfLinks.Add(ExtractLink(match.Value));
                }
            }

            return listOfLinks;
        }

        private static async Task<string> ReadUrlAsync(string url)
        {
            string urlContent = null;
            WebRequest req = WebRequest.Create(url);
            req.Method = "GET";

            try
            {
                using (WebResponse response = await req.GetResponseAsync())
                {
                    using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
                    {
                        urlContent = await responseStream.ReadToEndAsync();
                    }
                }
            }
            catch (WebException)
            {
                //keep urlContent null
            }

            return urlContent;
        }

        private static async Task<int> GetUrlSizeAsync(string url)
        {
            string urlContent = await ReadUrlAsync(url);
            if (urlContent == null)
                return -1;
            return urlContent.Length;
        }

        private static void DisplaySize(string url, int size)
        {
            if (size == -1)
            {
                Console.WriteLine($"{url} couldn't be loaded.");
            }
            else
            {
                Console.WriteLine($"{url} has {size} symbols.");
            }
        }

        private static string ExtractLink(string s)
        {
            var match = Regex.Match(s, @"http(\S*)""");
            return match.Value.TrimEnd('\"');
        }
    }
}
