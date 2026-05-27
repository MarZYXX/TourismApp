using System.Net.Mail;

namespace appTurismo.Helpers
{
    public static class FormValidators
    {
        public static bool IsValidEmail(string? email)
        {
            var value = email?.Trim();
            if (string.IsNullOrWhiteSpace(value) || value.Contains(' '))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(value);
                var domain = address.Host;
                return string.Equals(address.Address, value, StringComparison.OrdinalIgnoreCase) &&
                       domain.Contains('.') &&
                       !domain.StartsWith('.') &&
                       !domain.EndsWith('.');
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsValidPhone(string? phone) =>
            !string.IsNullOrWhiteSpace(phone) &&
            phone.Trim().Length == 10 &&
            phone.Trim().All(char.IsDigit);

        public static bool IsValidPassword(string? password) =>
            !string.IsNullOrWhiteSpace(password) && password.Length >= 6;

        public static bool IsValidName(string? name, bool required = true)
        {
            var value = name?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return !required;
            }

            return value.All(character =>
                char.IsLetter(character) ||
                char.IsWhiteSpace(character) ||
                character is '-' or '\'');
        }
    }
}
