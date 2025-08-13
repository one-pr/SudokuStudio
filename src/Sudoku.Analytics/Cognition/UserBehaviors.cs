namespace Sudoku.Cognition;

/// <summary>
/// Represents constants of <see cref="UserBehavior"/>.
/// </summary>
/// <seealso cref="UserBehavior"/>
public static class UserBehaviors
{
	/// <summary>
	/// Indicates all possible behaviors.
	/// </summary>
	public const UserBehavior All = UserBehavior.DigitFirst | UserBehavior.BlockFirst;
}
