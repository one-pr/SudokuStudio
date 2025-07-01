namespace Sudoku.Generating.Filtering.Annotations;

public partial class StepAnnotationAttribute
{
	/// <summary>
	/// Represents the attribute based on.
	/// </summary>
	public enum BasedOn
	{
		/// <summary>
		/// Indicates the item based on is type.
		/// </summary>
		Type,

		/// <summary>
		/// Indicates the item based on is member.
		/// </summary>
		Member
	}
}
