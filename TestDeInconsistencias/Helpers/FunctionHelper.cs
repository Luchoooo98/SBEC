using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestDeInconsistencias.Entities;

namespace TestDeInconsistencias.Helpers
{
    public static class FunctionHelper
    {
        public static bool AntecedentsOrConsequentsAreEqual(Rule rule1, Rule rule2, bool isAntecedentCheck)
        {
            string[] rule1Array;
            string[] rule2Array;

            if (isAntecedentCheck)
            {
                rule1Array = rule1.Antecedent.Replace(" ", "").Split(Constants.Constants.AND);
                rule2Array = rule2.Antecedent.Replace(" ", "").Split(Constants.Constants.AND);
            }
            else
            {
                rule1Array = rule1.Consequent.Replace(" ", "").Split(Constants.Constants.AND);
                rule2Array = rule2.Consequent.Replace(" ", "").Split(Constants.Constants.AND);
            }

            return AntecedentsOrConsequentsAreEqual(rule1Array, rule2Array);
        }

        public static bool AntecedentsOrConsequentsAreEqual(string[] rule1Array, string[] rule2Array)
        {

            if (rule1Array.Length != rule2Array.Length)
                return false;

            int index = 0;
            while (index < rule1Array.Length)
            {
                if (!rule2Array.Contains(rule1Array[index]))
                    return false;

                index += 1;
            }
            return true;
        }

        public static List<EqualComponentRule> GetPairsOfRulesWithEqualComponent(List<Rule> rules, bool isAntecedentCheck)
        {
            var equalComponentRulePairs = new List<EqualComponentRule>();

            rules.ForEach(rule =>
            {
                var index = rules.IndexOf(rule) + 1;
                for (int i = index; i < rules.Count; i++)
                {
                    if (AntecedentsOrConsequentsAreEqual(rule, rules[i], isAntecedentCheck))
                        equalComponentRulePairs.Add(new EqualComponentRule
                        {
                            Rule1 = rule,
                            Rule2 = rules[i],
                        });
                }
            });

            return equalComponentRulePairs;
        }

        public static void PrintRuleList(List<Rule> rules)
        {
            foreach (var rule in rules)
            {
                Console.WriteLine($"Regla {rule.Id}: {rule.Antecedent} => {rule.Consequent}");
            }
            Console.WriteLine("====================================================================\n");
        }

        //Inconsistencies methods

        //Precondiciones son equivalentes y UNA O MAS condiciones tambien lo son
        public static void FindReglasRedundates(List<EqualComponentRule> pairedRules)
        {
            Console.WriteLine("==Chequeando Reglas Redundantes==\n");

            pairedRules.ForEach(r =>
            {
                if (!AntecedentsOrConsequentsAreEqual(r.Rule1, r.Rule2, false))
                {
                    var rule1ConsArray = r.Rule1.Consequent.Replace(" ", "").Split(Constants.Constants.AND);
                    var rule2ConsArray = r.Rule2.Consequent.Replace(" ", "").Split(Constants.Constants.AND);

                    int index = 0;
                    var letter = "";
                    //Comparamos cada elemento del consecuente de la primera regla con la totalidad del conscuente
                    //de la segunda. Si hay match, se cumple la condicion de Regla Redundante
                    while (index != rule1ConsArray.Length)
                    {
                        letter = rule1ConsArray[index];
                        if (rule2ConsArray.Contains(letter))
                        {
                            Console.WriteLine($"{letter} de Regla -{r.Rule1.Id}- encontrada en Regla -{r.Rule2.Id}- . SON REDUNDANTES");
                            break;
                        }
                        index += 1;
                    }
                }
                else

                    Console.WriteLine($"Regla -{r.Rule1.Id}- Y Regla -{r.Rule2.Id}- tienen el mismo consecuente por lo que las reglas son iguales.");

            });
            Console.WriteLine("\n==Fin del análisis de reglas redundantes==\n");
        }

