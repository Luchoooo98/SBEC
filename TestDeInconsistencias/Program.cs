using System;
using TestDeInconsistencias.Helpers;

namespace TestDeInconsistencias
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var rules = FileHelper.GetRuleList(Constants.Constants.FILE_ROUTE);
                var equalAntecedentRules = FunctionHelper.GetPairsOfRulesWithEqualComponent(rules, true);
                var equalConsequentRules = FunctionHelper.GetPairsOfRulesWithEqualComponent(rules, false);

                var option = 10;
                while (option != 0)
                {
                    Console.WindowWidth = 150;
                    Console.WriteLine("Reglas: ");
                    FunctionHelper.PrintRuleList(rules);
                    Console.WriteLine("====Detector de Inconsistencias====");
                    Console.WriteLine("Elija opcion:\n 1) Reglas Redundates\n 2) Reglas Conflictivas\n 3) Reglas Incluidas En Otras\n 4) Condiciones SI Innecesarias\n 0) Salir ");
                    Console.Write("==> ");
                    option = Convert.ToInt32(Console.ReadLine());
                    Console.Clear();
                    switch (option)
                    {
                        case 1:
                            FunctionHelper.FindReglasRedundates(equalAntecedentRules);
                            break;
                        case 2:
                            FunctionHelper.FindReglasConflictivas(equalAntecedentRules);
                            break;
                        case 3:
                            FunctionHelper.FindReglasIncluidasEnOtras(equalConsequentRules);
                            break;
                        case 4:
                            FunctionHelper.FindCondicionesSiInnecesarias(equalConsequentRules);
                            break;
                        case 0:
                            Console.WriteLine("Saliendo...");
                            break;
                        default:
                            Console.WriteLine("Opción no reconocida. Intente de nuevo");
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }
    }
}
