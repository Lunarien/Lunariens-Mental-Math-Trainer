using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using ConsoleTables;
using static Lunariens_Mental_Math_Trainer.Formatting;

namespace Lunariens_Mental_Math_Trainer
{
    public enum Modes
    {
        Exit = -1,
        Text = 0,
        Speech = 1
    }
    public static class FileHandler
    {
        public class Statistic
        {
            public string? Problem { get; set; }
            public string? UsrSolution { get; set; }
            public double SolveTime { get; set; }
            public DateTime Date { get; set; }
            public bool Correctness { get; set; }
        }
        public static void InitStatistic(string digitCode, Modes mode)
        {
            if (digitCode.Contains('/'))
            {
                digitCode = digitCode.Replace("/", "÷");
            }
            else if (digitCode.Contains('*'))
            {
                digitCode = digitCode.Replace("*", "x");
            }
            string dirPath = "../stats/";
            string path = $@"{dirPath}{digitCode}m{(int)mode}.csv";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            if (File.Exists(path)) return;


            StreamWriter sw = new(path, true);
            var csv = new CsvWriter(sw, CultureInfo.InvariantCulture);
            csv.WriteHeader<Statistic>();
            csv.NextRecord();
            csv.Flush();
            sw.Close();
        }

        public static void SaveStatistic(string statName, string problem, string usrSolution, double time, DateTime date, bool correctness, Modes mode)
        {
            string path = $@"..\stats\{statName}m{(int)mode}.csv";
            path = path.Replace('*', 'x'); //replace * with x because windows doesn't allow * in file names

            path = path.Replace("/", "÷");
            var record = new Statistic { Problem = problem.Replace("\n", ""), UsrSolution = usrSolution, SolveTime = time, Date = date, Correctness = correctness };

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = false,
            };

            using (var stream = File.Open(path, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecord<Statistic>(record);
                csv.NextRecord();
            }
        }
        public static void OpenStatisticGraph(string digitCode, Modes mode)
        {
            if (digitCode.Contains('/'))
            {
                digitCode = digitCode.Replace('/', '÷');
            }
            else if (digitCode.Contains('*'))
            {
                digitCode = digitCode.Replace('*', 'x');
            }

            string path = $@"../stats/{digitCode}m{(int)mode}.csv";
            if (!File.Exists(path))
            {
                Console.WriteLine("No statistic found for the selected digit code.");
                return;
            }

            var reader = new StreamReader(path);
            var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Statistic>().ToList();
            var plt = new Plot(900, 600);

            List<double> solveTimes = records.Select(record => Math.Round(record.SolveTime, 3)).ToList();
            List<bool> solveCorrectnesses = records.Select(record => record.Correctness).ToList();

            var signal = plt.AddSignal(solveTimes.ToArray());
            signal.LineWidth = 2;
            signal.MarkerSize = 3;
            signal.Color = System.Drawing.ColorTranslator.FromHtml("#bf616a");


            for (int i = 0; i < solveCorrectnesses.Count; i++)
            {
                if (solveCorrectnesses[i] == false)
                {
                    var hSpan = plt.AddHorizontalSpan(i - 0.25, i + 0.25);
                    hSpan.Color = System.Drawing.Color.FromArgb(128, 191, 97, 106);

                }
            }

            var modeName = mode.ToString();
            if (solveTimes.Count == 0)
            {
                return;
            }
            plt.YAxis.SetBoundary(0, 1.1 * solveTimes.Max());
            plt.XAxis.SetBoundary(-0.5, solveTimes.Count + 0.5);
            plt.Title("Solve times for " + digitCode + " (mode " + modeName + ")");
            plt.YLabel("Solve time (s)");

            plt.XAxis.Ticks(false);
            plt.XAxis.Line(false);
            plt.YAxis2.Line(false);
            plt.XAxis2.Line(false);

            plt.Palette = ScottPlot.Palette.OneHalfDark;
            plt.Style(ScottPlot.Style.Gray1);
            var bnColor = System.Drawing.ColorTranslator.FromHtml("#2e3440");
            plt.Style(figureBackground: bnColor, dataBackground: bnColor);


            var pltWindow = new ScottPlot.FormsPlotViewer(plt, 900, 600, "Solve times for " + digitCode + " (mode " + modeName + ")");
            pltWindow.BackColor = bnColor;
            pltWindow.ShowDialog();
        }

        public static void OpenStatisticScreen(string digitCode, Modes mode)
        {
            if (digitCode.Contains('/')) //replace the slash with a different character so the file can be found. the file name cannot contain a slash.
            {
                digitCode = digitCode.Replace("/", "÷");
            }
            else if (digitCode.Contains('*')) //replace the asterisk with a different character so the file can be found. the file name cannot contain an asterisk.
            {
                digitCode = digitCode.Replace("*", "x");
            }

            string path = $@"../stats/{digitCode}m{(int)mode}.csv";
            if (!File.Exists(path))
            {
                Console.WriteLine("No statistic found for the selected digit code.");
                return;
            }

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Statistic>();
                Statistic[] recordsArray = records.ToList().ToArray();
                // menu for viewing the statistics follows.

                //Print out the statistics in a table.
                ConsoleTable table = new("Problem", "Your solution", "Solve time", "Date", "Correct?");
                foreach (Statistic record in recordsArray)
                {
                    string boolWord;
                    if (record.Correctness == true)
                    {
                        boolWord = "Yes";
                    }
                    else
                    {
                        boolWord = "No";
                    }

                    table.AddRow(record.Problem, record.UsrSolution, record.SolveTime.ToString() + "s", record.Date.ToString(), boolWord);
                }
                table.Write();
            }
            Console.Write("Press any key to go back to the main menu...");
            Console.ReadKey();

            GoodConsoleClear();
            return;
        }
    }
}