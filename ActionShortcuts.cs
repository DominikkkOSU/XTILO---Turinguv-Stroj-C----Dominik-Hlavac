using System;
using TSMachine; 

namespace TSMachine
{
  
    // Slouží ke zkrácení zápisu pravidel (Transition Function) v definici Turingova stroje.

    public static class Op
    {


      // záis akce, přečtením zapsání a posunu DOPRAVA
         public static Action R(string read, string write) 
        {
            return new Action(read, write, Direction.Right);
        }
      // záis akce, přečtením zapsání a posunu DOLEVA

        public static Action L(string read, string write)
        {
            return new Action(read, write, Direction.Left);
        }

       // záis akce, přečtením zapsání a zůstání na místě
        public static Action S(string read, string write)
        {
            return new Action(read, write, Direction.Stay);
        }

        // Předdefinované zkratky (Shortcuts)
        // Tyto vlastnosti slouží pro  zkrácení zápisu nejčastějších operací.
        // Umožňují psát např. 'Op.R11' místo 'Op.R("1", "1")'.

     //Předdefinované zkratky,  vlastnosti pro nejčastější operace

        public static Action R11 => R("1", "1"); // Identita na 1 + posun vpravo
        public static Action R00 => R("0", "0"); // Identita na 0 + posun vpravo
        public static Action R10 => R("1", "0"); // Přepis 1 -> 0 + posun vpravo
        


        // Zkratka pro: Přečtení  (*), zapsání to samé (*), posun do prava
        // Přeskočení jednoho znaku doprava bez změny
        public static Action R_Wild => R(Config.Wildcard, Config.Wildcard); 


// kratka pro: Přečtení  (*), zapsání to samé (*), stání na místě
        public static Action S_Wild => S(Config.Wildcard, Config.Wildcard);
    }
}