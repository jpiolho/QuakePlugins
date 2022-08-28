using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuakePlugins.Core
{
    internal static class Offsets
    {
        private const string OffsetFile = "offsets.json";

        private static Dictionary<string, nuint> _offsets;

        public static nuint GetOffset(string name) => _offsets[name];
        public static long GetOffsetLong(string name) => (long)_offsets[name];

        public static async Task LoadAsync(string root = "")
        {
            var path = Path.Combine(root, OffsetFile);

            var offsetsString = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllTextAsync(path));

            _offsets = new Dictionary<string, nuint>();
            foreach (var kv in offsetsString)
                _offsets[kv.Key] = nuint.Parse(kv.Value,NumberStyles.HexNumber);
        }
    }
}
