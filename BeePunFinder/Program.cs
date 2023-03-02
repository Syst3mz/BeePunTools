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
            var nm = MessageConvert("Fourscore and seven years ago our fathers brought forth, on this continent, a new nation, conceived in liberty, and dedicated to the proposition that all men are created equal. Now we are engaged in a great civil war, testing whether that nation, or any nation so conceived, and so dedicated, can long endure. We are met on a great battle-field of that war. We have come to dedicate a portion of that field, as a final resting-place for those who here gave their lives, that that nation might live. It is altogether fitting and proper that we should do this. But, in a larger sense, we cannot dedicate, we cannot consecrate—we cannot hallow—this ground. The brave men, living and dead, who struggled here, have consecrated it far above our poor power to add or detract. The world will little note, nor long remember what we say here, but it can never forget what they did here. It is for us the living, rather, to be dedicated here to the unfinished work which they who fought here have thus far so nobly advanced. It is rather for us to be here dedicated to the great task remaining before us—that from these honored dead we take increased devotion to that cause for which they here gave the last full measure of devotion—that we here highly resolve that these dead shall not have died in vain—that this nation, under God, shall have a new birth of freedom, and that government of the people, by the people, for the people, shall not perish from the earth.");
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
            if (lookup.ContainsKey(clean.ToLower()))
            {
                var entry = lookup[clean.ToLower()];
                if (entry.synonyms.Count >= 1)
                {
                    foreach (string synonym in entry.synonyms)
                    {
                        if (candidates.Contains(synonym))
                        {
                            return $"[({synonym})]";
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
        public List<string> synonyms { get; set; }
    }
}