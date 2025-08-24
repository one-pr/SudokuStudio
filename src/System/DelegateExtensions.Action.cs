namespace System;

public partial class DelegateExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Action"/>.
	/// </summary>
	extension(Action)
	{
		/// <summary>
		/// Represents pointer to <see cref="DoNothingMethod"/>.
		/// </summary>
		/// <seealso cref="DoNothingMethod"/>
		public static unsafe delegate*<void> DoNothingMethodPtr => &DoNothingMethod;

		/// <summary>
		/// Creates an <see cref="Action"/> instance that do nothing.
		/// </summary>
		public static Action DoNothing => DoNothingMethod;


		/// <summary>
		/// Represents a method that do nothing.
		/// </summary>
		public static void DoNothingMethod()
		{
		}
	}
}
