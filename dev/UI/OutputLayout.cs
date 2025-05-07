using ServerNetworkAPI.dev.Core;

namespace ServerNetworkAPI.dev.UI
{
    public class OutputRow
    {
        public int ConsoleRow { get; set; }
        public string Value { get; set; } = string.Empty;
    }

    public static class OutputLayout
    {
        private static readonly Dictionary<int, OutputRow> _rows = new();

        public static void Initialize()
        {
            Console.Clear();

            for (int i = 0; i < 25; i++)
            {
                Add(i, "#");
            }

            Add(25, "# <--   Status   -->");
            Add(26, "#");
            Add(27, "# Nmap Scan: ");
            Add(28, "# Total Scans: ");
            Add(29, "# Scanning...");
        }

        public static void UpdateRow(int row, string content)
        {
            if (_rows.TryGetValue(row, out var outputRow))
            {
                outputRow.Value = content;
                Console.SetCursorPosition(0, row);
                Console.Write(content.PadRight(Console.WindowWidth));
            }
        }

        private static void Add(int row, string content)
        {
            _rows[row] = new OutputRow { ConsoleRow = row, Value = content };
            Console.SetCursorPosition(0, row);
            Console.Write(content.PadRight(Console.WindowWidth));
        }

        public static void RedrawAll()
        {
            foreach (var (row, output) in _rows.OrderBy(r => r.Key))
            {
                Console.SetCursorPosition(0, row);
                Console.Write(output.Value.PadRight(Console.WindowWidth));
            }
        }

        public static void Clear()
        {
            Console.Clear();
            _rows.Clear();
        }
    }
}
