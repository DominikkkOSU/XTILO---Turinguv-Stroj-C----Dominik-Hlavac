using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSMachine
{

    // Globální konfigurace simulátoru.

    public static class Config
    {
        public const string Blank = "#"; 
        public const string Wildcard = "*"; 

        

        public static readonly State Q_Start = new State("q_start", Start: true);
        public static readonly State Q_End = new State("q_end", End: true);
    }




    public enum Direction
    {
        Left = -1,
        Stay = 0,
        Right = 1
    }


    // Reprezentace stavu automatu.
    // Použití 'record', aby došlo k automatickému fungování value-based equality 

    public record State(string Name, bool Start = false, bool End = false)
    {
        public override string ToString() => Name;
    }



    public class Action // akce na pásce
    {
        public string ReadSymbol { get; set; }
        public string WriteSymbol { get; set; }
        public Direction Operation { get; set; }

        public Action(string readSymbol, string writeSymbol, Direction operation)
        {
            ReadSymbol = readSymbol;
            WriteSymbol = writeSymbol;
            Operation = operation;
        }


        public bool Matches(string symbol)
        {
            // Logika shody:
            // 1. Pravidlo očekává * (bere cokoliv)
            // 2. Na pásce je * (pokud by vstup obsahoval wildcardy)
            // 3. Přesná shoda symbolů
            return ReadSymbol == Config.Wildcard || symbol == Config.Wildcard || ReadSymbol == symbol;
        }
    }

    // Wrapper nad Dictionary simulující nekonečnou pásku.
    // sparse array - Ukládají se jen neprázdné buňky.

    public class InfiniteTapeBuffer
    {
        private readonly Dictionary<int, string> _data = new Dictionary<int, string>();


        // Indexer pro bezpečný přístup. Pokud index v paměti není, vrátí (#).
  
        public string this[int index]
        {
            get => _data.ContainsKey(index) ? _data[index] : Config.Blank;
            set => _data[index] = value;
        }

        public Dictionary<int, string> ToDictionary() => _data;
    }


 

    public class Tape    // Celá páska včetně vstupních dat.
    {
        public InfiniteTapeBuffer Symbols { get; set; }
        public string InputStr { get; private set; }

        public Tape(List<string> inputStr = null)
        {
            Symbols = new InfiniteTapeBuffer();
            
            if (inputStr != null)
            {
                InputStr = string.Join("", inputStr);
                // Inicializace pásky vstupním řetězcem
                for (int i = 0; i < inputStr.Count; i++)
                {
                    Symbols[i] = inputStr[i];
                }
            }
            else
            {
                InputStr = "";
            }
        }

        // Vypíše aktuální obsah pásky seřazený podle indexů.
        // Zahrnuje i prázdná místa mezi zapsanými symboly, pokud existují.

        public override string ToString()
        {
            var data = Symbols.ToDictionary();
            if (data.Count == 0) return "";

            var sortedKeys = data.Keys.OrderBy(k => k);
            StringBuilder sb = new StringBuilder();
            foreach (var key in sortedKeys)
            {
                sb.Append(data[key]);
            }
            return sb.ToString();
        }

        // Vrátí obsah pásky očištěný o prázdné symboly 
        // Potřebné pro validaci finálního výstupu stroje.

        public string Stripped()
        {
            var data = Symbols.ToDictionary();
            var sortedKeys = data.Keys.OrderBy(k => k);
            
            StringBuilder sb = new StringBuilder();
            foreach (var key in sortedKeys)
            {
                string val = data[key];
                if (val != Config.Blank)
                {
                    sb.Append(val);
                }
            }
            return sb.ToString();
        }
    }




    public class Head
    {
        public int Position { get; set; } = 0;
    }


    // Přechodové pravidlo
    // Mapuje (CurrentState, ReadSymbols) -> (NextState, WriteSymbols, Moves).
  
    public class Rule
    {
        public State CurrentState { get; set; }
        public State NextState { get; set; }
        public List<Action> Actions { get; set; }

        // Pomocné gettery pro snadnější debuggování a výpis
        public List<string> ReadSymbols => Actions.Select(a => a.ReadSymbol).ToList();
        public List<string> WriteSymbols => Actions.Select(a => a.WriteSymbol).ToList();
        public List<Direction> Operations => Actions.Select(a => a.Operation).ToList();

        public Rule(State currentState, State nextState, List<Action> actions)
        {
            CurrentState = currentState;
            NextState = nextState;
            Actions = actions;
        }


        // Kontrola, zda lze toto pravidlo aplikovat na aktuální stav stroje.
        public bool Matches(State state, List<string> symbols)
        {
            //Kontrola stavu
            if (state != CurrentState) return false;

            //Kontrola počtu pásek a definice pravidla 
            if (Actions.Count != symbols.Count) return false;

            //Kontrola symbolů pod hlavami
            for (int i = 0; i < Actions.Count; i++)
            {
                // Delegování kontroly  na třídu Action
                if (!Actions[i].Matches(symbols[i]))
                {
                    return false; 
                }
            }
            return true;
        }

        public override string ToString()
        {
            string readStr = string.Join(", ", ReadSymbols);
            string writeStr = string.Join(", ", WriteSymbols);
            string opStr = string.Join(", ", Operations);

            return $"δ({CurrentState.Name}, ({readStr})) = ({NextState.Name}, ({writeStr}), ({opStr}))";
        }
    }
}