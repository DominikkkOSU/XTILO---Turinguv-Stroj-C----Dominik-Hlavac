using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSMachine
{


    public static class EncodingHelper //převod pravidel do unárního kódu
    {

        public class EncodingMap // Umožňuje zpětně zjištění, že např. "q_start" je kódováno jako číslo 1.
        {
            public Dictionary<string, int> States { get; set; }
            public Dictionary<string, int> Symbols { get; set; }
            public Dictionary<Direction, int> Directions { get; set; }
        }


        
   
        public static EncodingMap CreateEncodingMappings(List<Rule> rules) // Vytvoření kompletní mapování pro danou sadu pravidel.
        {
            var validRules = rules.Where(r => r != null).ToList();
            return new EncodingMap
            {
                States = CreateStatesMapping(validRules),
                Symbols = CreateSymbolsMapping(validRules),
                Directions = CreateDirectionsMapping()
            };
        }


  

        private static Dictionary<string, int> CreateStatesMapping(List<Rule> rules)         // Přiřadí každému stavu unikátní číslo.
        {
            // Použití HashSet pro získání unikátních stavů z levé i pravé strany pravidel
            var uniqueStates = new HashSet<State>();
            foreach (var r in rules)
            {
                uniqueStates.Add(r.CurrentState);
                uniqueStates.Add(r.NextState);
            }

            // Extrakce speciálních stavů
            var startState = uniqueStates.FirstOrDefault(s => s.Start);      
            var endState = uniqueStates.FirstOrDefault(s => s.End);

          
            // Seřazení vnitřních stavů abecedně, pro konzistentní kódování při každém spuštění
            var innerStates = uniqueStates
                .Where(s => !s.Start && !s.End)
                .OrderBy(s => s.Name)
                .ToList();

            var mapping = new Dictionary<string, int>();

            // Startovní stav má podle konvence index 1
            if (startState != null) mapping[startState.Name] = 1;

            // Vnitřní stavy mají indexy 2, 3,
            int idx = 2;
            foreach (var s in innerStates)
            {
                mapping[s.Name] = idx++;
            }

            //Koncový stav má poslední index
            if (endState != null) mapping[endState.Name] = idx;

            return mapping;
        }

     
        // Přiřadí čísel symbolům abecedy.
  
        private static Dictionary<string, int> CreateSymbolsMapping(List<Rule> rules)
        {
            var allSymbols = new HashSet<string>();
            foreach (var r in rules)
            {

                // Kontrola čtených i zapisovaných symbolů, pro kompletní abecedu
                foreach (var s in r.ReadSymbols) allSymbols.Add(s);
                foreach (var s in r.WriteSymbols) allSymbols.Add(s);
            }

            // Seřazení zajišťuje determinismus
            var sortedSymbols = allSymbols.OrderBy(s => s).ToList();
            
            var mapping = new Dictionary<string, int>();
            // Číslování symbolů od 1 - 0 slouží jako odělovač v unárním kódu
            for (int i = 0; i < sortedSymbols.Count; i++)
            {
                mapping[sortedSymbols[i]] = i + 1;
            }
            return mapping;
        }

        //mapování směrů.

        private static Dictionary<Direction, int> CreateDirectionsMapping()
        {
            return new Dictionary<Direction, int>
            {
                { Direction.Right, 1 }, // R
                { Direction.Left, 2 },  // L
                { Direction.Stay, 3 }   // S
            };
        }

        /// Generuje unární sekvenci: N nul následovaných M jedničkami.
        /// Příklad: EncodeZeroOne(3, 1) -> "0001" (reprezentuje číslo 3 s oddělovačem 1)
   
        private static string EncodeZeroOne(int n, int m = 1)
        {
            return new string('0', n) + new string('1', m);
        }





        public static string EncodeRulesToBinary(List<Rule?> rules)         // Hlavní metoda pro převod pravidel do binárního řetězce.
        {
            var validRules = rules.Where(r => r != null).Select(r => r!).ToList();
            if (!validRules.Any()) return "";

            var mappings = CreateEncodingMappings(validRules);

            // Lokální funkce pro zkrácení zápisu 
            // Převádí název stavu/symbolu na sekvenci nul a jedniček
            string SegStates(string name, int m = 1) => EncodeZeroOne(mappings.States[name], m);
            string SegSymbols(string sym, int m = 1) => EncodeZeroOne(mappings.Symbols[sym], m);
            
          


            StringBuilder output = new StringBuilder();
            output.Append("111"); // Globální prefix stroje

            int lastIdx = validRules.Count - 1;

            for (int i = 0; i < validRules.Count; i++)
            {
                var rule = validRules[i];
                bool isLastRule = (i == lastIdx);
                
                // Určení oddělovače  na konci pravidla:
                int termM = isLastRule ? 3 : 2; 

                // Aktuální stav
                output.Append(SegStates(rule.CurrentState.Name));

                //Čtené symboly (Inputs)
                foreach (var s in rule.ReadSymbols)
                {
                    output.Append(SegSymbols(s));
                }

                // Následující stav (Next State)
                output.Append(SegStates(rule.NextState.Name));

                // Zapisované symboly (Outputs) - oddělovač je "1"
                foreach (var s in rule.WriteSymbols)
                {
                     output.Append(SegSymbols(s, 1));
                }

                // Směry pohybu (Operations) 
                // Poslední směr nese ukončovací sekvenci celého pravidla (termM)
                for (int j = 0; j < rule.Operations.Count; j++)
                {
                    var op = rule.Operations[j];
                    bool isLastOp = (j == rule.Operations.Count - 1);
                    
         
                    int dirCode = mappings.Directions[op]; // Získání kód směru z mapování
                    
                
                    output.Append(EncodeZeroOne(dirCode, isLastOp ? termM : 1));
                }
            }

            return output.ToString();
        }
        

        // Vrací čitelnou reprezentaci mapování pro debugovací účely 

        public static Dictionary<string, Dictionary<string, string>> GetDebugMappings(List<Rule> rules)
        {
            var map = CreateEncodingMappings(rules);
            
            // Převedeme int mapování na string pro hezčí výpis v konzoli
            return new Dictionary<string, Dictionary<string, string>>
            {
                { "states", map.States.ToDictionary(k => k.Key, v => v.Value.ToString()) },
                { "symbols", map.Symbols.ToDictionary(k => k.Key, v => v.Value.ToString()) },
                { "directions", map.Directions.ToDictionary(k => k.Key.ToString(), v => v.Value.ToString()) }
            };
        }
    }
}