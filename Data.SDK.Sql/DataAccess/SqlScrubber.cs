using System.Text.RegularExpressions;

namespace Tiddly.Sql.DataAccess
{
    public static class SqlScrubber
    {
        // Regex for detection of SQL meta-characters
        // /(\%27)|(\')|(\-\-)|(\%23)|(#)/ix
        // Explanation:
        // We first detect either the hex equivalent of the single-quote, the single-quote itself or the presence of the double-dash.These are SQL characters for MS SQL Server and Oracle, which denote the beginning of a comment, and everything that follows is ignored.

        // Modified regex for detection of SQL meta-characters
        // /((\%3D)|(=))[^\n]*((\%27)|(\')|(\-\-)|(\%3B)|(;))/i
        // Explanation:
        // This signature first looks out for the = sign or its hex equivalent(%3D). It then allows for zero or more non-newline characters, and then it checks for the single-quote, the double-dash or the semi-colon.

        // Regex for typical SQL Injection attack
        // /\w*((\%27)|(\'))((\%6F)|o|(\%4F))((\%72)|r|(\%52))/ix
        // Explanation:
        // \w* - zero or more alphanumeric or underscore characters
        // (\%27)|\' - the ubiquitous single-quote or its hex equivalent
        // (\%6F)|o|(\%4F))((\%72)|r|(\%52) - the word 'or' with various combinations of its upper and lower case hex equivalents.

        // Regex for detecting SQL Injection with the UNION keyword
        // /((\%27)|(\'))union/ix
        // Explanation
        // \%27)|(\') - the single-quote and its hex equivalent
        // union - the keyword union

        // Regex for detecting SQL Injection attacks on a MS SQL Server
        // /exec(\s|\+)+(s|x)p\w+/ix
        // Explanation:
        // exec - the keyword required to run the stored or extended procedure
        // (\s|\+)+ - one or more whitespaces or their HTTP encoded equivalents
        // (s|x)p - the letters 'sp' or 'xp' to identify stored or extended procedures respectively
        // \w+ - one or more alphanumeric or underscore characters to complete the name of the procedure

        private static readonly Regex MsSqlGeneric =
            new Regex(@"/((\%3D)|(=))[^\n]*((\%27)|(\')|(\-\-)|(\%3B)|(;))/i", RegexOptions.Compiled);


        // Or check : checks for both the actual letters and the html encoded values of the letters. It must have either a space or horizontal tab before and after the letters or encoded values
        //   ((\%09| |\%20)(o|\%4F|\%6F)(r|\%52|\%72)(\%09| |\%20))
        // And check – same deal, checks for both the letters and the html encoded values of the letters. Must have either a space or horizontal tab before and after the letters or encoded values
        //   (('|\';|\%27;|;)( |\%09|\%20|)(e|\%65|\%45)(x|\%78|\%58)(e|\%65|\%45)(c|\%63|\%43)( |\%20|))
        // Exec check – checks for each letter and html encoded value. Needs to be preceded by a ‘ or ‘;.
        //   (('|\';|\%27;|;)( |\%09|\%20|)(e|\%65|\%45)(x|\%78|\%58)(e|\%65|\%45)(c|\%63|\%43)( |\%20|))
        // Declare check – this checks for a more complicated form where a statement is encoded into a string of chars and then cast back. In short, checks for declare in any form followed by @
        //   (('|\';|\%27;|;)(d|\%64|\%44)(e|\%65|\%45)(c|\%63|\%43)(l|\%6C|\%4C)(a|\%61|\%41)(r|\%72|\%52)(e|\%65|\%45)( |\%20|\%09)(\@|\%40))
        // Check for --, ‘ , /* and *\. These are all terminating and comment characters
        //   ((\-\-)|(\/\*)|(\*\/)|('*))

        private static readonly Regex UberRegex = new Regex(
            @"(((\%09| |\%20)(o|\%4F|\%6F)(r|\%52|\%72)(\%09| |\%20))|(('|\%27|\%09| |\%20)(a|\%61|\%41)(n|\%6E|\%4E)(d|%64|%44)(\%09| |\%20))|(('|\';|\%27;|;)( |\%09|\%20|)(e|\%65|\%45)(x|\%78|\%58)(e|\%65|\%45)(c|\%63|\%43)( |\%20|))|(('|\';|\%27;|;)(d|\%64|\%44)(e|\%65|\%45)(c|\%63|\%43)(l|\%6C|\%4C)(a|\%61|\%41)(r|\%72|\%52)(e|\%65|\%45)( |\%20|\%09)(\@|\%40))|((\-\-)|(\/\*)|(\*\/)|('*)))");

        public static bool IsValid(string valueToTest)
        {
            return !MsSqlGeneric.IsMatch(valueToTest);
        }

        public static string ScrubString(string valueToScrub)
        {
            return UberRegex.Replace(valueToScrub, string.Empty);
        }
    }
}