        //Precondiciones son equivalentes y las conclusiones son contradictorias (Q y noQ por ejemplo)
        public static void FindReglasConflictivas(List<EqualComponentRule> pairedRules)
        {
            Console.WriteLine("==Chequeando Reglas Conflictivas==\n");

            pairedRules.ForEach(r =>
            {
                if (!AntecedentsOrConsequentsAreEqual(r.Rule1, r.Rule2, false))
                {
                    var rule1ConsArray = r.Rule1.Consequent.Replace(" ", "").Split(Constants.Constants.AND);
                    var rule2ConsArray = r.Rule2.Consequent.Replace(" ", "").Split(Constants.Constants.AND);

                    var rule1ConsArrayInverted = new string[rule1ConsArray.Length];

                    //1) Tomar el consecuente de la Regla 1 e invertirlo
                    int index = 0;
                    for (index = 0; index < rule1ConsArray.Length; index++)
                    {
                        if (rule1ConsArray[index].Contains("no"))
                            rule1ConsArrayInverted[index] = rule1ConsArray[index].Replace("no", "");
                        else
                            rule1ConsArrayInverted[index] = $"no{rule1ConsArray[index]}";
                    }

                    //2) Si alguno de los elementos del consecuente invertido de 1 está en 2, es conflictiva
                    foreach (var letter in rule1ConsArrayInverted)
                    {
                        if (rule2ConsArray.Contains(letter))
                        {
                            Console.WriteLine($"{letter} de Regla -{r.Rule2.Id}- encontrado en Regla -{r.Rule1.Id}-. SON CONFLICTIVAS");
                            break;
                        }
                    }
                }
                else

                    Console.WriteLine($"Regla -{r.Rule1.Id}- Y Regla -{r.Rule2.Id}- tienen el mismo consecuente por lo que las reglas son iguales.");

            });

            Console.WriteLine("\n==Fin del análisis de reglas conflictivas==\n");
        }


        //Reglas incluidas en otras: Una esta incluida en otra si AMBAS TIENEN = CONSECUENTE
        // y las precondiciones de una se satisfacen, si las precondiciones de otra se satisfacen.
        // Si todos los elementos de la mas chica estan en la mas grande, está la mas chica incluida en la mas grande.
        // Si tienen el mismo Length, automaticamente sabemos que son la misma regla.
        public static void FindReglasIncluidasEnOtras(List<EqualComponentRule> pairedRules)
        {
            Console.WriteLine("==Chequeando Reglas Incluidas En Otras==\n");

            pairedRules.ForEach(r =>
            {
                if (!AntecedentsOrConsequentsAreEqual(r.Rule1, r.Rule2, true))
                {
                    //Se toman los antecedentes de cada regla separandolos en una lista.
                    var rule1AntArray = r.Rule1.Antecedent.Replace(" ", "").Split(Constants.Constants.AND).ToList();
                    var rule2AntArray = r.Rule2.Antecedent.Replace(" ", "").Split(Constants.Constants.AND).ToList();

                    //Se verifican la longitud de ambos, para saber cual es el que tenga menor longitud. El menor es el que se va a usar para comparar los elementos y ver si sus antecedentes estan incluidos en el segundo.
                    if (rule1AntArray.Count > rule2AntArray.Count)
                    {
                        ComparacionDeAntecedentesIncluidasEnOtras(r, rule2AntArray, rule1AntArray);
                    }

                    if (rule1AntArray.Count < rule2AntArray.Count)
                    {
                        ComparacionDeAntecedentesIncluidasEnOtras(r, rule1AntArray, rule2AntArray);
                    }

                    if (rule1AntArray.Count == rule2AntArray.Count)
                    {
                        ComparacionDeAntecedentesIncluidasEnOtras(r, rule1AntArray, rule2AntArray);
                    }
                }
                else
                    Console.WriteLine($"Regla -{r.Rule1.Id}- Y Regla -{r.Rule2.Id}- tienen el mismo antecedente por lo que las reglas son iguales.");

            });

            Console.WriteLine("\n==Fin del análisis de condiciones incluidas en otras==\n");
        }

