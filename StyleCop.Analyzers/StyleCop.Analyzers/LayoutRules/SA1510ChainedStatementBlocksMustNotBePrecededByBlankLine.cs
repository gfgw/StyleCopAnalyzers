﻿namespace StyleCop.Analyzers.LayoutRules
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.Helpers;

    /// <summary>
    /// Chained C# statements are separated by a blank line.
    /// </summary>
    /// <remarks>
    /// <para>To improve the readability of the code, StyleCop requires blank lines in certain situations, and prohibits
    /// blank lines in other situations. This results in a consistent visual pattern across the code, which can improve
    /// recognition and readability of unfamiliar code.</para>
    ///
    /// <para>Some types of C# statements can only be used when chained to the bottom of another statement. Examples
    /// include catch and finally statements, which must always be chained to the bottom of a try-statement. Another
    /// example is an else-statement, which must always be chained to the bottom of an if-statement, or to another
    /// else-statement. These types of chained statements must not be separated by a blank line. For example:</para>
    ///
    /// <code language="csharp">
    /// try
    /// {
    ///     this.SomeMethod();
    /// }
    ///
    /// catch (Exception ex)
    /// {
    ///     Console.WriteLine(ex.ToString());
    /// }
    /// </code>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1510ChainedStatementBlocksMustNotBePrecededByBlankLine : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the
        /// <see cref="SA1510ChainedStatementBlocksMustNotBePrecededByBlankLine"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "SA1510";
        private const string Title = "Chained statement blocks must not be preceded by blank line";
        private const string MessageFormat = "'{0}' statement must not be preceded by a blank line";
        private const string Category = "StyleCop.CSharp.LayoutRules";
        private const string Description = "Chained C# statements are separated by a blank line.";
        private const string HelpLink = "http://www.stylecop.com/docs/SA1510.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

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
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleElseStatement, SyntaxKind.ElseClause);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleCatchClause, SyntaxKind.CatchClause);
            context.RegisterSyntaxNodeActionHonorExclusions(this.HandleFinallyClause, SyntaxKind.FinallyClause);
        }

        private void HandleElseStatement(SyntaxNodeAnalysisContext context)
        {
            var elseClause = (ElseClauseSyntax)context.Node;

            if (!TriviaHelper.HasLeadingBlankLines(elseClause.ElseKeyword))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, elseClause.ElseKeyword.GetLocation(), elseClause.ElseKeyword.ToString()));
        }

        private void HandleCatchClause(SyntaxNodeAnalysisContext context)
        {
            var catchClause = (CatchClauseSyntax)context.Node;

            if (!TriviaHelper.HasLeadingBlankLines(catchClause.CatchKeyword))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, catchClause.CatchKeyword.GetLocation(), catchClause.CatchKeyword.ToString()));
        }

        private void HandleFinallyClause(SyntaxNodeAnalysisContext context)
        {
            var finallyClause = (FinallyClauseSyntax)context.Node;

            if (!TriviaHelper.HasLeadingBlankLines(finallyClause.FinallyKeyword))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, finallyClause.FinallyKeyword.GetLocation(), finallyClause.FinallyKeyword.ToString()));
        }
    }
}
