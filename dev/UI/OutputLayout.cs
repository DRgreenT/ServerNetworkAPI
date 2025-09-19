using ServerNetworkAPI.dev.Core;
using ServerNetworkAPI.dev.Services;
using System.Drawing;

namespace ServerNetworkAPI.dev.UI
{
    public class OutputRow
    {
        public int ConsoleRow { get; set; }
        public string Value { get; set; } = string.Empty;
        public ConsoleColor color { get; set; } = ConsoleColor.Gray;
    }

    public static class OutputLayout
    {
        private static readonly Dictionary<int, OutputRow> _rows = new();

        public static void Initialize()
        {
            if (SystemInfoService.IsHeadlessModeFromArgs)
                return;

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

        public static void UpdateRow(int row, string content, ConsoleColor? color)
        {
            if (row == 23)
            {
                ScrollMessageRows();
                WriteRow(row, content, color ?? ConsoleColor.Gray);
            }
            else
            {
                WriteRow(row, content, color ?? ConsoleColor.Gray);
            }
        }
        private static void ScrollMessageRows()
        {
            for (int i = 6; i < 23; i++)
            {
                if (_rows.TryGetValue(i + 1, out var nextRow) && _rows.TryGetValue(i, out var currentRow))
                {
                    currentRow.Value = nextRow.Value;
                    currentRow.color = nextRow.color;
                    DrawRow(i, currentRow.Value, currentRow.color);
                }
            }
        }

        private static void WriteRow(int row, string content, ConsoleColor color)
        {
            if (_rows.TryGetValue(row, out var outputRow))
            {
                outputRow.Value = content;
                outputRow.color = color;
                DrawRow(row, content, color);
            }
        }

        private static void DrawRow(int row, string content, ConsoleColor color)
        {
            if (SystemInfoService.IsHeadlessModeFromArgs)
                return;
            Console.SetCursorPosition(0, row);
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = color;
            Console.Write(content.PadRight(Console.WindowWidth));
            Console.ForegroundColor = originalColor;
        }


        private static void Add(int row, string content)
        {
            if (SystemInfoService.IsHeadlessModeFromArgs)
                return;
            _rows[row] = new OutputRow { ConsoleRow = row, Value = content };
            Console.SetCursorPosition(0, row);
            Console.Write(content.PadRight(Console.WindowWidth));
        }

        //public static void RedrawAll()
        //{
        //    foreach (var (row, output) in _rows.OrderBy(r => r.Key))
        //    {
        //        Console.SetCursorPosition(0, row);
        //        Console.Write(output.Value.PadRight(Console.WindowWidth));
        //    }
        //}

        //public static void Clear()
        //{
        //    Console.Clear();
        //    _rows.Clear();
        //}
    }
}
