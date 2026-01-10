namespace System;

/// <summary>
/// Defines a mechanism for computing the logical relation between two instances of type <typeparamref name="TSelf"/>.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
public interface ILogicalOperators<TSelf> : IBitwiseOperators<TSelf, TSelf, TSelf> where TSelf : ILogicalOperators<TSelf>?
{
	/// <summary>
	/// Determines whether the specified instance should be considered <see langword="true"/> in a boolean context.
	/// </summary>
	/// <param name="value">The instance to evaluate.</param>
	/// <returns>
	/// <see langword="true"/> if the instance should be considered <see langword="true"/>; otherwise, <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// This operator is invoked by the C# compiler when a boolean evaluation is required
	/// (for example in <see langword="if"/> statements or conditional expressions).
	/// When used together with overloaded <c><see langword="operator"/> &amp;</c> and <c><see langword="operator"/> |</c>,
	/// the compiler also consults <c><see langword="operator true"/></c> and <c><see langword="operator false"/></c>
	/// to implement the conditional behavior of <c><see langword="operator"/> &amp;&amp;</c> and <c><see langword="operator"/> ||</c>.
	/// If the type can be <see langword="null"/>, document how <see langword="null"/> is handled
	/// (for example: treated as <see langword="false"/>, causes a <see cref="ArgumentNullException"/>, etc.).
	/// </remarks>
	static abstract bool operator true(TSelf value);

	/// <summary>
	/// Determines whether the specified instance should be considered <see langword="false"/> in a boolean context.
	/// </summary>
	/// <param name="value">The instance to evaluate.</param>
	/// <returns>
	/// <see langword="true"/> if the instance should be considered <see langword="false"/>; otherwise, <see langword="false"/>.
	/// </returns>
	/// <remarks>
	/// Should be defined in pair with <c><see langword="operator true"/></c>.
	/// The C# compiler uses both operators to decide control flow for conditional expressions and
	/// to cooperate with user-defined <c><see langword="operator"/> &amp;</c> / <c><see langword="operator"/> |</c>
	/// overloads when implementing <c><see langword="operator"/> &amp;&amp;</c> / <c><see langword="operator"/> ||</c>.
	/// Describe any consistency requirements with <c><see langword="operator true"/></c> and how <see langword="null"/> is handled.
	/// </remarks>
	static virtual bool operator false(TSelf value) => !(value ? true : false);

	/// <summary>
	/// Negates the current instance, and makes the result to be negated one.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	static abstract bool operator !(TSelf value);
}
