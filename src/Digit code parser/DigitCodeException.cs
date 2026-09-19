namespace Lunariens_Mental_Math_Trainer
{
    [Serializable]
    public class DigitCodeException : Exception
    {
        public DigitCodeException ()
        {}

        public DigitCodeException (string message) 
            : base(message)
        {}

    }
}