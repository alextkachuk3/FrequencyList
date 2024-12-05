using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FrequencyList.Controllers
{
    public class HomeController : Controller
    {
        private readonly string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        private readonly string dictionaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Dictionaries");

        public HomeController()
        {
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            if (!Directory.Exists(dictionaryPath)) Directory.CreateDirectory(dictionaryPath);
        }

        public IActionResult Index()
        {
            var files = Directory.GetFiles(dictionaryPath).Select(Path.GetFileName).ToList();
            return View(files);
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file, string dictionaryFileName)
        {
            if (file != null && file.Length > 0 && !string.IsNullOrWhiteSpace(dictionaryFileName))
            {
                string filePath = Path.Combine(uploadPath, dictionaryFileName + ".txt");
                string dictionaryFilePath = Path.Combine(dictionaryPath, dictionaryFileName + ".txt");

                using (var stream = new FileStream(filePath, FileMode.Create))     
                    file.CopyTo(stream);

                RunPythonScript(filePath, dictionaryFilePath);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ViewDictionaries(string[] selectedFiles)
        {
            if (selectedFiles == null || selectedFiles.Length == 0)
                return RedirectToAction("Index");

            var mergedDictionary = new Dictionary<string, int>();

            foreach (var file in selectedFiles)
            {
                string dictionaryFilePath = Path.Combine(dictionaryPath, file);

                if (System.IO.File.Exists(dictionaryFilePath))
                {
                    var wordFrequencies = System.IO.File.ReadAllLines(dictionaryFilePath)
                        .Select(line => line.Split('\t'))
                        .Where(parts => parts.Length == 2)
                        .Select(parts => (Word: parts[0], Frequency: int.Parse(parts[1])));

                    foreach (var (Word, Frequency) in wordFrequencies)
                    {
                        if (mergedDictionary.ContainsKey(Word))
                            mergedDictionary[Word] += Frequency;
                        else
                            mergedDictionary[Word] = Frequency;
                    }
                }
            }

            var sortedMergedDictionary = mergedDictionary
                .OrderByDescending(entry => entry.Value)
                .ToList();

            return View("FrequencyList", sortedMergedDictionary);
        }

        public IActionResult GenerateFrequencyDictionary(string[] selectedFiles, bool inverted = false, bool lemmatized = false)
        {
            if (selectedFiles == null || selectedFiles.Length == 0)
                return RedirectToAction("Index");

            var dictionary = new Dictionary<string, int>();

            foreach (var file in selectedFiles)
            {
                string filePath;

                if (lemmatized)
                {
                    filePath = Path.Combine(dictionaryPath, file);
                }
                else
                {
                    filePath = Path.Combine(uploadPath, file);
                }

                if (System.IO.File.Exists(filePath))
                {
                    IEnumerable<(string Word, int Frequency)> wordFrequencies;

                    if (lemmatized)
                    {
                        wordFrequencies = System.IO.File.ReadAllLines(filePath)
                            .Select(line => line.Split('\t'))
                            .Where(parts => parts.Length == 2)
                            .Select(parts => (Word: parts[0], Frequency: int.Parse(parts[1])));
                    }
                    else
                    {
                        var text = System.IO.File.ReadAllText(filePath);

                        var cleanedText = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-zA-Zа-яА-Я0-9'\s]", "");

                        var words = cleanedText.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        wordFrequencies = words
                            .GroupBy(word => word.ToLower())
                            .Select(group => (Word: group.Key, Frequency: group.Count()));
                    }

                    foreach (var (Word, Frequency) in wordFrequencies)
                    {
                        if (dictionary.ContainsKey(Word))
                            dictionary[Word] += Frequency;
                        else
                            dictionary[Word] = Frequency;
                    }
                }
            }

            var sortedDictionary = dictionary.OrderByDescending(entry => entry.Value).ToList();

            if (inverted)
            {
                var invertedDictionary = sortedDictionary
                    .GroupBy(kv => kv.Value)
                    .ToDictionary(
                        group => group.Key,
                        group => string.Join(", ", group.Select(kv => kv.Key))
                    );

                return View("InvertedFrequencyList", invertedDictionary);
            }
            
            return View("FrequencyList", sortedDictionary);
        }

        private void RunPythonScript(string inputFilePath, string outputFilePath)
        {
            string stopwordsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "stopwords_ua.txt");
            string scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "text_processor.py");
            string arguments = $"\"{scriptPath}\" \"{inputFilePath}\" \"{outputFilePath}\" \"{stopwordsFilePath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            process?.WaitForExit();
        }
    }
}
