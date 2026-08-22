using System.Numerics;
using static Lunariens_Mental_Math_Trainer.Formatting;
using Raylib_cs;
using System.Text.RegularExpressions;
using NAudio.Utils;

namespace Lunariens_Mental_Math_Trainer
{
    struct FlashAnzanParameters(int digits, int count, int flashDuration)
    {
        internal int digits = digits;
        internal int count = count;
        internal int flashInterval = flashDuration;
    }
    static class AnzanParamManager
    {
        static private MatchCollection ChunkTypes(string parameters)
        {
            Regex regex = new(@"\s*(\d+)\s*");
            MatchCollection matches = regex.Matches(parameters);
            if (matches.Count != 3)
            {
                throw new FormatException("Incorrect number of parameters entered!");
            }
            return matches;
        }

        static internal FlashAnzanParameters Get()
        {
            Console.WriteLine("Define Flash anzan parameters. Type \"help\" for more information.");
            while (true)
            {
                Console.Write(">>> ");
                string paramInput = Console.ReadLine();
                if (paramInput.ToLower() == "help")
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    using (StreamReader reader = new("./resources/fa-params-help.txt"))
                    {
                        Console.WriteLine(reader.ReadToEnd());
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    continue;
                }
                try
                {
                    MatchCollection chunks = ChunkTypes(paramInput);
                    Match[] chunksArray = chunks.ToArray();
                    int[] intParameters = new int[3];
                    for (int i = 0; i < 3; i++)
                    {
                        intParameters[i] = int.Parse(chunksArray[i].ToString());
                    }
                    var (digits, count, flashInterval) = (intParameters[0], intParameters[1], intParameters[2]);
                    FlashAnzanParameters anzanParams = new(digits, count, flashInterval);
                    return anzanParams;
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    enum WindowState
    {
        Start,
        Flashing,
        Answering
    }

    static class FlashAnzan
    {
        private static long[] GenerateSequence(int digitCount, int numberCount)
        {
            Random random = new();

            int minVal = (int)Math.Pow(10, digitCount - 1);
            int maxVal = (int)Math.Pow(10, digitCount);

            long[] numbers = new long[numberCount];
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(minVal, maxVal);
            }
            return numbers;
        }
        
        internal static void OpenWindow(FlashAnzanParameters parameters)
        {
            Raylib.InitWindow(960, 540, "Flash anzan window");
            Raylib.SetExitKey(KeyboardKey.X);
            Raylib.SetTargetFPS(60);
            Font font = Raylib.LoadFontEx("./resources/ABACUS.ttf", 200, null, 0);
            Raylib.GenTextureMipmaps(ref font.Texture);
            Raylib.SetTextureFilter(font.Texture, TextureFilter.Bilinear);

            Raylib.InitAudioDevice();
            Sound fxIntro = Raylib.LoadSound("resources/anzan intro.wav");
            Sound fxBlip = Raylib.LoadSound("resources/anzan number blip.wav");
            int lastBlip = -1;

            WindowState state = WindowState.Start;
            int schedulePosition = 0;
            double startWindowLife = 0;
            double recentStartWindowLife = 0; //more frequently updated/whenever the original startWindowLife is to be kept

            float remainingFadeTime = 0.0f;
            
            string num = "?";
            long[] numSequence = new long[parameters.count];
            long correctAnswer = 0;
            string userInput = "";
            bool? lastAnswerCorrect = null;

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                switch (state)
                {
                    case WindowState.Start:
                        Raylib.DrawTextEx(font, "[X] Exit ", new Vector2(0, 494), 45, 1, Color.White);
                        Raylib.DrawTextEx(font, "[Space] Start", new Vector2(320, 494), 45, 1, Color.White);

                        if (Raylib.IsKeyPressed(KeyboardKey.Space))
                        {
                            Raylib.SetExitKey(KeyboardKey.Null); // the flashes would have to be cancelled first before allowing exiting.
                            schedulePosition = 0;
                            lastBlip = -1;
                            userInput = "";
                            lastAnswerCorrect = null;

                            numSequence = GenerateSequence(parameters.digits, parameters.count);
                            correctAnswer = 0;
                            for (int i = 0; i < numSequence.Length; i++)
                            {
                                correctAnswer += numSequence[i];
                            }

                            num = numSequence[0].ToString();
                            state = WindowState.Flashing;

                            startWindowLife = Raylib.GetTime() * 1000;
                            recentStartWindowLife = startWindowLife; // marker A;
                            remainingFadeTime = 1500;
                            Raylib.PlaySound(fxIntro);
                        }
                        break;

                    case WindowState.Flashing:
                        double startDelay = 2000;
                        double elapsedFlashing = Raylib.GetTime() * 1000 - startWindowLife;
                        double currentFlashStart = parameters.flashInterval * schedulePosition + startDelay;
                        double showEnd = currentFlashStart + parameters.flashInterval * 2.0 / 3;
                        double nextStart = currentFlashStart + parameters.flashInterval;

                        if (remainingFadeTime > 0)
                        {
                            float opacity = remainingFadeTime / 1500;
                            Raylib.DrawTextEx(font, "[Backspace] Cancel", new Vector2(0, 494), 45, 1, Color.FromHSV(0, 0, opacity));
                            remainingFadeTime -= (float)(Raylib.GetTime() * 1000 - recentStartWindowLife); // see "marker A;" (last updated there), then "marker B;"
                            recentStartWindowLife = Raylib.GetTime() * 1000; // marker B;
                        }
                        //else: dont display anything. (because the text is fully black by now)

                        // display the num
                        if (elapsedFlashing >= currentFlashStart && elapsedFlashing < showEnd)
                        {
                            if (lastBlip < schedulePosition)
                            {
                                Raylib.PlaySound(fxBlip);
                                lastBlip = schedulePosition;
                            }
                            float fontSize = Math.Max(250 / num.Length, 60);
                            float textWidth = Raylib.MeasureTextEx(font, num, fontSize, 2).X;
                            float windowX = (Raylib.GetRenderWidth() - textWidth) / 2;
                            Raylib.DrawTextEx(font, num, new Vector2(windowX, 100), fontSize, 2, Color.White);
                        }

                        if (elapsedFlashing >= nextStart)
                        {
                            schedulePosition++;
                            if (schedulePosition >= parameters.count)
                            {
                                state = WindowState.Answering;
                                Raylib.SetExitKey(KeyboardKey.X);
                            }
                            else
                            {
                                num = numSequence[schedulePosition].ToString();
                            }
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.Backspace))
                        {
                            state = WindowState.Start;
                            Raylib.SetExitKey(KeyboardKey.X);
                        }

                        break;

                    case WindowState.Answering:
                        int key = Raylib.GetCharPressed();
                        while (key != 0)
                        {
                            if (key >= '0' && key <= '9' && userInput.Length < 20)
                                userInput += (char)key;
                            key = Raylib.GetCharPressed();
                        }

                        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && userInput.Length > 0)
                            userInput = userInput[..^1];
                        bool anyEnterPressed = Raylib.IsKeyPressed(KeyboardKey.Enter) || Raylib.IsKeyPressed(KeyboardKey.KpEnter);
                        if (anyEnterPressed && userInput.Length > 0)
                        {
                            long userAnswer = long.Parse(userInput);
                            lastAnswerCorrect = userAnswer == correctAnswer;
                            userInput = "";
                        }

                        if (lastAnswerCorrect != null)
                        {
                            string judgeText = lastAnswerCorrect.Value ? "Correct" : $"Wrong";
                            Color resultColor = lastAnswerCorrect.Value ? Color.Green : Color.Red;
                            float fontPosX = lastAnswerCorrect.Value ?
                                Raylib.GetScreenWidth() / 2 - Raylib.MeasureTextEx(font, "Correct", 100, 2).X / 2 :
                                Raylib.GetScreenWidth() / 2 - Raylib.MeasureTextEx(font,   "Wrong", 100, 2).X / 2;
                            Raylib.DrawTextEx(font, judgeText, new Vector2(fontPosX, 100), 100, 2, resultColor);

                            if (Raylib.IsKeyPressed(KeyboardKey.Space))
                            {
                                lastAnswerCorrect = null;
                                state = WindowState.Start;
                                Raylib.SetExitKey(KeyboardKey.X);
                            }
                            Raylib.DrawTextEx(font, "[X] Exit ", new Vector2(0, 494), 45, 1, Color.White);
                            Raylib.DrawTextEx(font, "[Space] Continue", new Vector2(320, 494), 45, 1, Color.White);
                        }
                        else
                        {
                            Raylib.DrawTextEx(font, "Your answer:", new Vector2(0, 100), 100, 2, Color.White);
                            Raylib.DrawTextEx(font, userInput, new Vector2(0, 250), 120, 2, Color.Yellow);
                            if (Raylib.GetTime() * 2 % 2 < 1)
                            {
                                float textWidth = Raylib.MeasureTextEx(font, userInput, 150, 2).X;
                                Raylib.DrawTextEx(font, "|", new Vector2(textWidth, 250), 150, 2, Color.Yellow);
                            }
                            Raylib.DrawTextEx(font, "[X] Exit ", new Vector2(0, 494), 45, 1, Color.White);
                            Raylib.DrawTextEx(font, "[Enter] Submit", new Vector2(320, 494), 45, 2, Color.White);
                        }
                        break;
                }

                Raylib.EndDrawing();
            }
            Raylib.UnloadFont(font);
            Raylib.UnloadSound(fxBlip);
            Raylib.UnloadSound(fxIntro);
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
            GoodConsoleClear();
        }
    }
}