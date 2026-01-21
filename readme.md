# XTILO - Teoretická informatika a logika
# Simulátor deterministického k-páskového Turingova stroje 

Tento semestrální projekt je implementací deterministického Turingova stroje, s podporou více pásek. Pro realizaci úkolu jsem použil programovací jazyk C#. 

Spuštění programu demonstruje funkčnost simulátoru na úloze zpracování binárního vstupu.

## Funkce řešení

**Flexibilní definice Turingova stroje** - Definuje stavy, abecedu, počáteční/koncové stavy a přechodové funkce.
**K-Paskový přístup** - Podpora libovolného počtu pásek.
**Pravidla wild card** - Implementace speciálního symbolu * pro ulehčení definice pravidel
**Simulace** - Krokování výpočtu s vizualizací stavu pásky a pozice hlavy v terminálu.
**kodování Turingova stroje** - Automatické generování binarního kódu TS, který znázorňuje použitá pravidla

# Struktura projektu

Projekt je rozdělený do několika modulů:


**`Program.cs`**: Vstupní bod aplikace, obsahuje konkrétní instance  Turingova stroje: stavy, pravidla a spouští simulaci.
**`TuringMachine.cs`**: Jádro simulátoru. Stará se o "read-write-move" cyklus a o pravidla aplikace.
**`MachineComponents.cs`**: Definice struktury dat:
    * `State`: Reprezentace stavu - název, start/end.
    * `Tape`: Reprezentace nekonečné pásky.
    * `Rule`: Definice přechodové funkce.
**`Encoding.cs`**: Logika pro převod pravidel stroje do unárně-binárního řetězce.
**`ActionShortcuts.cs`**: Pomocná statická třída pro čitelnější zápis pravidel.



## Popis algoritmu

Zadání semestrální práce zmiňovalo realizaci funkce pro násobení: $$fun(x_1, \dots, x_n) = \prod_{i=1}^{n} x_i$$

Vzhledem ke komplexitě implementace aritmetických operací z důvodu časové náročnosti je implementována **demonstrační logika validace a přenosu dat (Identity Operation)**.

### Cíl demonstrace
Implementace slouží jako "Proof of Concept", který ověřuje správnost samotného simulátoru (engine). Stroj demonstruje:
1. Schopnost práce se 3 páskami (Input, Accumulator, Aux).
2. Rozpoznání struktury vstupu (binární čísla oddělená `#`).
3. Manipulaci s daty (čtení z jedné pásky a zápis na druhou).
4. Generování unárního kódu stroje na závěr výpočtu.

### Konfigurace pásek
Pro realizaci výpočtu využívá stroj **3 pásky**:
1.  **Páska 0 (INPUT):** Obsahuje vstupní data. Hlava čte jednotlivá čísla zleva doprava.
2.  **Páska 1 (ACCUMULATOR):** Slouží pro zápis zpracovaných dat.
3.  **Páska 2 (AUXILIARY):** Pomocná páska.


### Princip činnosti
Algoritmus pracuje v cyklu:
1. Hlava čte symboly na **Pásce 0**.
2. Pokud přečte datový symbol (`0` nebo `1`), **zkopíruje jej na Pásku 1** (demonstrace přenosu informace).
3. Pokud narazí na oddělovač `#`, přeskočí jej a pokračuje ve čtení.
4. Proces končí přechodem do koncového stavu, jakmile jsou všechna data ze vstupu přečtena a přenesena.

## Kódování Turingova stroje

Program na závěr simulace vypíše zakódovanou podobu stroje pomocí **unárního kódování** indexů oddělené separátory.

* **Formát:** `111 code(rule1) 11 code(rule2) 11 ... 111`
* **Kódování pravidla:**
    `0^n 1 0^m 1 ...` (kde počet nul reprezentuje index stavu nebo symbolu).

Třída `EncodingHelper` automaticky mapuje všechny použité stavy a symboly na unikátní indexy a generuje výsledný řetězec.

# Spuštění programu

Projekt je typu .NET Console Application.


**Prerekvizity:**
* .NET SDK (verze 6.0 nebo novější)

**Spuštění z terminálu:**
```bash 
dotnet run 
```




# Zdroje
1. https://sites.google.com/view/7tilo-25/ p%C5%99edn%C3%A1%C5%A1ky - Přednášky z XTILO
2. https://turingmachinesimulator.com/ - Nástroj pro vizualizaci a debugování logiky TS. Sloužil pro ověření správnosti navržených přechodových funkcí před jejich přepsáním do kódu
3. Umělá inteligence - Sloužilo jako 'virtuální konzultant' pro pochopení složitějších částí zadání a přednášek. Dále jsem AI využil pro návrh kostry projektu a kontrolu gramatiky v dokumentaci.