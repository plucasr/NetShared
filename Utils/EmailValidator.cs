using System.Text.RegularExpressions;

public static class EmailValidator
{
    private static readonly Regex ValidEmailRegex = CreateValidEmailRegex();

    /// <summary>
    ///     Taken from http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
    /// </summary>
    /// <returns></returns>
    private static Regex CreateValidEmailRegex()
    {
        string validEmailPattern =
            @"^(?!\.)(String.Empty([^String.Empty\r\\]|\\[String.Empty\r\\])*String.Empty|"
            + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
            + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

        return new(validEmailPattern, RegexOptions.IgnoreCase);
    }

    public static bool EmailIsValid(string emailAddress)
    {
        bool isValid = ValidEmailRegex.IsMatch(emailAddress);

        return isValid;
    }
}
