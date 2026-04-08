namespace System.Reflection;

/// <summary>
/// Provides with extension methods on <see cref="Type"/>.
/// </summary>
/// <seealso cref="Type"/>
public static class TypeExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Type @this)
	{
		/// <summary>
		/// Determines whether the type has a parameterless constructor.
		/// </summary>
		public bool HasParameterlessConstructor
			=> @this.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) is not null;

		/// <summary>
		/// Indicates whether the type is <see langword="static"/> or not.
		/// </summary>
		public bool IsStatic
		{
			get
			{
				// In metadata (IL layer), a <see langword="static class"/> will be emitted
				// as an <see langword="abstracted sealed class"/> with no constructors.
				const BindingFlags instanceConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				return @this.IsAbstract && @this.IsSealed && @this.GetConstructors(instanceConstructorFlags).Length == 0;
			}
		}


		/// <summary>
		/// Determines whether the current type can be assigned to a variable of the specified
		/// <paramref name="targetType"/>, although it is with generic parameters.
		/// </summary>
		/// <param name="targetType">The type to compare with the current type.</param>
		/// <returns>Returns <see langword="true"/> if the target type is matched, without generic constraints.</returns>
		/// <seealso href="https://stackoverflow.com/questions/74616/how-to-detect-if-type-is-another-generic-type/1075059#1075059">
		/// Question: How to detect if type is another generic type
		/// </seealso>
		public bool IsGenericAssignableTo([NotNullWhen(true)] Type? targetType)
		{
			foreach (var it in @this.GetInterfaces())
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == targetType)
				{
					return true;
				}
			}

			if (@this.IsGenericType && @this.GetGenericTypeDefinition() == targetType)
			{
				return true;
			}

			if (@this.BaseType is not { } baseType)
			{
				return false;
			}

			return baseType.IsGenericAssignableTo(targetType);
		}

		/// <summary>
		/// Determine whether two <see cref="Type"/> instances are exactly same, with type arguments also checked.
		/// </summary>
		/// <param name="left">The first instance to be checked.</param>
		/// <param name="right">The second instance to be checked.</param>
		/// <param name="ignoreByRef">Indicates whether this method ignores <see langword="ref"/> modifier or not.</param>
		/// <param name="skipUnboundTypeParameters">Indicates whether this method skips unbound type parameters or not.</param>
		/// <returns>A <see cref="bool"/> result indicating that.</returns>
		public static bool IsExactlySame(Type left, Type right, bool ignoreByRef, bool skipUnboundTypeParameters)
		{
			if (left == right)
			{
				return true;
			}

			if (!ignoreByRef && left.IsByRef ^ right.IsByRef)
			{
				return false;
			}

			if (skipUnboundTypeParameters && left.IsGenericParameter && right.IsGenericParameter)
			{
				return true;
			}
			if (!skipUnboundTypeParameters && left.IsGenericParameter)
			{
				return false;
			}

			if (left.IsGenericType != right.IsGenericType)
			{
				return false;
			}
			if (!left.IsGenericType)
			{
				return false;
			}
			if (left.GetGenericTypeDefinition() != right.GetGenericTypeDefinition())
			{
				return false;
			}

			var typeargs1 = left.GetGenericArguments();
			var typeargs2 = right.GetGenericArguments();
			if (typeargs1.Length != typeargs2.Length)
			{
				return false;
			}

			for (var i = 0; i < typeargs1.Length; i++)
			{
				if (!IsExactlySame(typeargs1[i], typeargs2[i], ignoreByRef, skipUnboundTypeParameters))
				{
					return false;
				}
			}
			return true;
		}
	}
}
