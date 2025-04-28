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
            if (!AppConfig.ConsoleUserInterface)
            {
                return;
            }
            Console.Clear();
            Add(0, "");
            Add(1, "");
            Add(2, "");
            Add(3, "");

            for (int i = 4; i < 20; i++)
            {
                Add(i, "#");
            }

            Add(20, "# <--   Status   -->");
            Add(21, "#");
            Add(22, "# Nmap Scan: ");
            Add(23, "# Total Scans: ");
            Add(24, "# Scanning...");
        }

        public static void UpdateRow(int row, string content)
        {
            if (!AppConfig.ConsoleUserInterface)
            {
                return;
            }
            if (_rows.TryGetValue(row, out var outputRow))
            {
                outputRow.Value = content;
                Console.SetCursorPosition(0, row);
                Console.Write(content.PadRight(Console.WindowWidth));
            }
        }

        private static void Add(int row, string content)
        {
            if (!AppConfig.ConsoleUserInterface)
            {
                return;
            }
            _rows[row] = new OutputRow { ConsoleRow = row, Value = content };
            Console.SetCursorPosition(0, row);
            Console.Write(content.PadRight(Console.WindowWidth));
        }

        public static void RedrawAll()
        {
            if (!AppConfig.ConsoleUserInterface)
            {
                return;
            }
            foreach (var (row, output) in _rows.OrderBy(r => r.Key))
            {
                Console.SetCursorPosition(0, row);
                Console.Write(output.Value.PadRight(Console.WindowWidth));
            }
        }

        public static void Clear()
        {
            if (!AppConfig.ConsoleUserInterface)
            {
                return;
            }
            Console.Clear();
            _rows.Clear();
        }
    }
}
