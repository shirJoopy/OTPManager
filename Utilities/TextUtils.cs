namespace OTPManager.Utilities
{
    public static class TextUtils
    {
        public static string HideText(string text) {
            if (text == null || text.Length <= 3) return "***";
            return text.Substring(0, 2) + " "+new string('*', text.Length-4) + text.Substring(text.Length - 2);
        }
    }
}
