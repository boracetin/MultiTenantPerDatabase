using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace MultitenantPerDb.Analyzers;

/// <summary>
/// Analyzer that enforces IUnitOfWork can only be used by classes implementing ICanAccessUnitOfWork
/// This is the CORE security rule - prevents unauthorized UnitOfWork access
/// Diagnostic ID: MTDB004
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnauthorizedUnitOfWorkAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "MTDB004";
    
    private static readonly LocalizableString Title = "Unauthorized IUnitOfWork usage";
    private static readonly LocalizableString MessageFormat = 
        "Class '{0}' cannot use IUnitOfWork because it does not implement ICanAccessUnitOfWork interface. Only authorized classes (inheriting from BaseService) can access UnitOfWork.";
    private static readonly LocalizableString Description = 
        "IUnitOfWork can only be used by classes that implement ICanAccessUnitOfWork interface. This ensures proper access control and prevents unauthorized database access.";
    
    private const string Category = "Security";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Error, // ERROR - This is a security violation!
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // Check constructor parameters
        context.RegisterSyntaxNodeAction(AnalyzeConstructor, SyntaxKind.ConstructorDeclaration);
        
        // Check field declarations
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        
        // Check property declarations
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    private static void AnalyzeConstructor(SyntaxNodeAnalysisContext context)
    {
        var constructor = (ConstructorDeclarationSyntax)context.Node;
        var containingClass = constructor.Parent as ClassDeclarationSyntax;
        
        if (containingClass == null)
            return;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
        if (classSymbol == null)
            return;

        // Check if class implements ICanAccessUnitOfWork
        if (ImplementsICanAccessUnitOfWork(classSymbol))
            return;

        // Check each constructor parameter
        foreach (var parameter in constructor.ParameterList.Parameters)
        {
            var parameterSymbol = context.SemanticModel.GetDeclaredSymbol(parameter);
            if (parameterSymbol == null)
                continue;

            // Check if parameter type is IUnitOfWork<T>
            if (IsUnitOfWorkInterfaceType(parameterSymbol.Type))
            {
                var diagnostic = Diagnostic.Create(
                    Rule,
                    parameter.GetLocation(),
                    classSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
        var containingClass = fieldDeclaration.Parent as ClassDeclarationSyntax;
        
        if (containingClass == null)
            return;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
        if (classSymbol == null)
            return;

        // Check if class implements ICanAccessUnitOfWork
        if (ImplementsICanAccessUnitOfWork(classSymbol))
            return;

        // Check field type
        var variableDeclaration = fieldDeclaration.Declaration;
        var typeInfo = context.SemanticModel.GetTypeInfo(variableDeclaration.Type);
        
        if (typeInfo.Type != null && IsUnitOfWorkInterfaceType(typeInfo.Type))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                variableDeclaration.Type.GetLocation(),
                classSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;
        var containingClass = propertyDeclaration.Parent as ClassDeclarationSyntax;
        
        if (containingClass == null)
            return;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(containingClass);
        if (classSymbol == null)
            return;

        // Check if class implements ICanAccessUnitOfWork
        if (ImplementsICanAccessUnitOfWork(classSymbol))
            return;

        // Check property type
        var typeInfo = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type);
        
        if (typeInfo.Type != null && IsUnitOfWorkInterfaceType(typeInfo.Type))
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                propertyDeclaration.Type.GetLocation(),
                classSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static bool ImplementsICanAccessUnitOfWork(INamedTypeSymbol type)
    {
        // Check if class implements ICanAccessUnitOfWork directly or through inheritance
        return type.AllInterfaces.Any(i => i.Name == "ICanAccessUnitOfWork");
    }

    private static bool IsUnitOfWorkInterfaceType(ITypeSymbol type)
    {
        // Check if type is IUnitOfWork<T>
        if (type is INamedTypeSymbol namedType)
        {
            return namedType.Name == "IUnitOfWork" || 
                   namedType.OriginalDefinition.Name == "IUnitOfWork";
        }
        
        return false;
    }
}
