using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArgumentGenerator
{
    [Generator]
    public class ArgumentMapGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {

        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;
            var handler = syntaxTrees
                .FirstOrDefault(tree => tree.GetText().ToString().Contains("Main(string[] args)"));
            if (handler == null) return;
            var root = handler.GetRoot();
            var mainMethod = (from methodDeclaration in root.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Main"
                select methodDeclaration).FirstOrDefault();
            if (mainMethod == null) return;
            var statementDs =
                mainMethod.Body.Statements.FirstOrDefault(s => s.ToString().Contains("Parser.Default.ParseArguments"))
                    as LocalDeclarationStatementSyntax;
            if (statementDs == null) return;

            if (statementDs.Declaration.Variables[0].Initializer is { } init)
            {
                if (init.Value is InvocationExpressionSyntax exp)
                {
                    int count = exp.ArgumentList.Arguments.Count - 1;
                    CreateExtensions(context, count);
                }
            }
        }

        private void CreateExtensions(GeneratorExecutionContext context, int count)
        {
            if (count < 17) return;
            string parameterList = string.Join(",",
                Enumerable.Range(1, count).Select(i => "T" + i.ToString()));
            string typeofList = string.Join(",",
                Enumerable.Range(1, count).Select(i => $"typeof(T{i})"));
            string funcList = string.Join(",",
                Enumerable.Range(1, count).Select(i => $"Func<T{i}, TResult> parsedFunc{i}"));
            funcList += ", Func<IEnumerable<Error>, TResult> notParsedFunc";

            string bodyList = string.Join("\n",
                Enumerable.Range(1, count)
                    .Select(i => $"if(parsed.Value is T{i} t{i}) \n{{ \nreturn parsedFunc{i}(t{i}); \n}}"));
            var stringBuilder = new StringBuilder($@"
namespace Tindo.UgitCLI
{{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;
    internal static class GenExtensions
    {{
        public static ParserResult<object> ParseArguments<{parameterList}>(
                this Parser parser, IEnumerable<string> args)
        {{
            if (parser == null)
            {{
                 throw new ArgumentNullException(nameof(parser));
            }}
            
            return parser.ParseArguments(args, new[]
            {{
                {typeofList}
            }});
        }}

        public static TResult MapResult<{parameterList}, TResult>(this ParserResult<object> result, {funcList})
        {{
            if (result is Parsed<object> parsed)
            {{
                 {bodyList}

                 throw new InvalidOperationException();
            }}

            return notParsedFunc(((NotParsed<object>) result).Errors); 
        }}
    }} 
}}
");
            context.AddSource("ArgumentGenerator", SourceText.From(stringBuilder.ToString(),
                Encoding.UTF8));
        }

    }
}
    
