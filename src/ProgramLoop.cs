using static Lunariens_Mental_Math_Trainer.Formatting;
using static Lunariens_Mental_Math_Trainer.Helpers;
using static Lunariens_Mental_Math_Trainer.Modes;
using static Lunariens_Mental_Math_Trainer.FileHandler;
using static Lunariens_Mental_Math_Trainer.Training;
using System.Diagnostics;
using System.Speech.Synthesis;

namespace Lunariens_Mental_Math_Trainer
{
    internal static class Core
    {
        internal static void ProgramLoop(IFormatProvider ifp, Stopwatch sw, SpeechSynthesizer synth)
        {
            while (true) //when the program starts. this loop will ensure the existence of the main menu with its functionality.
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Main menu:");
                Console.WriteLine("1) Start training");
                Console.WriteLine("2) View a statistic graph from a list");
                Console.WriteLine("3) View a statistic graph for a specific problem type");
                Console.WriteLine("4) View console statistic from a list");
                Console.WriteLine("5) View console statistic for a specific problem type");
                Console.WriteLine("9) Exit LMMT");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Your choice: ");
                string usrChoice = Console.ReadLine();

                if (usrChoice == "1")
                {
                    GoodConsoleClear();
                    Modes mode = GetMode();
                    if (mode == Exit) //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }
                    GoodConsoleClear();

                    DigitCode[] usrDCs = [];
                    string? usrSessionDefinition = null;
                    bool gettingInput = true;

                    if (mode == Modes.Text || mode == Modes.Speech)
                    {
                        while (gettingInput)
                        {
                            usrSessionDefinition = GetSessionDefinitionStr();
                            if (usrSessionDefinition != null)
                            {
                                try
                                {
                                    Parser parser = new(usrSessionDefinition);
                                    usrDCs = parser.Parse(out SessionConfiguration.problemCount);
                                    gettingInput = false;
                                }
                                catch (FormatException e)
                                {
                                    GoodConsoleClear();
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine(e.Message);
                                    Console.ForegroundColor = ConsoleColor.White;
                                }
                            }
                            else
                            {
                                gettingInput = false;
                            }
                        }
                        if (usrSessionDefinition == null)
                        {
                            GoodConsoleClear();
                            continue;
                        }

                        foreach (DigitCode dc in usrDCs)
                        {
                            InitStatistic(dc.ToString(), mode);
                        }

                        OpenTrainingScreen(sw, ifp, usrDCs, synth, mode, SessionConfiguration.problemCount);
                    }
                    else if (mode == Modes.FlashAnzan)
                    {
                        FlashAnzanParameters flashParams = AnzanParamManager.Get();
                        FlashAnzan.OpenWindow(flashParams);
                    }
                }
                else if (usrChoice == "2") //view stats from a list
                {
                    GoodConsoleClear();
                    string[] files = Array.Empty<string>();
                    //check if there are any files. list them if so.
                    if (StatFolderExists())
                    {
                        files = Directory.GetFiles(GetStatDirectory());
                        for (int i = 0; i < files.Length; i++)
                        {
                            int fileNameIndex = files[i].LastIndexOf("\\") + 1;
                            files[i] = files[i][fileNameIndex..];
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    bool inOption2 = true;
                    while (inOption2)
                    {
                        if (files.Length != 0)
                        {
                            GoodConsoleClear();
                            Console.WriteLine("Existing statistics:");
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            for (int i = 0; i < files.Length; i++)
                            {
                                Console.WriteLine($"{i + 1}) {files[i]}");
                            }
                            Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Statistic to open: ");
                            string usrFileChoice = Console.ReadLine();

                            int fileMode = -1;
                            string file = files[int.Parse(usrFileChoice) - 1];
                            string filePath = Path.Combine(GetStatDirectory(), file);

                            // retrieve the mode from the file name. This is used in the OpenStatistic method below.
                            if (int.TryParse(usrFileChoice, out _) && int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                            {
                                int modeIndex = file.LastIndexOf("m") + 1;
                                fileMode = int.Parse(file[modeIndex].ToString());
                            }
                            else if (usrFileChoice == "exit")
                            {
                                GoodConsoleClear();
                                break;
                            }
                            else
                            {
                                GoodConsoleClear();
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Invalid choice, try again.");
                                Console.ForegroundColor = ConsoleColor.White;
                                if (inOption2)
                                {
                                    inOption2 = false;
                                    continue;
                                }
                                inOption2 = true;
                            }

                            if (int.TryParse(usrFileChoice, out int _))
                            {
                                if (int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                                {
                                    string[] statLines = File.ReadAllLines(filePath);
                                    if (statLines.Length == 1)
                                    {
                                        GoodConsoleClear();
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine("No statistics found inside the selected file.");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        continue;
                                    }
                                    string statDigitCode = file[..3];
                                    OpenStatisticGraph(statDigitCode, (Modes)fileMode);
                                    inOption2 = false;
                                }
                                else
                                {
                                    GoodConsoleClear();
                                    Console.WriteLine("Invalid choice, try again.");
                                }
                            }
                            else
                            {
                                GoodConsoleClear();
                                Console.WriteLine("Invalid choice, try again.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("There are no statistic files. Go calculate!");
                        }

                        GoodConsoleClear();
                    }
                }
                else if (usrChoice == "3")
                {
                    string[] files = Array.Empty<string>();
                    if (StatFolderExists())
                    {
                        files = Directory.GetFiles(GetStatDirectory());
                    }
                    else
                    {
                        GoodConsoleClear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    GoodConsoleClear();
                    Modes selectedMode = GetMode();
                    if (selectedMode == Exit) //if user wants to exit back to menu
                    {
                        GoodConsoleClear();
                        continue;
                    }

                    GoodConsoleClear();
                    DigitCode usrDC = new();
                    usrDC.Get();

                    OpenStatisticGraph(usrDC.ToString(), selectedMode);
                    GoodConsoleClear();
                }
                else if (usrChoice == "4") // view a list of digit code statistic files and let user choose which one to view
                {
                    GoodConsoleClear();
                    // list existing statistic files

                    string[] files = Array.Empty<string>();
                    if (StatFolderExists())
                    {
                        files = Directory.GetFiles(GetStatDirectory());
                        for (int i = 0; i < files.Length; i++)
                        {
                            int fileNameIndex = files[i].LastIndexOf("\\") + 1;
                            files[i] = files[i][fileNameIndex..];
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    if (files.Length != 0)
                    {
                        GoodConsoleClear();
                        Console.WriteLine("Existing statistics:");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        for (int i = 0; i < files.Length; i++)
                        {
                            int digitCodeStartIndex = files[i].LastIndexOf("\\") + 1;
                            Console.WriteLine($"{i + 1}) {files[i][digitCodeStartIndex..]}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("Statistic to open: ");
                        string usrFileChoice = Console.ReadLine();

                        Modes fileMode;

                        // retrieve the mode from the file name. This is used in the OpenStatisticScreen method below.
                        if (int.TryParse(usrFileChoice, out _) && int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                        {
                            string file = files[int.Parse(usrFileChoice) - 1];

                            int digitCodeStartIndex = file.LastIndexOf("\\") + 1;
                            file = file[digitCodeStartIndex..];
                            
                            int modeIndex = file.IndexOf("m") + 1;
                            fileMode = (Modes)int.Parse(file[modeIndex].ToString());
                        }
                        else if (usrFileChoice == "exit")
                        {
                            GoodConsoleClear();
                            continue;
                        }
                        else
                        {
                            GoodConsoleClear();
                            Console.WriteLine("Invalid choice, try again.");
                            Thread.Sleep(1000);
                            GoodConsoleClear();
                            continue;
                        }

                        if (int.TryParse(usrFileChoice, out int _))
                        {
                            if (int.Parse(usrFileChoice) <= files.Length && int.Parse(usrFileChoice) > 0)
                            {
                                string file = files[int.Parse(usrFileChoice) - 1];

                                int digitCodeStartIndex = file.LastIndexOf("\\") + 1;
                                int digitCodeEndIndex = file.LastIndexOf("m");
                                string statDigitCode = file[digitCodeStartIndex..digitCodeEndIndex];
                                GoodConsoleClear();
                                OpenStatisticScreen(statDigitCode, fileMode);
                            }
                            else
                            {
                                GoodConsoleClear();
                                Console.WriteLine("Invalid choice, try again.");
                            }
                        }
                    }


                }
                else if (usrChoice == "5") // view console statistics for specific digit code
                {
                    string[] files = Array.Empty<string>();
                    if (StatFolderExists())
                    {
                        files = Directory.GetFiles(GetStatDirectory());
                    }
                    else
                    {
                        GoodConsoleClear();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No statistics folder was detected! Go calculate or copy your previous one.");
                        Console.ForegroundColor = ConsoleColor.White;
                        continue;
                    }

                    GoodConsoleClear();
                    Modes selectedMode = GetMode();
                    if (selectedMode == Exit) //if user wants to exit
                    {
                        GoodConsoleClear();
                        continue;
                    }

                    GoodConsoleClear();
                    DigitCode usrDC = new();
                    usrDC.Get();

                    GoodConsoleClear();
                    OpenStatisticScreen(usrDC.ToString(), selectedMode);
                }
                else if (usrChoice == "9")
                {
                    Console.WriteLine("Exiting...");
                    Thread.Sleep(20);
                    Environment.Exit(0);
                }
                else
                {
                    GoodConsoleClear();
                    Console.WriteLine("Invalid choice, try again.");
                }
            }
        }
    }
}