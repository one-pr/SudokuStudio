namespace Sudoku.CommandLine;

/// <summary>
/// Provides with extension methods on <see cref="CommandBase"/>.
/// </summary>
/// <seealso cref="CommandBase"/>
public static class CommandBaseExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension<TCommand>(TCommand @this) where TCommand : CommandBase
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out SymbolList<Option> options, out SymbolList<Argument> arguments)
		{
			options = @this.OptionsCore;
			arguments = @this.ArgumentsCore;
		}

		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(
			out SymbolList<Option> options,
			out SymbolList<Argument> arguments,
			out SymbolList<Option> globalOptions
		)
			=> ((options, arguments), globalOptions) = (@this, @this.Parent?.GlobalOptionsCore ?? []);
	}
}
