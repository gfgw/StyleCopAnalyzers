﻿namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// The opening parenthesis or bracket in a call to a C# method or indexer, or the declaration of a method or
    /// indexer, is not placed on the same line as the method or indexer name.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when the opening bracket of a method or indexer call or declaration is not
    /// placed on the same line as the method or indexer. The following examples show correct placement of the opening
    /// bracket:</para>
    /// <code language="csharp">
    /// public string JoinName(string first, string last)
    /// {
    ///     return JoinStrings(
    ///         first, last);
    /// }
    ///
    /// public int this[int x]
    /// {
    ///     get { return this.items[x]; }
    /// }
    /// </code>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1110OpeningParenthesisMustBeOnDeclarationLine : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="SA1110OpeningParenthesisMustBeOnDeclarationLine"/>
        /// analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1110";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReadabilityResources.SA1110Title), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReadabilityResources.SA1110MessageFormat), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReadabilityResources.SA1110Description), ReadabilityResources.ResourceManager, typeof(ReadabilityResources));
        private static readonly string HelpLink = "http://www.stylecop.com/docs/SA1110.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.ReadabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleMethodDeclaration, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleInvocationExpression, SyntaxKind.InvocationExpression);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleIndexerDeclaration, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleElementAccessExpression, SyntaxKind.ElementAccessExpression);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleAttribute, SyntaxKind.Attribute);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleDelegateDeclaration, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleAnonymousMethod, SyntaxKind.AnonymousMethodExpression);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleArrayCreation, SyntaxKind.ArrayCreationExpression);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleOperatorDeclaration, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleConversionOperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
        }

        private void HandleConversionOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var conversionOperator = (ConversionOperatorDeclarationSyntax)context.Node;

            var identifierName = conversionOperator.ChildNodes()
                .OfType<IdentifierNameSyntax>()
                .FirstOrDefault();
            if (identifierName == null || identifierName.Identifier.IsMissing)
            {
                return;
            }

            var parameterListSyntax = conversionOperator.ParameterList;

            if (parameterListSyntax != null && !parameterListSyntax.OpenParenToken.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context, parameterListSyntax.OpenParenToken, identifierName.Identifier);
            }
        }

        private void HandleOperatorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var operatorDeclaration = (OperatorDeclarationSyntax)context.Node;

            if (operatorDeclaration.OperatorToken.IsMissing)
            {
                return;
            }

            var parameterListSyntax = operatorDeclaration.ParameterList;

            if (parameterListSyntax != null && !parameterListSyntax.OpenParenToken.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context, parameterListSyntax.OpenParenToken, operatorDeclaration.OperatorToken);
            }
        }

        private void HandleArrayCreation(SyntaxNodeAnalysisContext context)
        {
            var array = (ArrayCreationExpressionSyntax)context.Node;

            if (array.Type.IsMissing ||
                array.Type.ElementType == null ||
                !array.Type.RankSpecifiers.Any())
            {
                return;
            }

            var firstSize = array.Type.RankSpecifiers.First();

            if (!firstSize.OpenBracketToken.IsMissing)
            {
                CheckIfLocationOfExpressionAndOpenTokenAreTheSame(context, firstSize.OpenBracketToken, array.Type.ElementType);
            }
        }

        private void HandleAnonymousMethod(SyntaxNodeAnalysisContext context)
        {
            var anonymousMethod = (AnonymousMethodExpressionSyntax)context.Node;

            if (anonymousMethod.DelegateKeyword.IsMissing ||
                anonymousMethod.ParameterList == null ||
                anonymousMethod.ParameterList.IsMissing ||
                anonymousMethod.ParameterList.OpenParenToken.IsMissing)
            {
                return;
            }

            CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context, anonymousMethod.ParameterList.OpenParenToken, anonymousMethod.DelegateKeyword);
        }

        private void HandleDelegateDeclaration(SyntaxNodeAnalysisContext context)
        {
            var delegateDeclaration = (DelegateDeclarationSyntax)context.Node;

            if (!delegateDeclaration.Identifier.IsMissing &&
                delegateDeclaration.ParameterList != null &&
                !delegateDeclaration.ParameterList.OpenParenToken.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context,
                    delegateDeclaration.ParameterList.OpenParenToken, delegateDeclaration.Identifier);
            }
        }

        private void HandleAttribute(SyntaxNodeAnalysisContext context)
        {
            var attribute = (AttributeSyntax)context.Node;

            var qualifiedNameSyntax = attribute.ChildNodes().OfType<QualifiedNameSyntax>().FirstOrDefault();
            IdentifierNameSyntax identifierNameSyntax = null;
            if (qualifiedNameSyntax != null)
            {
                identifierNameSyntax = qualifiedNameSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().LastOrDefault();
            }
            else
            {
                identifierNameSyntax = attribute.DescendantNodes().OfType<IdentifierNameSyntax>().FirstOrDefault();
            }

            if (identifierNameSyntax != null)
            {
                if (attribute.ArgumentList != null &&
                    !attribute.ArgumentList.OpenParenToken.IsMissing &&
                    !identifierNameSyntax.Identifier.IsMissing)
                {
                    CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context,
                        attribute.ArgumentList.OpenParenToken, identifierNameSyntax.Identifier);
                }
            }
        }

        private void HandleElementAccessExpression(SyntaxNodeAnalysisContext context)
        {
            var elementAccess = (ElementAccessExpressionSyntax)context.Node;

            if (elementAccess.Expression == null ||
                elementAccess.ArgumentList.IsMissing ||
                elementAccess.ArgumentList.OpenBracketToken.IsMissing)
            {
                return;
            }

            CheckIfLocationOfExpressionAndOpenTokenAreTheSame(context,
                elementAccess.ArgumentList.OpenBracketToken, elementAccess.Expression);
        }

        private void HandleIndexerDeclaration(SyntaxNodeAnalysisContext obj)
        {
            var indexerDeclaration = (IndexerDeclarationSyntax)obj.Node;

            if (!indexerDeclaration.ThisKeyword.IsMissing &&
                indexerDeclaration.ParameterList != null &&
                !indexerDeclaration.ParameterList.IsMissing &&
                !indexerDeclaration.ParameterList.OpenBracketToken.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(obj, indexerDeclaration.ParameterList.OpenBracketToken, indexerDeclaration.ThisKeyword);
            }
        }

        private void HandleObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            var objectCreation = (ObjectCreationExpressionSyntax)context.Node;
            var identifier = this.GetIdentifier(objectCreation);

            if (!identifier.HasValue
                || identifier.Value.IsMissing)
            {
                return;
            }

            if (objectCreation.ArgumentList != null
                && !objectCreation.ArgumentList.OpenParenToken.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context, objectCreation.ArgumentList.OpenParenToken, identifier.Value);
            }
        }

        private SyntaxToken? GetIdentifier(ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
        {
            switch (objectCreationExpressionSyntax.Type.Kind())
            {
            case SyntaxKind.QualifiedName:
                var qualifiedNameSyntax = (QualifiedNameSyntax)objectCreationExpressionSyntax.Type;
                var identifierNameSyntax = qualifiedNameSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().LastOrDefault();
                return identifierNameSyntax?.Identifier;

            case SyntaxKind.IdentifierName:
                return ((IdentifierNameSyntax)objectCreationExpressionSyntax.Type).Identifier;

            case SyntaxKind.GenericName:
                return ((GenericNameSyntax)objectCreationExpressionSyntax.Type).Identifier;

            default:
                return null;
            }
        }

        private void HandleInvocationExpression(SyntaxNodeAnalysisContext context)
        {
            var invocationExpression = (InvocationExpressionSyntax)context.Node;

            var identifierNameSyntax = invocationExpression.Expression as IdentifierNameSyntax ??
                                                        invocationExpression.Expression.DescendantNodes().OfType<IdentifierNameSyntax>().LastOrDefault();

            if (identifierNameSyntax != null)
            {
                if (invocationExpression.ArgumentList != null &&
                    !invocationExpression.ArgumentList.OpenParenToken.IsMissing &&
                    !identifierNameSyntax.Identifier.IsMissing)
                {
                    CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context,
                        invocationExpression.ArgumentList.OpenParenToken, identifierNameSyntax.Identifier);
                }
            }
        }

        private void HandleConstructorDeclaration(SyntaxNodeAnalysisContext context)
        {
            var constructotDeclarationSyntax = (ConstructorDeclarationSyntax)context.Node;
            if (constructotDeclarationSyntax.ParameterList != null
                && !constructotDeclarationSyntax.ParameterList.OpenParenToken.IsMissing
                && !constructotDeclarationSyntax.Identifier.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context, constructotDeclarationSyntax.ParameterList.OpenParenToken, constructotDeclarationSyntax.Identifier);
            }
        }

        private void HandleMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            var methodDeclaration = (MethodDeclarationSyntax)context.Node;
            if (methodDeclaration.ParameterList != null
                && !methodDeclaration.ParameterList.OpenParenToken.IsMissing
                && !methodDeclaration.Identifier.IsMissing)
            {
                CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(context, methodDeclaration.ParameterList.OpenParenToken, methodDeclaration.Identifier);
            }
        }

        private static void CheckIfLocationOfIdentifierNameAndOpenTokenAreTheSame(SyntaxNodeAnalysisContext context, SyntaxToken openToken, SyntaxToken identifierToken)
        {
            var identifierLine = identifierToken.GetLineSpan();
            var openParenLine = openToken.GetLineSpan();
            if (identifierLine.IsValid &&
                openParenLine.IsValid &&
                openParenLine.StartLinePosition.Line != identifierLine.StartLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, openToken.GetLocation()));
            }
        }

        private static void CheckIfLocationOfExpressionAndOpenTokenAreTheSame(SyntaxNodeAnalysisContext context, SyntaxToken openToken, ExpressionSyntax expression)
        {
            var identifierLine = expression.GetLineSpan();
            var openParenLine = openToken.GetLineSpan();
            if (identifierLine.IsValid &&
                openParenLine.IsValid &&
                openParenLine.StartLinePosition.Line != identifierLine.StartLinePosition.Line)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, openToken.GetLocation()));
            }
        }
    }
}
