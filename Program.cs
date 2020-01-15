using System;
using System.Linq;
using System.Threading.Tasks;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AddWarmUpMethodToCctor
{
    public sealed class Program : ConsoleAppBase
    {
        private static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder().RunConsoleAppFrameworkAsync<Program>(args);
        }

        [Command("add")]
        public void AddWarmUp(
            [Option(0, ".NET CLR dll/exe file path")] string path,
            [Option("o", "Output .NET CLR dll/exe file path")] string outputPath = null
        )
        {
            try
            {
                var isSamePath = outputPath is null || string.CompareOrdinal(path, outputPath) == 0;
                var module = ModuleDefinition.ReadModule(path, new ReaderParameters()
                {
                    ReadWrite = isSamePath
                });
                foreach (var typeDefinition in module.Types.Where(FilterType))
                {
                    ModifyCctor(typeDefinition);
                }
                if (isSamePath)
                {
                    module.Write();
                }
                else
                {
                    module.Write(outputPath);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }

        private bool FilterField(FieldDefinition field) => field.IsStatic && !field.IsLiteral && field.FieldType.IsValueType;
        private bool FilterType(TypeDefinition typeDefinition) => typeDefinition.Methods.Any(FilterCctor) && typeDefinition.Fields.Any(FilterField);
        private bool FilterCctor(MethodDefinition method) => method.Name == ".cctor" && method.IsStatic && method.IsRuntimeSpecialName && method.IsSpecialName && method.HasBody;

        private void ModifyCctor(TypeDefinition typeDefinition)
        {
            var cctor = typeDefinition.Methods.First(FilterCctor);
            var body = cctor.Body;
            var warmUp = AddWarmUp(typeDefinition);
            var processor = body.GetILProcessor();
            var instructions = body.Instructions;
            var callWarmUp = Instruction.Create(OpCodes.Call, warmUp);
            processor.InsertBefore(instructions[0], callWarmUp);
        }

        private static MethodDefinition AddWarmUp(TypeDefinition typeDefinition)
        {
            var module = typeDefinition.Module;
            var warmUp = new MethodDefinition("<>WarmUp", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, module.TypeSystem.Void);
            warmUp.Body.GetILProcessor().Append(Instruction.Create(OpCodes.Ret));
            typeDefinition.Methods.Add(warmUp);
            return warmUp;
        }
    }
}
