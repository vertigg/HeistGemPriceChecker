using HeistGemChecker.OCR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Runtime;
using Microsoft.Extensions.Caching.Memory;

namespace HeistGemChecker.src
{
    public class SkillGemsResponse
    {
        public List<SkillGemResponse>? Lines { get; set; }
    }

    public class SkillGemResponse
    {
        public string Name { get; set; }
        public float ChaosValue { get; set; }
        public float DivineValue { get; set; }
        public int GemLevel { get; set; }
        public int GemQuality { get; set; }
        public bool Corrupted { get; set; }
        public int ListingCount { get; set; }

    }

    internal class PoeNinja
    {

        private static readonly MemoryCache cache = new(new MemoryCacheOptions());

        public static async Task<SkillGemsResponse> FetchGemData(string league = "Kalandra")
        {
            using HttpClient client = new();
            return await client.GetFromJsonAsync<SkillGemsResponse>($"https://poe.ninja/api/data/itemoverview?league={league}&type=SkillGem");
        }

        public static async Task<List<SkillGemResponse>> GetApiGemData()
        {
            if (cache.TryGetValue("CachedFilteredGems", out List<SkillGemResponse> result))
            {
                return result;
            }
            else
            {
                var data = await FetchGemData();
                List<SkillGemResponse> res = data.Lines
                    .Where(item => HeistOCR.GemTypes.Any(type => item.Name.Contains(type, StringComparison.OrdinalIgnoreCase)))
                    .Where(item => !item.Corrupted)
                    .Where(item => item.GemLevel >= 1 & item.GemLevel <= 19)
                    .Where(item => item.GemQuality > 0)
                    .OrderByDescending(item => item.GemLevel)
                    .ThenByDescending(item => item.ListingCount)
                    .ToList();
                var ttl = DateTimeOffset.Now + TimeSpan.FromMinutes(10);
                cache.Set("CachedFilteredGems", res, ttl);
                return res;
            }
        }
    }
}
