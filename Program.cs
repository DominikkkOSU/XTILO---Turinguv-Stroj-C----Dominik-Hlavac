using System;
using System.Collections.Generic;

namespace TSMachine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TURING MACHINE SIMULATOR ===");
            Console.WriteLine("Task: Copying Input with Double-Blank Detection");

            //DEFINICE STAVŮ
            var qStart = Config.Q_Start;   // Výchozí stav
            var qEnd = Config.Q_End;       // Koncový stav 
            
            // Pomocný stav. 
            var qCheck = new State("q_check"); 

            var rules = new List<Rule>();

            // --- 2. PRAVIDLA (Logika stroje) ---

         
            rules.Add(new Rule(qStart, qStart, new List<Action> {
                Op.R("1", "1"),              // Input páska: Přečti 1, zapiš 1, move right.
                Op.R(Config.Wildcard, "1"),  // Output páska: Na aktuální místo zapiš 1, posuň hlavu doprava.
                Op.S_Wild                    // Pomocná páska: Stay
            }));

        
            // Když 0, zapiš 0, posun dobrava, zůstaň v qStart.
            rules.Add(new Rule(qStart, qStart, new List<Action> {
                Op.R("0", "0"),          
                Op.R(Config.Wildcard, "0"), 
                Op.S_Wild
            }));

            // Když #, zapiš #, posun doprava, přepni se do q_check.
            rules.Add(new Rule(qStart, qCheck, new List<Action> {
                Op.R("#", "#"),             // Přečti #, posuň se dál.
                Op.R(Config.Wildcard, "#"), // Zapiš # na výstup.
                Op.S_Wild
            }));


            // Pokud po # následuje 1 nebo 0, zapiš 1, posun doprava, přepni se do qStart.
            rules.Add(new Rule(qCheck, qStart, new List<Action> {
                Op.R("1", "1"),             
                Op.R(Config.Wildcard, "1"), 
                Op.S_Wild
            }));
            rules.Add(new Rule(qCheck, qStart, new List<Action> {
                Op.R("0", "0"),          
                Op.R(Config.Wildcard, "0"), 
                Op.S_Wild
            }));

           
            // ## znamena konec pásky.
         
            rules.Add(new Rule(qCheck, qEnd, new List<Action> {
                Op.S("#", "#"),              // Už se nikam neposouvej.
                Op.S_Wild,                   
                Op.S_Wild
            }));


            //KONFIGURACE A SPUŠTĚNÍ ---
            // Vstupní data: "101" (data) + "#" (oddělovač) + "11" (data).
            // Poznámka: TS si na konci listu domyslí nekonečno dalších "#".
            var inputData = new List<string> { "1", "0", "1", "#", "1", "1" };
            
            // Příprava pásek (0 = vstup, 1 = výstup, 2 = pomocná)
            var tapes = new List<Tape> {
                new Tape(inputData), 
                new Tape(),          
                new Tape()           
            };

            // Hlavy čteček začínají na pozici 0
            var heads = new List<Head> { new Head(), new Head(), new Head() };

            // Inicializace samotného stroje
            var machine = new TuringMachine(rules, tapes, heads);

            Console.WriteLine($"\nInput Tape: {string.Join("", inputData)}");
            Console.WriteLine("Running simulation...\n");

            // Spuštění simulace. Začínáme v qStart.
            // MaxSteps 1000 je pojistka proti nekonečné smyčce 
            var result = machine.Run(qStart, maxSteps: 1000);

            // Výpis výsledků
            PrintResult(result);

            Console.ReadKey();
        }

        static void PrintResult(SimulationResult result)
        {
            Console.WriteLine("\n" + new string('=', 80));
            // Pokud stroj skončil ve stavu qEnd = Accepted
            if (result.Accepted)
                Console.WriteLine($"✓ ACCEPTED in {result.Steps} steps");
            else
                Console.WriteLine($"✗ REJECTED in {result.Steps} steps"); // Stroj se zasekl nebo došly kroky.
            Console.WriteLine(new string('=', 80));

            Console.WriteLine($"\nInitial Input: {result.InitialInput}");
            Console.WriteLine($"Final Output:  {result.FinalReturnContent}"); // Co je zapsáno na výstupní pásce

            Console.WriteLine("\n--- Executed Rules ---");
            int i = 0;
            // Výpis historie kroků pro ladění
            foreach(var r in result.RulesExecuted) Console.WriteLine($"{i++}: {r}");
            Console.WriteLine($"Total executed rules: {result.Steps}");

            Console.WriteLine("\n--- All Tape Contents and Head Positions ---");
        
            Console.WriteLine("\n--- Binary Encoded Rules ---");
            Console.WriteLine(result.EncodedRulesBinary);
            Console.WriteLine(new string('=', 80) + "\n");
            
            if (result.Accepted) Console.WriteLine("[SUCCESS] Machine halted in Accept state.");
        }
    }
}