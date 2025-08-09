namespace Sudoku.CommandLine.Options;

/// <summary>
/// Represents a comparing method option.
/// </summary>
internal sealed class ComparingMethodOption : Option<GridComparison>, IOption<GridComparison>
{
	/// <summary>
	/// Initializes a <see cref="ComparingMethodOption"/> instance.
	/// </summary>
	public ComparingMethodOption() : base(["-m", "--method"], "Indicates the method to be compared")
	{
		Arity = ArgumentArity.ExactlyOne;
		IsRequired = false;
		SetDefaultValue(GridComparison.Default);
	}


	/// <inheritdoc/>
	static GridComparison IMySymbol<GridComparison>.ParseArgument(ArgumentResult result)
		=> throw new NotImplementedException();
}
