using System;
using System.Linq;
using Tesseract;
using System.IO;
using System.Drawing;

namespace HeistGemChecker.OCR
{
    /// <summary>
    /// Tesseract OCR magic
    /// </summary>
    internal class HeistOCR
    {
        public static readonly TesseractEngine TessEngine = new(@"./data", "eng", EngineMode.Default, configFile: "@./data/heist");
        public static readonly string[] GemTypes = new string[] { "Divergent", "Anomalous", "Phantasmal" };
        public static readonly string[] GemNames = File.ReadAllLines(@"./data/heist_gem_names.txt");

        public static string GetTextFromImage(Bitmap image)
        {
            using var page = TessEngine.Process(image);
            return page.GetText();
        }

        public static string[] GetGemsFromText(string text)
        {
            var _cleanedText = text
                .Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(line => GemTypes.Any(type => line.Contains(type, StringComparison.OrdinalIgnoreCase)));
            return GemNames.Where(name => _cleanedText.Any(line => line.Contains(name, StringComparison.OrdinalIgnoreCase))).ToArray();
        }

        public static string[] DetectHeistGems(Bitmap image)
        {
            var text = GetTextFromImage(image);
            return GetGemsFromText(text);
        }
    }
}
