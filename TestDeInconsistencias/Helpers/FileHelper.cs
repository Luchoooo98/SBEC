using System;
using System.Collections.Generic;

namespace TestDeInconsistencias.Helpers
{
    public static class FileHelper
    {
        public static List<Entities.Rule> GetRuleList(string route)
        {
            Console.WriteLine("Leyendo archivo de reglas...");
            string[] lines = System.IO.File.ReadAllLines(route);
            var rules = new List<Entities.Rule>();
            foreach (string line in lines)
            {

                var aFrom = line.IndexOf(Constants.Constants.RULE_START) + Constants.Constants.RULE_START.Length;
                var fullAntecedent = line.Substring(aFrom, line.IndexOf(Constants.Constants.RULE_DIVIDER) - aFrom).Trim();

                var cFrom = line.IndexOf(Constants.Constants.RULE_DIVIDER) + Constants.Constants.RULE_DIVIDER.Length;
                var fullConsequent = line.Substring(cFrom).Trim();

                rules.Add(new Entities.Rule { Antecedent = fullAntecedent, Consequent = fullConsequent, Id = rules.Count + 1 });

            }
            Console.WriteLine("Archivo leído con exito");
            return rules;
        }
    }
}
