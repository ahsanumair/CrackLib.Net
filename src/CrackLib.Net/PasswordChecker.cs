namespace CrackLib.Net
{
    using System.Collections.Generic;
    using System.Linq;
    public interface IPasswordChecker
    {
        bool MatchPasswordAndRawText(string rawtext, string password);
        ISet<StatusCode> Check(string password);
    }

    public class PasswordChecker : IPasswordChecker
    {
        private const string IndexFilesName = "words.pack";

        private string _path;
        private string _wordList;

        /// <summary>
        /// Minimum number of different characters. </summary>
        public const int MinDiff = 5;

        /// <summary>
        /// Minimum password length. </summary>
        public const int MinLength = 8;

        /// <summary>
        /// Transformation rules. </summary>
        internal static readonly string[] Destructors = {
            // noop - must do this to test raw word.
            ":",
            /* trimming leading/trailing junk */
            "[",
            "]",
            "[[",
            "]]",
            "[[[",
            "]]]",
            /* purging out punctuation/symbols/junk */
            "/?p@?p",
            "/?s@?s",
            "/?X@?X",
            /* attempt reverse engineering of password strings */
            "/$s$s",
            "/$s$s/0s0o",
            "/$s$s/0s0o/2s2a",
            "/$s$s/0s0o/2s2a/3s3e",
            "/$s$s/0s0o/2s2a/3s3e/5s5s",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/1s1i",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/1s1l",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/1s1i/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/1s1i/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/1s1l/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/1s1l/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/5s5s/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/1s1i",
            "/$s$s/0s0o/2s2a/3s3e/1s1l",
            "/$s$s/0s0o/2s2a/3s3e/1s1i/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/1s1i/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/1s1l/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/1s1l/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/4s4h",
            "/$s$s/0s0o/2s2a/3s3e/4s4a",
            "/$s$s/0s0o/2s2a/3s3e/4s4h",
            "/$s$s/0s0o/2s2a/5s5s",
            "/$s$s/0s0o/2s2a/5s5s/1s1i",
            "/$s$s/0s0o/2s2a/5s5s/1s1l",
            "/$s$s/0s0o/2s2a/5s5s/1s1i/4s4a",
            "/$s$s/0s0o/2s2a/5s5s/1s1i/4s4h",
            "/$s$s/0s0o/2s2a/5s5s/1s1l/4s4a",
            "/$s$s/0s0o/2s2a/5s5s/1s1l/4s4h",
            "/$s$s/0s0o/2s2a/5s5s/4s4a",
            "/$s$s/0s0o/2s2a/5s5s/4s4h",
            "/$s$s/0s0o/2s2a/5s5s/4s4a",
            "/$s$s/0s0o/2s2a/5s5s/4s4h",
            "/$s$s/0s0o/2s2a/1s1i",
            "/$s$s/0s0o/2s2a/1s1l",
            "/$s$s/0s0o/2s2a/1s1i/4s4a",
            "/$s$s/0s0o/2s2a/1s1i/4s4h",
            "/$s$s/0s0o/2s2a/1s1l/4s4a",
            "/$s$s/0s0o/2s2a/1s1l/4s4h",
            "/$s$s/0s0o/2s2a/4s4a",
            "/$s$s/0s0o/2s2a/4s4h",
            "/$s$s/0s0o/2s2a/4s4a",
            "/$s$s/0s0o/2s2a/4s4h",
            "/$s$s/0s0o/3s3e",
            "/$s$s/0s0o/3s3e/5s5s",
            "/$s$s/0s0o/3s3e/5s5s/1s1i",
            "/$s$s/0s0o/3s3e/5s5s/1s1l",
            "/$s$s/0s0o/3s3e/5s5s/1s1i/4s4a",
            "/$s$s/0s0o/3s3e/5s5s/1s1i/4s4h",
            "/$s$s/0s0o/3s3e/5s5s/1s1l/4s4a",
            "/$s$s/0s0o/3s3e/5s5s/1s1l/4s4h",
            "/$s$s/0s0o/3s3e/5s5s/4s4a",
            "/$s$s/0s0o/3s3e/5s5s/4s4h",
            "/$s$s/0s0o/3s3e/5s5s/4s4a",
            "/$s$s/0s0o/3s3e/5s5s/4s4h",
            "/$s$s/0s0o/3s3e/1s1i",
            "/$s$s/0s0o/3s3e/1s1l",
            "/$s$s/0s0o/3s3e/1s1i/4s4a",
            "/$s$s/0s0o/3s3e/1s1i/4s4h",
            "/$s$s/0s0o/3s3e/1s1l/4s4a",
            "/$s$s/0s0o/3s3e/1s1l/4s4h",
            "/$s$s/0s0o/3s3e/4s4a",
            "/$s$s/0s0o/3s3e/4s4h",
            "/$s$s/0s0o/3s3e/4s4a",
            "/$s$s/0s0o/3s3e/4s4h",
            "/$s$s/0s0o/5s5s",
            "/$s$s/0s0o/5s5s/1s1i",
            "/$s$s/0s0o/5s5s/1s1l",
            "/$s$s/0s0o/5s5s/1s1i/4s4a",
            "/$s$s/0s0o/5s5s/1s1i/4s4h",
            "/$s$s/0s0o/5s5s/1s1l/4s4a",
            "/$s$s/0s0o/5s5s/1s1l/4s4h",
            "/$s$s/0s0o/5s5s/4s4a",
            "/$s$s/0s0o/5s5s/4s4h",
            "/$s$s/0s0o/5s5s/4s4a",
            "/$s$s/0s0o/5s5s/4s4h",
            "/$s$s/0s0o/1s1i",
            "/$s$s/0s0o/1s1l",
            "/$s$s/0s0o/1s1i/4s4a",
            "/$s$s/0s0o/1s1i/4s4h",
            "/$s$s/0s0o/1s1l/4s4a",
            "/$s$s/0s0o/1s1l/4s4h",
            "/$s$s/0s0o/4s4a",
            "/$s$s/0s0o/4s4h",
            "/$s$s/0s0o/4s4a",
            "/$s$s/0s0o/4s4h",
            "/$s$s/2s2a",
            "/$s$s/2s2a/3s3e",
            "/$s$s/2s2a/3s3e/5s5s",
            "/$s$s/2s2a/3s3e/5s5s/1s1i",
            "/$s$s/2s2a/3s3e/5s5s/1s1l",
            "/$s$s/2s2a/3s3e/5s5s/1s1i/4s4a",
            "/$s$s/2s2a/3s3e/5s5s/1s1i/4s4h",
            "/$s$s/2s2a/3s3e/5s5s/1s1l/4s4a",
            "/$s$s/2s2a/3s3e/5s5s/1s1l/4s4h",
            "/$s$s/2s2a/3s3e/5s5s/4s4a",
            "/$s$s/2s2a/3s3e/5s5s/4s4h",
            "/$s$s/2s2a/3s3e/5s5s/4s4a",
            "/$s$s/2s2a/3s3e/5s5s/4s4h",
            "/$s$s/2s2a/3s3e/1s1i",
            "/$s$s/2s2a/3s3e/1s1l",
            "/$s$s/2s2a/3s3e/1s1i/4s4a",
            "/$s$s/2s2a/3s3e/1s1i/4s4h",
            "/$s$s/2s2a/3s3e/1s1l/4s4a",
            "/$s$s/2s2a/3s3e/1s1l/4s4h",
            "/$s$s/2s2a/3s3e/4s4a",
            "/$s$s/2s2a/3s3e/4s4h",
            "/$s$s/2s2a/3s3e/4s4a",
            "/$s$s/2s2a/3s3e/4s4h",
            "/$s$s/2s2a/5s5s",
            "/$s$s/2s2a/5s5s/1s1i",
            "/$s$s/2s2a/5s5s/1s1l",
            "/$s$s/2s2a/5s5s/1s1i/4s4a",
            "/$s$s/2s2a/5s5s/1s1i/4s4h",
            "/$s$s/2s2a/5s5s/1s1l/4s4a",
            "/$s$s/2s2a/5s5s/1s1l/4s4h",
            "/$s$s/2s2a/5s5s/4s4a",
            "/$s$s/2s2a/5s5s/4s4h",
            "/$s$s/2s2a/5s5s/4s4a",
            "/$s$s/2s2a/5s5s/4s4h",
            "/$s$s/2s2a/1s1i",
            "/$s$s/2s2a/1s1l",
            "/$s$s/2s2a/1s1i/4s4a",
            "/$s$s/2s2a/1s1i/4s4h",
            "/$s$s/2s2a/1s1l/4s4a",
            "/$s$s/2s2a/1s1l/4s4h",
            "/$s$s/2s2a/4s4a",
            "/$s$s/2s2a/4s4h",
            "/$s$s/2s2a/4s4a",
            "/$s$s/2s2a/4s4h",
            "/$s$s/3s3e",
            "/$s$s/3s3e/5s5s",
            "/$s$s/3s3e/5s5s/1s1i",
            "/$s$s/3s3e/5s5s/1s1l",
            "/$s$s/3s3e/5s5s/1s1i/4s4a",
            "/$s$s/3s3e/5s5s/1s1i/4s4h",
            "/$s$s/3s3e/5s5s/1s1l/4s4a",
            "/$s$s/3s3e/5s5s/1s1l/4s4h",
            "/$s$s/3s3e/5s5s/4s4a",
            "/$s$s/3s3e/5s5s/4s4h",
            "/$s$s/3s3e/5s5s/4s4a",
            "/$s$s/3s3e/5s5s/4s4h",
            "/$s$s/3s3e/1s1i",
            "/$s$s/3s3e/1s1l",
            "/$s$s/3s3e/1s1i/4s4a",
            "/$s$s/3s3e/1s1i/4s4h",
            "/$s$s/3s3e/1s1l/4s4a",
            "/$s$s/3s3e/1s1l/4s4h",
            "/$s$s/3s3e/4s4a",
            "/$s$s/3s3e/4s4h",
            "/$s$s/3s3e/4s4a",
            "/$s$s/3s3e/4s4h",
            "/$s$s/5s5s",
            "/$s$s/5s5s/1s1i",
            "/$s$s/5s5s/1s1l",
            "/$s$s/5s5s/1s1i/4s4a",
            "/$s$s/5s5s/1s1i/4s4h",
            "/$s$s/5s5s/1s1l/4s4a",
            "/$s$s/5s5s/1s1l/4s4h",
            "/$s$s/5s5s/4s4a",
            "/$s$s/5s5s/4s4h",
            "/$s$s/5s5s/4s4a",
            "/$s$s/5s5s/4s4h",
            "/$s$s/1s1i",
            "/$s$s/1s1l",
            "/$s$s/1s1i/4s4a",
            "/$s$s/1s1i/4s4h",
            "/$s$s/1s1l/4s4a",
            "/$s$s/1s1l/4s4h",
            "/$s$s/4s4a",
            "/$s$s/4s4h",
            "/$s$s/4s4a",
            "/$s$s/4s4h",
            "/0s0o",
            "/0s0o/2s2a",
            "/0s0o/2s2a/3s3e",
            "/0s0o/2s2a/3s3e/5s5s",
            "/0s0o/2s2a/3s3e/5s5s/1s1i",
            "/0s0o/2s2a/3s3e/5s5s/1s1l",
            "/0s0o/2s2a/3s3e/5s5s/1s1i/4s4a",
            "/0s0o/2s2a/3s3e/5s5s/1s1i/4s4h",
            "/0s0o/2s2a/3s3e/5s5s/1s1l/4s4a",
            "/0s0o/2s2a/3s3e/5s5s/1s1l/4s4h",
            "/0s0o/2s2a/3s3e/5s5s/4s4a",
            "/0s0o/2s2a/3s3e/5s5s/4s4h",
            "/0s0o/2s2a/3s3e/5s5s/4s4a",
            "/0s0o/2s2a/3s3e/5s5s/4s4h",
            "/0s0o/2s2a/3s3e/1s1i",
            "/0s0o/2s2a/3s3e/1s1l",
            "/0s0o/2s2a/3s3e/1s1i/4s4a",
            "/0s0o/2s2a/3s3e/1s1i/4s4h",
            "/0s0o/2s2a/3s3e/1s1l/4s4a",
            "/0s0o/2s2a/3s3e/1s1l/4s4h",
            "/0s0o/2s2a/3s3e/4s4a",
            "/0s0o/2s2a/3s3e/4s4h",
            "/0s0o/2s2a/3s3e/4s4a",
            "/0s0o/2s2a/3s3e/4s4h",
            "/0s0o/2s2a/5s5s",
            "/0s0o/2s2a/5s5s/1s1i",
            "/0s0o/2s2a/5s5s/1s1l",
            "/0s0o/2s2a/5s5s/1s1i/4s4a",
            "/0s0o/2s2a/5s5s/1s1i/4s4h",
            "/0s0o/2s2a/5s5s/1s1l/4s4a",
            "/0s0o/2s2a/5s5s/1s1l/4s4h",
            "/0s0o/2s2a/5s5s/4s4a",
            "/0s0o/2s2a/5s5s/4s4h",
            "/0s0o/2s2a/5s5s/4s4a",
            "/0s0o/2s2a/5s5s/4s4h",
            "/0s0o/2s2a/1s1i",
            "/0s0o/2s2a/1s1l",
            "/0s0o/2s2a/1s1i/4s4a",
            "/0s0o/2s2a/1s1i/4s4h",
            "/0s0o/2s2a/1s1l/4s4a",
            "/0s0o/2s2a/1s1l/4s4h",
            "/0s0o/2s2a/4s4a",
            "/0s0o/2s2a/4s4h",
            "/0s0o/2s2a/4s4a",
            "/0s0o/2s2a/4s4h",
            "/0s0o/3s3e",
            "/0s0o/3s3e/5s5s",
            "/0s0o/3s3e/5s5s/1s1i",
            "/0s0o/3s3e/5s5s/1s1l",
            "/0s0o/3s3e/5s5s/1s1i/4s4a",
            "/0s0o/3s3e/5s5s/1s1i/4s4h",
            "/0s0o/3s3e/5s5s/1s1l/4s4a",
            "/0s0o/3s3e/5s5s/1s1l/4s4h",
            "/0s0o/3s3e/5s5s/4s4a",
            "/0s0o/3s3e/5s5s/4s4h",
            "/0s0o/3s3e/5s5s/4s4a",
            "/0s0o/3s3e/5s5s/4s4h",
            "/0s0o/3s3e/1s1i",
            "/0s0o/3s3e/1s1l",
            "/0s0o/3s3e/1s1i/4s4a",
            "/0s0o/3s3e/1s1i/4s4h",
            "/0s0o/3s3e/1s1l/4s4a",
            "/0s0o/3s3e/1s1l/4s4h",
            "/0s0o/3s3e/4s4a",
            "/0s0o/3s3e/4s4h",
            "/0s0o/3s3e/4s4a",
            "/0s0o/3s3e/4s4h",
            "/0s0o/5s5s",
            "/0s0o/5s5s/1s1i",
            "/0s0o/5s5s/1s1l",
            "/0s0o/5s5s/1s1i/4s4a",
            "/0s0o/5s5s/1s1i/4s4h",
            "/0s0o/5s5s/1s1l/4s4a",
            "/0s0o/5s5s/1s1l/4s4h",
            "/0s0o/5s5s/4s4a",
            "/0s0o/5s5s/4s4h",
            "/0s0o/5s5s/4s4a",
            "/0s0o/5s5s/4s4h",
            "/0s0o/1s1i",
            "/0s0o/1s1l",
            "/0s0o/1s1i/4s4a",
            "/0s0o/1s1i/4s4h",
            "/0s0o/1s1l/4s4a",
            "/0s0o/1s1l/4s4h",
            "/0s0o/4s4a",
            "/0s0o/4s4h",
            "/0s0o/4s4a",
            "/0s0o/4s4h",
            "/2s2a",
            "/2s2a/3s3e",
            "/2s2a/3s3e/5s5s",
            "/2s2a/3s3e/5s5s/1s1i",
            "/2s2a/3s3e/5s5s/1s1l",
            "/2s2a/3s3e/5s5s/1s1i/4s4a",
            "/2s2a/3s3e/5s5s/1s1i/4s4h",
            "/2s2a/3s3e/5s5s/1s1l/4s4a",
            "/2s2a/3s3e/5s5s/1s1l/4s4h",
            "/2s2a/3s3e/5s5s/4s4a",
            "/2s2a/3s3e/5s5s/4s4h",
            "/2s2a/3s3e/5s5s/4s4a",
            "/2s2a/3s3e/5s5s/4s4h",
            "/2s2a/3s3e/1s1i",
            "/2s2a/3s3e/1s1l",
            "/2s2a/3s3e/1s1i/4s4a",
            "/2s2a/3s3e/1s1i/4s4h",
            "/2s2a/3s3e/1s1l/4s4a",
            "/2s2a/3s3e/1s1l/4s4h",
            "/2s2a/3s3e/4s4a",
            "/2s2a/3s3e/4s4h",
            "/2s2a/3s3e/4s4a",
            "/2s2a/3s3e/4s4h",
            "/2s2a/5s5s",
            "/2s2a/5s5s/1s1i",
            "/2s2a/5s5s/1s1l",
            "/2s2a/5s5s/1s1i/4s4a",
            "/2s2a/5s5s/1s1i/4s4h",
            "/2s2a/5s5s/1s1l/4s4a",
            "/2s2a/5s5s/1s1l/4s4h",
            "/2s2a/5s5s/4s4a",
            "/2s2a/5s5s/4s4h",
            "/2s2a/5s5s/4s4a",
            "/2s2a/5s5s/4s4h",
            "/2s2a/1s1i",
            "/2s2a/1s1l",
            "/2s2a/1s1i/4s4a",
            "/2s2a/1s1i/4s4h",
            "/2s2a/1s1l/4s4a",
            "/2s2a/1s1l/4s4h",
            "/2s2a/4s4a",
            "/2s2a/4s4h",
            "/2s2a/4s4a",
            "/2s2a/4s4h",
            "/3s3e",
            "/3s3e/5s5s",
            "/3s3e/5s5s/1s1i",
            "/3s3e/5s5s/1s1l",
            "/3s3e/5s5s/1s1i/4s4a",
            "/3s3e/5s5s/1s1i/4s4h",
            "/3s3e/5s5s/1s1l/4s4a",
            "/3s3e/5s5s/1s1l/4s4h",
            "/3s3e/5s5s/4s4a",
            "/3s3e/5s5s/4s4h",
            "/3s3e/5s5s/4s4a",
            "/3s3e/5s5s/4s4h",
            "/3s3e/1s1i",
            "/3s3e/1s1l",
            "/3s3e/1s1i/4s4a",
            "/3s3e/1s1i/4s4h",
            "/3s3e/1s1l/4s4a",
            "/3s3e/1s1l/4s4h",
            "/3s3e/4s4a",
            "/3s3e/4s4h",
            "/3s3e/4s4a",
            "/3s3e/4s4h",
            "/5s5s",
            "/5s5s/1s1i",
            "/5s5s/1s1l",
            "/5s5s/1s1i/4s4a",
            "/5s5s/1s1i/4s4h",
            "/5s5s/1s1l/4s4a",
            "/5s5s/1s1l/4s4h",
            "/5s5s/4s4a",
            "/5s5s/4s4h",
            "/5s5s/4s4a",
            "/5s5s/4s4h",
            "/1s1i",
            "/1s1l",
            "/1s1i/4s4a",
            "/1s1i/4s4h",
            "/1s1l/4s4a",
            "/1s1l/4s4h",
            "/4s4a",
            "/4s4h",
            "/4s4a",
            "/4s4h"
        };

        /// <summary>
        /// Transformation rules. </summary>
        internal static readonly string[] Constructors = new string[] { ":", "r", "d", "f", "dr", "fr", "rf" };

        /// <summary>
        /// Create a new instance using the default word list. </summary>
        public PasswordChecker() : this(null, IndexFilesName)
        {
        }

        /// <summary>
        /// Create a new instance using the default word list. </summary>
        public PasswordChecker(string path) : this(path, IndexFilesName)
        {
        }

        /// <summary>
        /// Create a new instance. </summary>
        /// <param name="path">Path where word list and index files are located</param>
        /// <param name="wordList"> the name of the word list files. </param>
        public PasswordChecker(string path, string wordList)
        {
            _path = path;
            _wordList = wordList;
        }

        /// <summary>
        /// Check password against rules. </summary>
        /// <param name="password"> the password. </param>
        /// <returns> Set of status codes. </returns>
        public ISet<StatusCode> Check(string password)
        {
            ISet<StatusCode> status;

            var packer = new Packer(_path, _wordList, "r");

            try
            {
                status = Check(packer, password);
            }
            finally
            {
                packer.Close();
            }

            return status;
        }

        /// <summary>
        /// Check password against rules. </summary>
        /// <param name="p"> the packer holding the word list. </param>
        /// <param name="password"> the password. </param>
        /// <returns> Set of error codes. </returns>
        public ISet<StatusCode> Check(Packer p, string password)
        {

            ISet<StatusCode> result = new HashSet<StatusCode>();
            if (password.Length < 4)
            {
                result.Add(StatusCode.Short);
            }
            if (password.Length < MinLength)
            {
                result.Add(StatusCode.Short);
            }
            string junk = password.Substring(0, 1);
            for (int i = 1; i < password.Length; i++)
            {
                if (junk.IndexOf(password[i]) == -1)
                {
                    junk = junk + password[i];
                }
            }
            if (junk.Length < MinDiff)
            {
                result.Add(StatusCode.Different);
            }
            if ((password = password.Trim()).Length == 0)
            {
                result.Add(StatusCode.Whitespace);
            }
            foreach (var destructor in Destructors)
            {
                string mp;
                if (ReferenceEquals((mp = Rules.Mangle(password, destructor)), null))
                {
                    continue;
                }
                if (p.Find(mp) != -1)
                {
                    result.Add(StatusCode.Dictionary);
                }
            }
            password = Rules.Reverse(password);
            foreach (var destructor in Destructors)
            {
                string mp;
                if (ReferenceEquals((mp = Rules.Mangle(password, destructor)), null))
                {
                    continue;
                }
                if (p.Find(mp) != -1)
                {
                    result.Add(StatusCode.Dictionary);
                }
            }

            // if there are no error codes then password is acceptable
            if (!result.Any())
            {
                result.Add(StatusCode.Ok);
            }

            return result;
        }

        /// <summary>
        /// Attempt to turn a password into raw-text. </summary>
        /// <param name="rawtext"> the raw-text. </param>
        /// <param name="password"> the password. </param>
        /// <returns> true if the password can be turned into the raw-text. </returns>
        public bool MatchPasswordAndRawText(string rawtext, string password)
        {
            /* use destructor to turn password into raw-text */
            /* note use of Reverse() to save duplicating all rules */
            string mp;
            foreach (var destructor in Destructors)
            {
                if (ReferenceEquals((mp = Rules.Mangle(password, destructor)), null))
                {
                    continue;
                }
                if (mp.Equals(rawtext))
                {
                    return true;
                }
                if (Rules.Reverse(mp).Equals(rawtext))
                {
                    return true;
                }
            }
            foreach (var constructor in Constructors)
            {
                if (ReferenceEquals((mp = Rules.Mangle(rawtext, constructor)), null))
                {
                    continue;
                }
                if (mp.Equals(rawtext))
                {
                    return true;
                }
            }
            return false;
        }
    }
}