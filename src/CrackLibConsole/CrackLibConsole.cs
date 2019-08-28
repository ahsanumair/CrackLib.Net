namespace CrackLibConsole
{
    using System;
    using System.IO;
    using System.Configuration;
    using CrackLib.Net;

    public class CrackLibConsole
    {
        private const string IndexFilesName = "words.pack";
        private const string WordListFileName = "words";

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args"> the arguments. </param>
        public static void Main(string[] args)
        {
            try
            {
                var path = ConfigurationManager.AppSettings["CrackLibPath"];

                if (args.Length >= 2 && args.Length <= 3 && args[0].Equals("-check"))
                {
                    var name = args.Length == 3 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : IndexFilesName;
                    var word = args.Length == 3 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : args[1];

                    try
                    {
                        var err = new PasswordChecker(path,name).Check(word);
                        Console.WriteLine(string.Join(", ", err));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else if (args.Length >= 1 && args.Length <= 2 && args[0].Equals("-dump"))
                {
                    var name = args.Length == 2 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : IndexFilesName;

                    Packer p = new Packer(path, name, "r");
                    try
                    {
                        for (int i = 0; i < p.Size(); i++)
                        {
                            Console.WriteLine(p.Get(i));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        p.Close();
                    }
                }
                else if (args.Length >= 1 && args.Length <= 3 && args[0].Equals("-make"))
                {
                    var name = args.Length == 3 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : IndexFilesName;
                    var words = args.Length == 3 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : WordListFileName;

                    Packer p = new Packer(path, name, "rw");
                    try
                    {
                        path = !string.IsNullOrWhiteSpace(path) ? Path.Combine(path, words) : words;

                        if (!File.Exists(path))
                        {
                            Console.WriteLine($"Dictionary word file at \"{path}\" not found");
                            return;
                        }

                        StreamReader br = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));

                        string s;
                        while (!ReferenceEquals(s = br.ReadLine(), null))
                        {
                            Console.WriteLine("Putting : " + s);
                            p.Put(s);
                        }

                        Console.WriteLine("Index build completed successfully");
                    }
                    catch (NotSupportedException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        p.Close();
                    }
                }
                else if (args.Length >= 2 && args.Length <= 3 && args[0].Equals("-find"))
                {
                    var name = args.Length == 3 && !string.IsNullOrWhiteSpace(args[1]) ? args[1] : IndexFilesName;
                    var word = args.Length == 3 && !string.IsNullOrWhiteSpace(args[2]) ? args[2] : args[1];

                    Packer p = new Packer(path, name, "r");
                    try
                    {
                        if (string.IsNullOrWhiteSpace(word))
                        {
                            Console.WriteLine("No word provided to find.");
                            return;
                        }

                        int i = p.Find(word);
                        if (i != -1)
                        {
                            Console.WriteLine("Found " + p.Get(i) + " at " + i);
                        }
                        else
                        {
                            Console.WriteLine(word + " not found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        p.Close();
                    }
                }
                else
                {
                    Usage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Output usage to stderr.
        /// </summary>
        public static void Usage()
        {
            Console.Error.WriteLine("");
            Console.Error.WriteLine("CrackLibConsole checks if given word is an acceptable password. It also contains commands to re-build the index files when new words are added in a dictionary file, it can also be used to find and dump the words in the index.");
            Console.Error.WriteLine("");
            Console.Error.WriteLine("-------------- Usage 1 ------------------");
            Console.Error.WriteLine("CrackLibConsole -check <word> - Checks if word is acceptable as password in default dictionary called 'words.pack'");
            Console.Error.WriteLine(" | -find <word> -Finds a word in default dictionary called 'words.pack'");
            Console.Error.WriteLine(" | -make - Generates index files named 'words.pack' from given word list file named 'words'");
            Console.Error.WriteLine(" | -dump - Displays contents of default dictionary 'words.pack'");

            Console.Error.WriteLine("-------------- Usage 2 ------------------");
            Console.Error.WriteLine("CrackLibConsole -check <dict-file> <word> - Checks if word is acceptable as password in a given dictionary");
            Console.Error.WriteLine(" | -find <dict-file> <word> - Finds a word in a given dictionary");
            Console.Error.WriteLine(" | -make <dict-file> <words-list-file> - Generates word index files from default dictionary file");
            Console.Error.WriteLine(" | -dump <dict-file> - Displays contents of given dictionary file");
        }
    }
}