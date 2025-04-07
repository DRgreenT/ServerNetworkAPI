namespace ServerNetworkAPI.dev
{
    public class OutputRow
    {
        public int consoleRow { get; set; }
        public string rowValue { get; set; } = string.Empty;
    }
    public class OutputManager
    {
        public static HashSet<int> outputRows = new();
        public static List<OutputRow> outputData = new();

        public OutputManager()
        {
            System.Console.Clear();
            Initialize();
        }

        public static List<OutputRow> GetList()
        {
            return outputData;
        }

        public static void ToOutputList(int row, string value)
        {
            if (!outputRows.Contains(row))
            {
                outputData.Add(new OutputRow
                {
                    consoleRow = row,
                    rowValue = value
                });
                outputRows.Add(row);
            }
        }

        public static void EditRow(int row, string value)
        {
            var target = outputData.FirstOrDefault(r => r.consoleRow == row);
            if (target != null)
            {
                target.rowValue = value;
            }
        }

        public void Initialize()
        {
            CreateHeader();
            CreateBody();
            CreateFooter();
            DEBUG_AREA();
        }
        private static void CreateHeader()
        {
            ToOutputList(0, (new string('#', 60)));
            ToOutputList(1, "# Server NetworkAPI Vers." + Init.version);
            ToOutputList(2, (new string('#', 60)));
            ToOutputList(3, (new string('#', 1)));
        }


        private static void CreateBody()
        {
            ToOutputList(4, "");
            for (int i = 5; i < Output.pingStatusRow - 2; i++)
            {
                ToOutputList(i, "# "); //Data
            }
            ToOutputList(Output.pingStatusRow - 2, "# "); // Reserve for line above status title
        }

        private static void CreateFooter()
        {
            ToOutputList(Output.pingStatusRow - 1, "# <--   Status   -->");
            ToOutputList(Output.pingStatusRow, "#"); // Ping ScanStatus %
            ToOutputList(Output.nmapStatusRow, "#"); // nmap ScanStatus %
            ToOutputList(Output.totalScanStatusRow, "#"); // TotalScans
            ToOutputList(Output.nextScanStatusRow, "#"); // Timer next scan
        }

        private static void DEBUG_AREA()
        {
            ToOutputList(Output.nextScanStatusRow + 2, "# DEBUG");
            ToOutputList(Output.nextScanStatusRow + 3, "#");
            ToOutputList(Output.nextScanStatusRow + 4, "#");
        }
    }

}
