using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace MultitenantPerDb.Analyzers;

/// <summary>
/// Analyzer that prevents unauthorized UnitOfWork access in Controllers
/// Controllers should use Services, not UnitOfWork directly
/// Diagnostic ID: MTDB002
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnitOfWorkAccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MTDB002";
    
    private static readonly LocalizableString Title = "Unauthorized UnitOfWork Access in Controller";
    private static readonly LocalizableString MessageFormat = 
        "Controller '{0}' cannot access IUnitOfWork. Controllers should use Service layer, not UnitOfWork directly.";
    private static readonly LocalizableString Description = 
        "Controllers should not access IUnitOfWork directly. Use Service layer for business logic and data access.";
    
    private const string Category = "Architecture";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        var constructor = (ConstructorDeclarationSyntax)context.Node;
        
        // Get the containing type
        var containingType = context.SemanticModel.GetDeclaredSymbol(constructor)?.ContainingType;
        if (containingType == null)
            return;

        // Check if this is a Controller
        if (!IsController(containingType))
            return;

        // Check each parameter
        foreach (var parameter in constructor.ParameterList.Parameters)
        {
            var parameterType = context.SemanticModel.GetTypeInfo(parameter.Type!).Type;
            if (parameterType == null)
                continue;

            // Check if parameter is IUnitOfWork
            if (IsUnitOfWorkType(parameterType))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    parameter.GetLocation(),
                    containingType.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsController(INamedTypeSymbol type)
    {
        // Check if class name ends with "Controller"
        if (type.Name.EndsWith("Controller"))
            return true;

        // Check if inherits from ControllerBase or Controller
        var currentType = type.BaseType;
        while (currentType != null)
        {
            if (currentType.Name == "ControllerBase" || currentType.Name == "Controller")
                return true;
            currentType = currentType.BaseType;
        }

        return false;
    }

    private static bool IsUnitOfWorkType(ITypeSymbol type)
    {
        // Check if type is IUnitOfWork<T>
        if (type is INamedTypeSymbol namedType)
        {
            if (namedType.Name == "IUnitOfWork" || namedType.OriginalDefinition.Name == "IUnitOfWork")
                return true;
        }

        return false;
    }
}
