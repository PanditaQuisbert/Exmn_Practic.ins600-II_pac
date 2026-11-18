namespace SistemaNotasColegio.Configuration;

public static class PasswordPolicy
{
    public const int MinimumLength = 8;
    public const int InitialSecretariaMinimumLength = 12;

    public static bool IsValid(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length >= MinimumLength;
    }

    public static bool IsStrong(string password)
    {
        return !string.IsNullOrWhiteSpace(password)
            && password.Length >= InitialSecretariaMinimumLength
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(caracter => !char.IsLetterOrDigit(caracter) && !char.IsWhiteSpace(caracter));
    }
}
