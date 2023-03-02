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
            var nm = MessageConvert("it is a period of civil war. Rebel spaceships, striking from a hidden base, have won their first victory against the evil Galactic Empire. During the battle, Rebel spies managed to steal secret plans to the Empire's ultimate weapon, the DEATH STAR, and space station with enough power to destroy an entire planet. Pursued by the Empire's sinister agents, Princess Leia races home aboard her starship, custodian of the stolen plans that can save her people and restore freedom to the galaxy");
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
                var clean = word.Trim().Trim("!\'\"\\.,/<>?}{[]|=+-_*!@#$%^&()}".ToArray());
                newMessage += FindReplacement(clean, lookup, candidates) + " ";
            }
            
            return newMessage;
        }

        private static string FindReplacement(string clean, Dictionary<string, TEntry> lookup, HashSet<string> candidates)
        {
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
                var o = string.Join(",", validSwaps);
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