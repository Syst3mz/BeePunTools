using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BeePunFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            var nm = MessageConvert("It is a period of civil war. Rebel spaceships, striking from a hidden base, have won their first victory against the evil Galactic Empire. During the battle, Rebel spies managed to steal secret plans to the Empire's ultimate weapon, the DEATH STAR, an armored space station with enough power to destroy an entire planet. Pursued by the Empires sinister agents, Princess Leia races home aboard her starship, custodian of the stolen plans that can save her people and restore freedom to the galaxy....");
            Console.WriteLine(nm);
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
        
        static List<TEntry> GetTEntries()
        {
            var ret = new List<TEntry>();
            foreach (var line in File.ReadLines("en_thesaurus.jsonl"))
            {
                ret.Add(JsonSerializer.Deserialize<TEntry>(line));
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
            
            Dictionary<string, TEntry> lookup = new Dictionary<string, TEntry>();

            foreach (var entry in GetTEntries())
            {
                if (!lookup.ContainsKey(entry.word))
                {
                    lookup.Add(entry.word, entry);
                }
            }


            string newMessage = "";
            foreach (var word in message.Split(" "))
            {
                var clean = word.Trim().Trim("!\'\"\\.,/<>?}{[]|=+-_*!@#$%^&()}".ToArray());
                newMessage += FindReplacement(clean, lookup, candidates) + " ";
            }
            
            return newMessage;
        }

        private static string FindReplacement(string clean, Dictionary<string, TEntry> lookup, HashSet<string> candidates)
        {
            if (lookup.ContainsKey(clean.ToLower()))
            {
                var entry = lookup[clean.ToLower()];
                if (entry.synonyms.Length >= 1)
                {
                    foreach (string synonym in entry.synonyms)
                    {
                        if (candidates.Contains(synonym))
                        {
                            return synonym;
                        }
                    }

                    return clean;
                }
                return clean;
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
        public string[] synonyms { get; set; }
    }
}