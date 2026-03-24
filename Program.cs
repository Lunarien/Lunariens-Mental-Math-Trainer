using System.Diagnostics;
using System.Globalization;
using System.Speech.Synthesis;
using static Lunariens_Mental_Math_Trainer.Formatting;
using static Lunariens_Mental_Math_Trainer.Menus;

namespace Lunariens_Mental_Math_Trainer
{
    public static class Configuration // this is to be saved and loaded to/from a config file in the future
    {
        internal static int SpeechDelay = 500;
    }
    public static class SessionConfiguration
    {
        internal static int? problemCount = null;
    }

    public class Program
    {
        internal static Modes mode;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Stopwatch sw = new();
            IFormatProvider ifp = new CultureInfo("en-US");
            Console.ForegroundColor = ConsoleColor.White;
            SpeechSynthesizer synth = GetUSVoice();

            Console.WriteLine("Welcome to LMMT! (Lunarien's Mental Math Trainer)");
            ProgramLoop(ifp, sw, mode, synth);
        }
    }
}
