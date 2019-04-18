using System;
using System.Security.Cryptography;
using System.Text;

namespace Meziantou.PasswordManager.Client
{
    public class PasswordGenerator
    {
        private const string CharactersNumbers = "0123456789";
        private const string CharactersLowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string CharactersUppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string CharactersLowercaseLettersWithAccent = "àèìòùáéíóúýâêîôûãñõäëïöüÿåçðø¿¡ß";
        private const string CharactersUppercaseLettersWithAccent = "ÀÈÌÒÙÁÉÍÓÚÝÂÊÎÔÛÃÑÕÄËÏÖÜŸÅÇÐØ";
        private const string CharactersSymbols = @"!#$%&'()*+,-./:;<=>?@[\]_£¤~";

        private readonly RandomNumberGenerator _randomNumberGenerator = new RNGCryptoServiceProvider();

        public bool LowercaseLetters { get; set; } = true;
        public bool UppercaseLetters { get; set; } = true;
        public bool UppercaseLettersWithAccentMark { get; set; } = true;
        public bool LowercaseLettersWithAccentMark { get; set; } = true;
        public bool Numbers { get; set; } = true;
        public bool Symbols { get; set; } = true;
        public int Length { get; set; } = 16;

        private string GetCharset()
        {
            var result = "";
            if (Numbers)
                result += CharactersNumbers;

            if (LowercaseLetters)
                result += CharactersLowercaseLetters;

            if (UppercaseLetters)
                result += CharactersUppercaseLetters;

            if (LowercaseLettersWithAccentMark)
                result += CharactersLowercaseLettersWithAccent;

            if (UppercaseLettersWithAccentMark)
                result += CharactersUppercaseLettersWithAccent;

            if (Symbols)
                result += CharactersSymbols;

            return result;
        }

        public string Generate()
        {
            var charset = GetCharset();
            if (string.IsNullOrEmpty(charset))
                return string.Empty;

            var sb = new StringBuilder();
            for (var i = 0; i < Length; i++)
            {
                byte[] randomBytes = new byte[4];
                _randomNumberGenerator.GetBytes(randomBytes, 0, 4);
                var random = BitConverter.ToInt32(randomBytes, 0);
                var index = Math.Abs(random % charset.Length);

                sb.Append(charset[index]);
            }

            return sb.ToString();
        }
    }
}
