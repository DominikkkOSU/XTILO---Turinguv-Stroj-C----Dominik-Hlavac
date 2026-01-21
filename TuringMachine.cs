using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSMachine
{
  
    
  
    public class SimulationResult
    {
        public bool Accepted { get; set; }
        public int Steps { get; set; }
        public string FinalReturnContent { get; set; }
        public string InitialInput { get; set; }
        
        // Debugging info: Mapování stavů/symbolů pro binární kodér
        public Dictionary<string, Dictionary<string, string>> EncodedRulesMappings { get; set; }
        public string EncodedRulesBinary { get; set; }
        public List<string> RulesExecuted { get; set; }
    }

  
   
    
    public class TuringMachine
    {
        public List<Rule> Rules { get; set; }
        public List<Tape> Tapes { get; set; }
        public List<Head> Heads { get; set; }
        public State CurrentState { get; set; }

        public TuringMachine(List<Rule> rules, List<Tape> tapes, List<Head> heads)
        {
            Rules = rules;
            Tapes = tapes;
            Heads = heads;
        }


       
    
        public List<string> ReadTape()  // Synchronně přečte symboly pod všemi hlavami.
        {
            var symbols = new List<string>();
            for (int i = 0; i < Tapes.Count; i++)
            {
                // Tapes[i].Symbols je typu InfiniteTapeBuffer 
                // Pokud na indexu nic není, vrátí výchozí Blank symbol.
                symbols.Add(Tapes[i].Symbols[Heads[i].Position]);
            }
            return symbols;
        }

  
        public void WriteTape(List<string> writeSymbols)
        {
            for (int i = 0; i < Tapes.Count; i++)
            {
                Tapes[i].Symbols[Heads[i].Position] = writeSymbols[i];
            }
        }

     
        public void MoveHead(List<Direction> operations)
        {
            for (int i = 0; i < Heads.Count; i++)
            {
                // Přetypování Enum na int umožňuje jednoduchou aritmetiku
                Heads[i].Position += (int)operations[i];
            }
        }


        // Najde první pravidlo, které odpovídá aktuálnímu stavu a přečteným symbolům.
        public Rule? ReadRule()
        {
            var currentTapeSymbols = ReadTape();
            
            // Procházíme pravidla. V deterministickém TS by mělo pasovat max jedno.
            // Pokud pasuje více, bere se první v seznamu (priorita definice).
            foreach (var rule in Rules)
            {
                if (rule.Matches(CurrentState, currentTapeSymbols))
                {
                    return rule;
                }
            }
                return null; // Žádné pravidlo = stroj se zasekne (Halt)
        }

     
        // Vykoná jedno konkrétní pravidlo (zápis, posun, změna stavu).
        // Řeší logiku Wildcards (*).

        public void RunRule(Rule rule)
        {
            var currentSymbols = ReadTape();
            var writeSymbols = new List<string>();

            // Rozlišení, co zapsat (řešení Wildcard)
            for (int i = 0; i < rule.Actions.Count; i++)
            {
                var action = rule.Actions[i];
                var currentSymbol = currentSymbols[i];

            

                // Pokd pravidlo píše zapiš Wildcard (*), hodnota se nemění
                if (action.ReadSymbol == Config.Wildcard)
                {
                    writeSymbols.Add(currentSymbol);
                }
                else
                {
                    writeSymbols.Add(action.WriteSymbol);
                }
            }

            // Zápis
            WriteTape(writeSymbols);
            
            // Posun
            var movements = rule.Actions.Select(a => a.Operation).ToList();
            MoveHead(movements);

            // Změna stavu
            CurrentState = rule.NextState;
        }

       
        /// Pojistka proti Halting Problem)
        public SimulationResult Run(State startState, int maxSteps = 100000)
        {
            int steps = 0;
            CurrentState = startState;
            
            var rulesHistory = new List<Rule?>();
            var rulesStrings = new List<string>();

            while (steps < maxSteps)
            {
                var rule = ReadRule();
                
            // Ukládání historie pro následnou analýzu nebo kódování
                rulesHistory.Add(rule);
                rulesStrings.Add(rule != null ? rule.ToString() : "None");

           

                // Pokud nenajde TS nenajde pravidlo/ dojde do stavu označeného jako End, zastaví se.
                if (rule == null || CurrentState.End)
                {
                    break;
                }

        
                RunRule(rule);
                steps++;
            }

            // Filtrace null pravidel pro kódování (pokud stroj skončil chybou, null je poslední)
            var validRules = rulesHistory.Where(r => r != null).Select(r => r!).ToList();

            var result = new SimulationResult
            {
                Accepted = CurrentState.End,
                Steps = steps,
                FinalReturnContent = Tapes.Last().Stripped(), // Předpoklad, že výsledek je na poslední pásce
                InitialInput = Tapes.First().InputStr,
                
                // Získání debug informací z helperu 
                EncodedRulesMappings = (validRules.Any()) 
                    ? EncodingHelper.GetDebugMappings(validRules) 
                    : new Dictionary<string, Dictionary<string, string>>(),
                
                EncodedRulesBinary = EncodingHelper.EncodeRulesToBinary(this.Rules),
                
                RulesExecuted = rulesStrings.Where(r => r != "None").ToList()
            };

            PrintSummary(result);
            return result;
        }

     
   // Print vizualní reprezentaci pásek do konzole.
        public void PrintTape()
        {
            for (int i = 0; i < Tapes.Count; i++)
            {
                var tape = Tapes[i];
                var head = Heads[i];
                var data = tape.Symbols.ToDictionary();
                
             
                // Výpočet "viewportu". Protože páska je nekonečná (Dictionary), musíme najít min/max index,
                // který obsahuje data nebo kde stojí hlava, abychom nevykres
                int minIdx = data.Keys.Count > 0 ? Math.Min(data.Keys.Min(), head.Position) : head.Position;
                int maxIdx = data.Keys.Count > 0 ? Math.Max(data.Keys.Max(), head.Position) : head.Position;

                //malý okraj pro kontext
                minIdx -= 1;
                maxIdx += 1;

                StringBuilder sb = new StringBuilder();
                sb.Append("## ");

                for (int pos = minIdx; pos <= maxIdx; pos++)
                {
                    string symbol = tape.Symbols[pos];
                    
                    if (pos == head.Position)
                    {
                        sb.Append($"[{symbol}]"); // Zvýraznění pozice hlavy
                    }
                    else
                    {
                        sb.Append($" {symbol} ");
                    }
                }
                sb.Append(" ##");

                Console.WriteLine($"Tape {i}: {sb.ToString()}");
            }
        }

        private void PrintSummary(SimulationResult result)
        {
            Console.WriteLine("\n" + new string('=', 80));
            if (result.Accepted)
                Console.WriteLine($"✓ ACCEPTED in {result.Steps} steps");
            else
                Console.WriteLine($"✗ REJECTED in {result.Steps} steps");
            Console.WriteLine(new string('=', 80));

            Console.WriteLine($"\nInitial Input: {result.InitialInput}");
            Console.WriteLine($"Final Output:  {result.FinalReturnContent}");

            Console.WriteLine("\n--- Executed Rules ---");
            if (result.RulesExecuted != null && result.RulesExecuted.Count > 0)
            {
                // Výpis prvních X pravidel nebo všech, pokud jich není moc
                int limit = 20; 
                for (int i = 0; i < Math.Min(result.RulesExecuted.Count, limit); i++)
                {
                    Console.WriteLine($"{i}: {result.RulesExecuted[i]}");
                }
                if (result.RulesExecuted.Count > limit) Console.WriteLine($"... (+{result.RulesExecuted.Count - limit} more)");
            }
            else
            {
                Console.WriteLine("(No rules executed)");
            }

            Console.WriteLine("\n--- All Tape Contents and Head Positions ---");
            PrintTape();

            // Výpis mapování pro kontrolu kódování
            Console.WriteLine("\n--- Encoding Information ---");
            if (result.EncodedRulesMappings != null)
            {
                foreach (var category in result.EncodedRulesMappings)
                {
                    Console.WriteLine($"{category.Key}:");
                    foreach (var item in category.Value)
                    {
                        Console.WriteLine($"  {item.Key} -> {item.Value}");
                    }
                }
            }

            Console.WriteLine("\n--- Binary Encoded Rules ---");
            Console.WriteLine(result.EncodedRulesBinary);
            Console.WriteLine(new string('=', 80) + "\n");
        }
    }
}