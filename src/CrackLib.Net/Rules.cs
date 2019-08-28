namespace CrackLib.Net
{
    using System;
    using System.Text;
    internal class Rules
    {
        public const char RuleNoop = ':';
        public const char RulePrepend = '^';
        public const char RuleAppend = '$';
        public const char RuleReverse = 'r';
        public const char RuleUppercase = 'u';
        public const char RuleLowercase = 'l';
        public const char RulePluralise = 'p';
        public const char RuleCapitalise = 'c';
        public const char RuleDuplicate = 'd';
        public const char RuleReflect = 'f';
        public const char RuleSubstitute = 's';
        public const char RuleMatch = '/';
        public const char RuleNot = '!';
        public const char RuleLt = '<';
        public const char RuleGt = '>';
        public const char RuleExtract = 'x';
        public const char RuleOverstrike = 'o';
        public const char RuleInsert = 'i';
        public const char RuleEquals = '=';
        public const char RulePurge = '@';
        // class rule? socialist ethic in cracker?
        public const char RuleClass = '?';
        public const char RuleDfirst = '[';
        public const char RuleDlast = ']';
        public const char RuleMfirst = '(';
        public const char RuleMlast = ')';

        public static bool Suffix(string myword, string suffix)
        {
            int i = myword.Length;
            int j = suffix.Length;
            if (i > j)
            {
                return myword.IndexOf(suffix, StringComparison.Ordinal) == i - j;
            }
            else
            {
                return false;
            }
        }
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static string Capitalise(string s)
        {
            return s.Substring(0, 1).ToUpper() + s.Substring(1).ToLower();
        }

        public static string Pluralise(string s)
        {
            if (Suffix(s, "ch") || Suffix(s, "ex") || Suffix(s, "ix") || Suffix(s, "sh") || Suffix(s, "ss"))
            {
                /* bench -> benches */
                return s + "es";
            }
            else if (s.Length > 2 && s[s.Length - 1] == 'y')
            {
                if ("aeiou".IndexOf(s[s.Length - 2]) != -1)
                {
                    /* alloy -> alloys */
                    return s + "s";
                }
                else
                {
                    /* gully -> gullies */
                    return s.Substring(s.Length - 2) + "ies";
                }
            }
            else if (s[s.Length - 1] == 's')
            {
                /* bias -> biases */
                return s + "es";
            }
            else
            {
                /* catchall */
                return s + "s";
            }
        }

        public static string Purge(string s, char c)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != c)
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// this function takes two inputs, a class identifier and a character, and
        /// returns non-null if the given character is a member of the class, based
        /// upon restrictions set out below
        /// </summary>
        public static bool MatchClass(char clazz, char c)
        {
            bool retval = false;
            switch (clazz)
            {
                /* ESCAPE */
                case '?': // ?? -> ?
                    if (c == '?')
                    {
                        retval = true;
                    }
                    break;
                /* ILLOGICAL GROUPINGS (ie: not in ctype.h) */
                case 'V':
                case 'v': // vowels
                    if ("aeiou".IndexOf(char.ToLower(c)) != -1)
                    {
                        retval = true;
                    }
                    break;
                case 'C':
                case 'c': // consonants
                    if ("bcdfghjklmnpqrstvwxyz".IndexOf(char.ToLower(c)) != -1)
                    {
                        retval = true;
                    }
                    break;
                case 'W':
                case 'w': // whitespace
                    retval = char.IsWhiteSpace(c);
                    break;
                case 'P':
                case 'p': // punctuation
                    if (".`,:;'!?\"".IndexOf(char.ToLower(c)) != -1)
                    {
                        retval = true;
                    }
                    break;
                case 'S':
                case 's': // symbols
                    if ("$%%^&*()-_+=|\\[]{}#@/~".IndexOf(char.ToLower(c)) != -1)
                    {
                        retval = true;
                    }
                    break;
                /* LOGICAL GROUPINGS */
                case 'L':
                case 'l': // lowercase
                    retval = char.IsLower(c);
                    break;
                case 'U':
                case 'u': // uppercase
                    retval = char.IsUpper(c);
                    break;
                case 'A':
                case 'a': // alphabetic
                    retval = char.IsLetter(c);
                    break;
                case 'X':
                case 'x': // alphanumeric
                    retval = char.IsLetterOrDigit(c);
                    break;
                case 'D':
                case 'd': // digits
                    retval = char.IsDigit(c);
                    break;
                default:
                    throw new System.ArgumentException("MatchClass: unknown class :" + clazz);
            }
            if (char.IsUpper(clazz))
            {
                return retval;
            }
            return retval;
        }
        public static int IndexOf(string s, char clazz)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (MatchClass(clazz, s[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string Replace(string s, char clazz, char newChar)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (MatchClass(clazz, s[i]))
                {
                    sb.Append(newChar);
                }
                else
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }

        public static string PolyPurge(string s, char clazz)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (!MatchClass(clazz, s[i]))
                {
                    sb.Append(s[i]);
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Silly function used for > and < compares.
        /// </summary>
        public static int Char2Int(char c)
        {
            if (char.IsDigit(c))
            {
                return (c - '0');
            }
            else if (char.IsLower(c))
            {
                return (c - 'a' + 10);
            }
            else if (char.IsUpper(c))
            {
                return (c - 'A' + 10);
            }
            return -1;
        }
        public static string Mangle(string s, string control)
        {
            for (int i = 0; i < control.Length; i++)
            {
                switch (control[i])
                {
                    case RuleNoop:
                        break;
                    case RuleReverse:
                        s = Reverse(s);
                        break;
                    case RuleUppercase:
                        s = s.ToUpper();
                        break;
                    case RuleLowercase:
                        s = s.ToLower();
                        break;
                    case RuleCapitalise:
                        s = Capitalise(s);
                        break;
                    case RulePluralise:
                        s = Pluralise(s);
                        break;
                    case RuleReflect:
                        s = Reverse(s);
                        break;
                    case RuleDuplicate:
                        s = s + s;
                        break;
                    case RuleGt:
                        if (i == control.Length - 1)
                        {
                            throw new System.ArgumentException("mangle: '>' missing argument in :" + control);
                        }
                        else
                        {
                            int limit = Char2Int(control[++i]);
                            if (limit < 0)
                            {
                                throw new System.ArgumentException("mangle: '>' weird argument in :" + control);
                            }
                            if (s.Length <= limit)
                            {
                                return null;
                            }
                        }
                        break;
                    case RuleLt:
                        if (i == control.Length - 1)
                        {
                            throw new System.ArgumentException("mangle: '<' missing argument in :" + control);
                        }
                        else
                        {
                            int limit = Char2Int(control[++i]);
                            if (limit < 0)
                            {
                                throw new System.ArgumentException("mangle: '<' weird argument in :" + control);
                            }
                            if (s.Length >= limit)
                            {
                                return null;
                            }
                        }
                        break;
                    case RulePrepend:
                        if (i == control.Length - 1)
                        {
                            throw new System.ArgumentException("mangle: prepend missing argument in :" + control);
                        }
                        else
                        {
                            s = control[++i] + s;
                        }
                        break;
                    case RuleAppend:
                        if (i == control.Length - 1)
                        {
                            throw new System.ArgumentException("mangle: prepend missing argument in :" + control);
                        }
                        else
                        {
                            s = s + control[++i];
                        }
                        break;
                    case RuleExtract:
                        if (i >= control.Length - 2)
                        {
                            throw new System.ArgumentException("mangle: extract missing argument in :" + control);
                        }
                        else
                        {
                            int start = Char2Int(control[++i]);
                            int length = Char2Int(control[++i]);
                            if (start < 0 || length < 0)
                            {
                                throw new System.ArgumentException("mangle: extract: weird argument in :" + control);
                            }
                            s = s.Substring(start, length);
                        }
                        break;
                    case RuleOverstrike:
                        if (i >= control.Length - 2)
                        {
                            throw new System.ArgumentException("mangle: overs-trike missing argument in :" + control);
                        }
                        else
                        {
                            int pos = Char2Int(control[++i]);
                            if (i < 0)
                            {
                                throw new System.ArgumentException("mangle: overs-trike weird argument in :" + control);
                            }
                            StringBuilder sb = new StringBuilder(s);
                            sb[pos] = control[++i];
                            s = sb.ToString();
                        }
                        break;
                    case RuleInsert:
                        if (i >= control.Length - 2)
                        {
                            throw new System.ArgumentException("mangle: insert missing argument in :" + control);
                        }
                        else
                        {
                            int pos = Char2Int(control[++i]);
                            if (i < 0)
                            {
                                throw new System.ArgumentException("mangle: insert weird argument in :" + control);
                            }
                            s = (new StringBuilder(s)).Insert(pos, control[++i]).ToString();
                        }
                        break;
                    // THE FOLLOWING RULES REQUIRE CLASS MATCHING
                    case RulePurge: // @x or @?c
                        if (i == control.Length || (control[i + 1] == RuleClass && i == control.Length - 2))
                        {
                            throw new System.ArgumentException("mangle: delete missing argument in :" + control);
                        }
                        else if (control[i + 1] != RuleClass)
                        {
                            s = Purge(s, control[++i]);
                        }
                        else
                        {
                            s = PolyPurge(s, control[i + 2]);
                            i += 2;
                        }
                        break;
                    case RuleSubstitute: // sxy || s?cy
                        if (i >= control.Length - 2 || (control[i + 1] == RuleClass && i == control.Length - 3))
                        {
                            throw new System.ArgumentException("mangle: subst missing argument in :" + control);
                        }
                        else if (control[i + 1] != RuleClass)
                        {
                            s = s.Replace(control[i + 1], control[i + 2]);
                            i += 2;
                        }
                        else
                        {
                            s = Replace(s, control[i + 2], control[i + 3]);
                            i += 3;
                        }
                        break;
                    case RuleMatch: // /x || /?c
                        if (i == control.Length || (control[i + 1] == RuleClass && i == control.Length - 2))
                        {
                            throw new System.ArgumentException("mangle: / missing argument in :" + control);
                        }
                        else if (control[i + 1] != RuleClass)
                        {
                            if (s.IndexOf(control[++i]) == -1)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            if (IndexOf(s, control[i + 2]) == -1)
                            {
                                return null;
                            }
                            i += 2;
                        }
                        break;
                    case RuleNot: // !x || !?c
                        if (i == control.Length || (control[i + 1] == RuleClass && i == control.Length - 2))
                        {
                            throw new System.ArgumentException("mangle: ! missing argument in :" + control);
                        }
                        else if (control[i + 1] != RuleClass)
                        {
                            if (s.IndexOf(control[++i]) != -1)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            if (IndexOf(s, control[i + 2]) != -1)
                            {
                                return null;
                            }
                            i += 2;
                        }
                        break;
                    case RuleEquals: // =nx || =n?c
                        if (i >= control.Length - 2 || (control[i + 1] == RuleClass && i == control.Length - 3))
                        {
                            throw new System.ArgumentException("mangle: '=' missing argument in :" + control);
                        }
                        else
                        {
                            int pos = Char2Int(control[i + 1]);
                            if (pos < 0)
                            {
                                throw new System.ArgumentException("mangle: '='weird argument in :" + control);
                            }
                            if (control[i + 2] != RuleClass)
                            {
                                i += 2;
                                if (s[pos] != control[i])
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                i += 3;
                                if (!MatchClass(s[i], s[pos]))
                                {
                                    return null;
                                }
                            }
                        }
                        break;
                    case RuleDfirst:
                        if (s.Length >= 1)
                        {
                            s = s.Substring(1);
                        }
                        break;
                    case RuleDlast:
                        if (s.Length >= 2)
                        {
                            s = s.Substring(0, s.Length - 2);
                        }
                        break;
                    case RuleMfirst:
                        if (i == control.Length || (control[i + 1] == RuleClass && i == control.Length - 2))
                        {
                            throw new System.ArgumentException("mangle: '(' missing argument in :" + control);
                        }
                        else
                        {
                            if (control[i + 1] != RuleClass)
                            {
                                i++;
                                if (s[0] != control[i])
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                i += 2;
                                if (!MatchClass(control[i], s[0]))
                                {
                                    return null;
                                }
                            }
                        }
                        goto case RuleMlast;
                    case RuleMlast:
                        if (i == control.Length || (control[i + 1] == RuleClass && i == control.Length - 2))
                        {
                            throw new System.ArgumentException("mangle: ')' missing argument in :" + control);
                        }
                        else
                        {
                            if (control[i + 1] != RuleClass)
                            {
                                i++;
                                if (s[s.Length - 1] != control[i])
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                i += 2;
                                if (!MatchClass(control[i], s[s.Length - 1]))
                                {
                                    return null;
                                }
                            }
                        }
                        goto default;
                    default:
                        throw new System.ArgumentException("mangle: unknown command in :" + control);
                }
            }
            if (s.Length == 0)
            {
                return null;
            }
            return s;
        }
        public static bool PMatch(string control, string s)
        {
            if (control.Length != s.Length)
            {
                return false;
            }
            for (int i = 0; i < s.Length && i < control.Length; i++)
            {
                if (!MatchClass(control[i], s[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}