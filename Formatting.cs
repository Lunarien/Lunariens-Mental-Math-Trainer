using System.Speech.Synthesis;
using NAudio.Wave;

namespace Lunariens_Mental_Math_Trainer
{
    public static class Formatting
    {
        public static SpeechSynthesizer GetUSVoice()
        {
            SpeechSynthesizer synth = new();
            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                if (voice.VoiceInfo.Culture.Name.Equals("en-US"))
                {
                    synth.SelectVoice(voice.VoiceInfo.Name);
                    return synth;
                }
            }
            Console.WriteLine("Unable to find en-US voice. Using default voice (not recommended at all unless your system language is some form of english).");
            return synth;
        }
        public static string AddCommas(string number)
        {
            if (number.Length <= 3)
                return number;
            for (int i = number.Length - 3; i >= 0; i -= 3)
            {
                number = number.Insert(i, ",");
            }
            return number;
        }
        public static string ToSuperScript(string number)
        {
            string sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string result = "";
            for (int i = 0; i < number.Length; i++)
            {
                result += sups[int.Parse(number[i].ToString())].ToString(); // this appends the superscript of the given number to the string result.
            }
            return result;
        }

        public static string ToSuperScript(int number)
        {
            string sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string result = "";
            for (int i = 0; i < number.ToString().Length; i++)
            {
                result += sups[int.Parse(number.ToString()[i].ToString())].ToString(); // this monstrosity appends the superscript of the given number to the string result.
            }
            return result;
        }
        public static string SuperscriptToNum(string sup)
        {
            string sups = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            string result = "";
            for (int i = 0; i < sup.Length; i++)
            {
                result += sups.IndexOf(sup[i]).ToString(); // this appends the number corresponding to the superscript to the string result.
            }
            return result;
        }

        /// <summary>
        /// Trims the end of an audio file if its volume is under a specified threshold.
        /// </summary>
        /// <param name="inputFilePath">Path to the input audio file.</param>
        /// <param name="outputFilePath">Path to save the trimmed audio file.</param>
        /// <param name="volumeThresholdDb">Volume threshold in decibels. Values lower represent quieter sounds.</param>
        public static void TrimAudioEnd(string inputFilePath, string outputFilePath, float volumeThresholdDb)
        {
            using var reader = new AudioFileReader(inputFilePath);
            float[] buffer = new float[1024];
            TimeSpan lastNonSilentPosition = reader.TotalTime;

            while (reader.Position < reader.Length)
            {
                int samplesRead = reader.Read(buffer, 0, buffer.Length);
                float volumeDb = GetRmsVolume(buffer, samplesRead);
                if (volumeDb > volumeThresholdDb)
                {
                    lastNonSilentPosition = reader.CurrentTime;
                }
            }

            using var writer = new WaveFileWriter(outputFilePath, reader.WaveFormat);
            reader.Position = 0;
            while (reader.CurrentTime <= lastNonSilentPosition && reader.Position < reader.Length)
            {
                int samplesRead = reader.Read(buffer, 0, buffer.Length);
                writer.WriteSamples(buffer, 0, samplesRead);
            }
        }

        private static float GetRmsVolume(float[] samples, int sampleCount)
        {
            double sum = 0;
            for (int i = 0; i < sampleCount; i++)
            {
                sum += samples[i] * samples[i];
            }
            double mean = sum / sampleCount;
            return 20 * (float)Math.Log10(Math.Sqrt(mean) + float.Epsilon);
        }
        
        public static string NumToWords(string number, char magnitudeSep)
        {
            string[] parts = number.Split(magnitudeSep);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].TrimStart('0');
            }
            if (parts.Length == 1)
            {
                return number;
            }
            string[] magnitudes = ["thousand", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion", "duodecillion", "tredecillion", "quattuordecillion", "quindecillion", "sexdecillion", "septendecillion", "octodecillion", "novemdecillion", "vigintillion", "unvigintillion", "duovigintillion", "trevigintillion", "quattuorvigintillion", "quinvigintillion", "sexvigintillion", "septenvigintillion", "octovigintillion", "novemvigintillion", "trigintillion", "untrigintillion", "duotrigintillion", "tretrigintillion", "quattuortrigintillion", "quintrigintillion", "sextrigintillion", "septentrigintillion", "octotrigintillion", "novemtrigintillion", "quadragintillion", "unquadragintillion", "duoquadragintillion", "trequadragintillion", "quattuorquadragintillion", "quinquadragintillion", "sexquadragintillion", "septenquadragintillion", "octoquadragintillion", "novemquadragintillion", "quinquagintillion", "unquinquagintillion", "duoquinquagintillion", "trequinquagintillion", "quattuorquinquagintillion", "quinquinquagintillion", "sexquinquagintillion", "septenquinquagintillion", "octoquinquagintillion", "novemquinquagintillion", "sexagintillion", "unsexagintillion", "duosexagintillion", "tresexagintillion", "quattuorsexagintillion", "quinsexagintillion", "sexsexagintillion", "septensexagintillion", "octosexagintillion", "novemsexagintillion", "septuagintillion", "unseptuagintillion", "duoseptuagintillion", "treseptuagintillion", "quattuorseptuagintillion", "quinseptuagintillion", "sexseptuagintillion", "septenseptuagintillion", "octoseptuagintillion", "novemseptuagintillion", "octogintillion", "unoctogintillion", "duooctogintillion", "treoctogintillion", "quattuoroctogintillion", "quinoctogintillion", "sexoctogintillion", "septenoctogintillion", "octooctogintillion", "novemoctogintillion", "nonagintillion", "unnonagintillion", "duononagintillion", "trenonagintillion", "quattuornonagintillion", "quinnonagintillion", "sexnonagintillion", "septennonagintillion", "octononagintillion", "novemnonagintillion", "centillion", "uncentillion", "duocentillion", "trecentillion", "quattuorcentillion", "quincentillion", "sexcentillion", "septencentillion", "octocentillion", "novemcentillion", "duocentillion", "treduocentillion", "quattuorduocentillion", "quinduocentillion", "sexduocentillion", "septenduocentillion", "octoduocentillion", "novemduocentillion", "trecentillion", "quattuortrecentillion", "quintrencentillion", "sextrencentillion", "septentrencentillion", "octotrencentillion", "novemtrencentillion"];
            List<string> words = [];
            int magnitudeIndex = parts.Length - 2;
            for (int i = 0; i < parts.Length; i++, magnitudeIndex--)
            {
                if (!string.IsNullOrWhiteSpace(parts[i]))
                {
                    words.Add(parts[i] + (magnitudeIndex >= 0 && magnitudeIndex < magnitudes.Length ? " " + magnitudes[magnitudeIndex] : ""));
                }
            }
            return string.Join(" ", words).Trim();
        }
        
        public static void GoodConsoleClear()
        {   // Clearing the console doesn't work well in case of Windows Terminal. This abomination is a workaround for that.
            Console.Clear();
            Console.WriteLine("\f\u001bc\x1b[3J");
            Console.Clear();
        }
    }
}