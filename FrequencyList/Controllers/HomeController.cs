using FrequencyList.Models;
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
                string filePath = Path.Combine(uploadPath, file.FileName);
                string dictionaryFilePath = Path.Combine(dictionaryPath, dictionaryFileName + ".txt");

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

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

                    foreach (var entry in wordFrequencies)
                    {
                        if (mergedDictionary.ContainsKey(entry.Word))
                        {
                            mergedDictionary[entry.Word] += entry.Frequency;
                        }
                        else
                        {
                            mergedDictionary[entry.Word] = entry.Frequency;
                        }
                    }
                }
            }

            var sortedMergedDictionary = mergedDictionary
                .OrderByDescending(entry => entry.Value)
                .ToList();

            return View("MergedDictionaryResult", sortedMergedDictionary);
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

            using (var process = Process.Start(processStartInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Python Error: {error}");
                }
                else
                {
                    Console.WriteLine($"Python Output: {output}");
                }
            }
        }
    }
}
