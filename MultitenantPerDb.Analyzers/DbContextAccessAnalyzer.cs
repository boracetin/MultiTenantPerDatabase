using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace MultitenantPerDb.Analyzers;

/// <summary>
/// Analyzer that prevents unauthorized DbContext access
/// Only types implementing ICanAccessDbContext can inject DbContext
/// Diagnostic ID: MTDB001
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DbContextAccessAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MTDB001";
    
    private static readonly LocalizableString Title = "Unauthorized DbContext Access";
    private static readonly LocalizableString MessageFormat = 
        "Type '{0}' cannot access DbContext '{1}'. Only types implementing ICanAccessDbContext are allowed to inject DbContext.";
    private static readonly LocalizableString Description = 
        "Direct DbContext access is restricted. All database operations must go through the Repository pattern. " +
        "Only infrastructure components (Repository, UnitOfWork, Factories) implementing ICanAccessDbContext can access DbContext.";
    
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

        // Analyze constructor declarations
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        
        // Analyze field declarations
        context.RegisterSyntaxNodeAction(AnalyzeField, SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        var constructor = (ConstructorDeclarationSyntax)context.Node;
        
        // Get the containing type
        var containingType = context.SemanticModel.GetDeclaredSymbol(constructor)?.ContainingType;
        if (containingType == null)
            return;

        // Check if type implements ICanAccessDbContext
        if (ImplementsInterface(containingType, "ICanAccessDbContext"))
            return;

        // Check each parameter
        foreach (var parameter in constructor.ParameterList.Parameters)
        {
            var parameterType = context.SemanticModel.GetTypeInfo(parameter.Type!).Type;
            if (parameterType == null)
                continue;

            // Check if parameter is DbContext or derived from DbContext
            if (IsDbContextType(parameterType))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    parameter.GetLocation(),
                    containingType.Name,
                    parameterType.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        
        // Get the containing type
        var containingType = fieldDeclaration.Parent as TypeDeclarationSyntax;
        if (containingType == null)
            return;

        var typeSymbol = context.SemanticModel.GetDeclaredSymbol(containingType);
        if (typeSymbol == null)
            return;

        // Check if type implements ICanAccessDbContext
        if (ImplementsInterface(typeSymbol, "ICanAccessDbContext"))
            return;

        // Check field type
        var fieldType = context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type).Type;
        if (fieldType == null)
            return;

        if (IsDbContextType(fieldType))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                fieldDeclaration.Declaration.Type.GetLocation(),
                typeSymbol.Name,
                fieldType.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ImplementsInterface(INamedTypeSymbol type, string interfaceName)
    {
        return type.AllInterfaces.Any(i => i.Name == interfaceName);
    }

    private static bool IsDbContextType(ITypeSymbol type)
    {
        // Check if type is DbContext or inherits from DbContext
        var currentType = type;
        while (currentType != null)
        {
            if (currentType.Name == "DbContext" && 
                currentType.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore")
            {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }
}
