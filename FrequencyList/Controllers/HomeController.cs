using FrequencyList.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace FrequencyList.Controllers
{
    public class HomeController : Controller
    {
        private readonly string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        public HomeController()
        {
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
        }

        public IActionResult Index()
        {
            var files = Directory.GetFiles(uploadPath).Select(Path.GetFileName).ToList();
            return View(files);
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                string filePath = Path.Combine(uploadPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GenerateFrequency(string[] selectedFiles)
        {
            if (selectedFiles == null || selectedFiles.Length == 0)
            {
                return RedirectToAction("Index");
            }

            Dictionary<string, int> wordCounts = new Dictionary<string, int>();

            foreach (var file in selectedFiles)
            {
                string filePath = Path.Combine(uploadPath, file);
                if (System.IO.File.Exists(filePath))
                {
                    string content = System.IO.File.ReadAllText(filePath);
                    var words = Regex.Split(content, @"\W+")
                                     .Where(w => !string.IsNullOrEmpty(w))
                                     .Select(w => w.ToLower());
                    foreach (var word in words)
                    {
                        if (wordCounts.ContainsKey(word))
                            wordCounts[word]++;
                        else
                            wordCounts[word] = 1;
                    }
                }
            }

            var frequencies = wordCounts
                .Select(wc => new WordFrequency { Word = wc.Key, Frequency = wc.Value })
                .OrderByDescending(w => w.Frequency)
                .ToList();

            return View("FrequencyResult", frequencies);
        }
    }

}