        private static void ComparacionDeAntecedentesIncluidasEnOtras(EqualComponentRule r, List<string> reglaMenorLongitud, List<string> reglaMayorLongitud)
        {
            var inc = 0;
            string reglaPosibleEliminacion = r.Rule1.Antecedent.Length > r.Rule2.Antecedent.Length ? $"Por lo que la Regla {r.Rule1.Id} podria eliminarse." 
                                                                                : (r.Rule1.Antecedent.Length < r.Rule2.Antecedent.Length ? $"Por lo que la Regla {r.Rule2.Id} podria eliminarse."
                                                                                    : $"Por lo que cualquiera de las reglas podrian eliminarse.");
            //Se compara los elementos del primero con los del segundo para ver si existe uno dentro del otro.
            reglaMenorLongitud.ForEach(itemRule =>
            {
                var existe = reglaMayorLongitud.Where(x => x == itemRule).FirstOrDefault();
                if (existe != null)
                {
                    //si da resultado, incremento el contador para saber que ese elemento fue encontrado
                    inc++;
                }
            });

            //Si el contador es == a la longitud de la regla de menor longitud significa que todos sus elementos fueron encontrados y es una regla incluida en otra.
            if (inc == reglaMenorLongitud.Count)
            {
                Console.WriteLine($"Regla -{r.Rule1.Id}- Y -{r.Rule2.Id}- son un caso de Reglas Incluidas en Otras. {reglaPosibleEliminacion}");
            }
        }


        //Condiciones SI innecesarias: Ambas tienen = Consecuente, y una de las precondiciones de la primera es la negacion 
        // de una de las precondiciones de la segunda. Ademas, las otras precondiciones deben ser equivalentes.
        public static void FindCondicionesSiInnecesarias(List<EqualComponentRule> pairedRules)
        {
            Console.WriteLine("==Chequeando Condiciones SI Innecesarias==\n");

            pairedRules.ForEach(r =>
            {
                if (!AntecedentsOrConsequentsAreEqual(r.Rule1, r.Rule2, true))
                {
                    var rule1AntArray = r.Rule1.Antecedent.Replace(" ", "").Split(Constants.Constants.AND);
                    var rule2AntArray = r.Rule2.Antecedent.Replace(" ", "").Split(Constants.Constants.AND);
                    var rule1AntArrayInverted = new string[rule1AntArray.Length];


                    //El length de ambos antecedentes debe ser igual
                    if (rule1AntArray.Length == rule2AntArray.Length)
                    {
                        //Tomar el antecedente de la primera regla e invertirlo
                        bool isCondicionSiInnecesaria = false;
                        int index = 0;
                        while (!isCondicionSiInnecesaria && index < rule2AntArray.Length)
                        {
                            var letter = rule1AntArray[index];
                            var invertedLetter = letter.Contains("no") ? letter.Replace("no", "") : $"no{letter}";

                            //Si se encuentra, verificar que los antecedentes eliminando las letras que se oponen son iguales.
                            if (rule2AntArray.Contains(invertedLetter))
                            {
                                rule1AntArray = rule1AntArray.Where(l => l != letter).ToArray();
                                rule2AntArray = rule2AntArray.Where(l => l != invertedLetter).ToArray();

                                //Si lo son, entonces hay condición si innecesaria.
                                if (AntecedentsOrConsequentsAreEqual(rule1AntArray, rule2AntArray))
                                {
                                    Console.WriteLine($"Se encontró -{letter}- en Regla -{r.Rule1.Id}- y {invertedLetter} en Regla -{r.Rule2.Id}-. " +
                                    $"Además, el resto del antecedente es igual; por lo tanto hay Condiciones Si Innecesarias.");
                                    isCondicionSiInnecesaria = true;
                                }
                            }
                            index += 1;
                        }
                    }
                }
                else
                    Console.WriteLine($"Regla -{r.Rule1.Id}- Y Regla -{r.Rule2.Id}- tienen el mismo antecedente por lo que las reglas son iguales.");

            });

            Console.WriteLine("\n==Fin del análisis de condiciones si innecesarias==\n");
        }
    }
}
