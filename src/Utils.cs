using HeistGemChecker.src;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace HeistGemChecker
{
    internal class Utils
    {
        private static void Show(string message)
        {
            MessageBox.Show(message, "Heist Gem Detector", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }
        public static void ShowNotification(string message)
        {
            Show(message);
        }

        public static void ShowNotification(string[] results)
        {
            Show(string.Join("\n", results));
        }

        private static string FormattedPriceString(IGrouping<string, SkillGemResponse> group)
        {
            return string.Join("\n", group.Select(gem => $"Level {gem.GemLevel} {gem.GemQuality}q: {gem.ChaosValue} chaos, {gem.DivineValue} divines"));
        }

        public static void ShowNotification(List<SkillGemResponse> results)
        {
            Dictionary<string, string> messageGroups = results
                .GroupBy(item => item.Name)
                .Select(group => new { K = group.First().Name, V = FormattedPriceString(group)})
                .ToDictionary(t => t.K, t => t.V);

            Show(string.Join("\n\n", messageGroups.Select(item => item.Key + "\n" + item.Value).ToArray()));
        }
    }
}
