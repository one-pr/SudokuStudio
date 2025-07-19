namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents a keyword value converter for <see cref="Technique"/> instance.
/// </summary>
public sealed class TechniqueKeywordValueConverter : KeywordValueConverter
{
	/// <inheritdoc/>
	public override KeywordType TargetType => KeywordType.String;

	/// <inheritdoc/>
	public override Type BaseType => typeof(Technique);


	/// <inheritdoc/>
	public override bool TryConvert(object? value, Step step, [NotNullWhen(true)] out dynamic? result)
	{
		if (value is not Technique field)
		{
			result = null;
			return false;
		}

		result = field.GetName(step.Options.CurrentCulture);
		return true;
	}

	/// <inheritdoc/>
	public override bool TryConvertBack(object? value, Step step, [NotNullWhen(true)] out dynamic? result)
	{
		if (value is not string str)
		{
			goto ReturnFalse;
		}

		foreach (var field in Technique.Values)
		{
			if (field.GetName(step.Options.CurrentCulture) == str)
			{
				result = field;
				return true;
			}
		}

	ReturnFalse:
		result = null;
		return false;
	}
}
