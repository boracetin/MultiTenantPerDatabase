using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace MultitenantPerDb.Analyzers;

/// <summary>
/// Analyzer that enforces Service classes to inherit from BaseService
/// Diagnostic ID: MTDB003
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ServiceInheritanceAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MTDB003";
    
    private static readonly LocalizableString Title = "Service must inherit from BaseService";
    private static readonly LocalizableString MessageFormat = 
        "Service class '{0}' should inherit from BaseService<TDbContext>. This ensures proper UnitOfWork access control.";
    private static readonly LocalizableString Description = 
        "All service implementation classes should inherit from BaseService<TDbContext> to maintain architectural consistency and access control.";
    
    private const string Category = "Architecture";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning, // Warning, not Error (more flexible)
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);
        
        if (classSymbol == null)
            return;

        // Skip if abstract class or interface
        if (classSymbol.IsAbstract || classSymbol.TypeKind == TypeKind.Interface)
            return;

        // Check if class name ends with "Service"
        if (!classSymbol.Name.EndsWith("Service"))
            return;

        // Check if in Application.Services namespace
        var ns = classSymbol.ContainingNamespace?.ToDisplayString() ?? "";
        if (!ns.Contains(".Application.Services"))
            return;

        // Check if already inherits from BaseService
        if (InheritsFromBaseService(classSymbol))
            return;

        // Report diagnostic
        var diagnostic = Diagnostic.Create(
            Rule,
            classDeclaration.Identifier.GetLocation(),
            classSymbol.Name);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool InheritsFromBaseService(INamedTypeSymbol type)
    {
        var currentType = type.BaseType;
        while (currentType != null)
        {
            if (currentType.Name == "BaseService" || currentType.OriginalDefinition.Name == "BaseService")
                return true;
            currentType = currentType.BaseType;
        }
        return false;
    }
}
