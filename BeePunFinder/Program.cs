using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CommandLine;

namespace BeePunFinder
{
    class Program
    {
        private static bool FilterShort;
        private static bool OutputBeeTxt;
        private static bool WriteList;

        class Options
        {
            [Option('f', "FilterShort", HelpText = "Ignores words that are less than 2 chars to avoid substitutions with chemical elements.")]
            public bool FilterShort { get; set; }
            [Option('l', "WriteSubNames", HelpText = "Outputs the full list of found substitutions, if this is false, it will only output the first.")]
            public bool WriteList { get; set; }
            [Option('b', "OutputBeeText", HelpText = "Write out a new bee candidates file...its unclear why you would do this.")]
            public bool OutputBeeTxt { get; set; }
            [Option('i', "InputFile", HelpText = "Path to the input file.", SetName = "fop")]
            public string InputFile { get; set; }
            [Option('o', "OutputFile", HelpText = "Path to the output file.", SetName = "fop")]
            public string OutputFile { get; set; }
            [Option('c', "InstantConvert", HelpText = "Convert all of the following", SetName = "convert")]
            public IEnumerable<string> InstantConvert { get; set; }
        }
        
        static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    FilterShort = options.FilterShort;
                    OutputBeeTxt = options.OutputBeeTxt;
                    WriteList = options.WriteList;

                    if (options.InputFile != null)
                    {
                        if (options.OutputFile == null)
                        {
                            options.OutputFile = options.InputFile + ".o";
                        }

                        var f = File.ReadAllText(options.InputFile);
                        var convert = MessageConvert(f);
                        File.WriteAllText(options.OutputFile, convert);
                    }
                    else
                    {
                        Console.WriteLine(MessageConvert(string.Join(" ", options.InstantConvert)));
                    }
                });

            if (OutputBeeTxt)
            {
                MakeBeeCandidates();
            }
        }

        static void MakeBeeCandidates()
        {
            var f = File.OpenWrite("bee_candidates.txt");
            StreamWriter sw = new StreamWriter(f);
            foreach (var word in GetBeeCandidates())
            {
                Console.WriteLine(word);
                sw.WriteLine(word[0]);
            }
        }

        static List<string> GetBeeCandidates()
        {
            var beeC = new List<string>();
            foreach (var line in File.ReadLines("en_US.txt"))
            {
                var wordSep = line.Split("/");
                if (wordSep[1].Contains("i"))
                {
                    beeC.Add(wordSep[0].Trim());
                }
            }

            return beeC;
        }
        
        static Dictionary<string, TEntry> GetTEntries()
        {
            var ret = new Dictionary<string, TEntry>();
            foreach (var line in File.ReadLines("en_thesaurus.jsonl"))
            {
                var t = JsonSerializer.Deserialize<TEntry>(line);
                if (ret.ContainsKey(t.word))
                {
                    ret[t.word].synonyms.AddRange(t.synonyms);
                }
                else
                {
                    ret.Add(t.word, t);
                }
            }

            return ret;
        }

        static string MessageConvert(string message)
        {
            var candidates = new HashSet<string>();

            foreach (string beeCandidate in GetBeeCandidates())
            {
                if (!candidates.Contains(beeCandidate))
                {
                    candidates.Add(beeCandidate);
                }
            }

            Dictionary<string, TEntry> lookup = GetTEntries();


            string newMessage = "";
            foreach (var word in message.Split(" "))
            {
                bool addNewline = word.Contains("\n");

                var clean = word.Trim().Trim("!\'\"\\.,/<>?}{[]|=+-_*!@#$%^&()\n\t}".ToArray());
                newMessage += FindReplacement(clean, lookup, candidates) + " " + (addNewline? "\n" : "");
            }
            
            return newMessage;
        }

        private static string FindReplacement(string clean, Dictionary<string, TEntry> lookup, HashSet<string> candidates)
        {
            if (FilterShort && clean.Length <= 2)
            {
                return clean;
            }
            
            if (!lookup.ContainsKey(clean.ToLower()))
            {
                return clean;
            }

            var entry = lookup[clean.ToLower()];
            if (entry.synonyms.Count < 1)
            {
                return clean;
            }
            
            List<string> validSwaps = new List<string>();
            if (candidates.Contains(clean.ToLower()))
            {
                validSwaps.Add(clean);
            }
            
            foreach (string synonym in entry.synonyms)
            {
                if (candidates.Contains(synonym))
                {
                    validSwaps.Add(synonym);
                }
            }

            if (validSwaps.Count >= 1)
            {
                string o;
                if (WriteList)
                {
                    o = string.Join(",", validSwaps);
                }
                else
                {
                    o = validSwaps[0];
                }
                return $"[({o})]";
            }

            return clean;

        }
    }

    class WordEntry
    {
        public string word { get; set; }
        public string pos { get; set; }
        public string? synonyms { get; set; }
        public string[] definitions { get; set; }
    }
    
    class TEntry
    {
        public string word { get; set; }
        public string key { get; set; }
        public string pos { get; set; }
        public List<string> synonyms { get; set; }
    }
}