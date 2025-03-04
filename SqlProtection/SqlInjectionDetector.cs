using System.Text.RegularExpressions;

namespace Shared.SqlProtection;

public static class SQLInjectionDetector
{
    // Patterns for detecting SQL keywords and patterns commonly used in SQL injection attacks in PostgreSQL.
    private static readonly string[] Patterns =
    {
        @"(--\s?\d?$)", // SQL single-line comment
        @"(;|--|\bOR\b|\bAND\b)", // SQL operators and keywords
        @"(\bUNION\b)", // UNION keyword
        @"(\bCOPY\b)", // COPY command, which can be used to read/write files
        @"(\bDROP\s+TABLE\b)", // DROP TABLE keyword
        @"(\bDELETE\s+FROM\b)", // DELETE FROM keyword
        @"(\bCAST\b)", // CAST function
        @"(\bEXECUTE\b)", // EXECUTE keyword
        @"(\bPG_SLEEP\b)", // PG_SLEEP function for time-based blind injection checks
        @"(lo_import|lo_export)", // Large Object operations that can be abused to read/write files
        @"(\bSET\s+ROLE\b)", // Changing role/user
    };

    public static bool ContainsPossibleInjection(string? input)
    {
        return !string.IsNullOrEmpty(input)
            && Patterns.Any(pattern => Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase));
    }
}
