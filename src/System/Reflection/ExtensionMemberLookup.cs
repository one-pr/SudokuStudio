namespace System.Reflection;

/// <summary>
/// Provides a way to find for extension members that are introduced in C# 14.
/// </summary>
public static partial class ExtensionMemberLookup
{
	/// <summary>
	/// Indicates the name of method of extension marker.
	/// </summary>
	public const string ExtensionMarkerMethodName = "<Extension>$";

	/// <summary>
	/// Indicates the prefix of extension grouper type name.
	/// </summary>
	public const string ExtensionGrouperTypeNamePrefix = "<G>$";

	/// <summary>
	/// Indicates the prefix of extension marker type name.
	/// </summary>
	public const string ExtensionMarkerTypeNamePrefix = "<M>$";

	/// <summary>
	/// By design, we should find members and types marked <see langword="public"/> and <see langword="static"/>,
	/// and it shouldn't be a member overwritten from its base type or ancestor types.
	/// </summary>
	internal const BindingFlags DefaultBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

	/// <summary>
	/// Defines binding flags that will match skeleton members defined in extesion grouper types.
	/// </summary>
	internal const BindingFlags ExtensionGrouperSkeletonMembersBindingFlags = BindingFlags.Public | BindingFlags.NonPublic
		| BindingFlags.Static | BindingFlags.Instance
		| BindingFlags.DeclaredOnly;


	/// <summary>
	/// Extends <see cref="Type"/> instances.
	/// </summary>
	extension(Type type)
	{
		/// <summary>
		/// Try to enumerates for all possible extension containers that represents extension members for the current type.
		/// </summary>
		/// <param name="includingFileLocalTypes">Indicates whether <see langword="file"/> types are included or not.</param>
		/// <param name="assemblies">
		/// The assemblies that you want to find. If specify <see langword="null"/> or an empty array,
		/// this method will defaultly find for extension members in the current-executing assembly.
		/// </param>
		/// <returns>
		/// A sequence of tuple of <see cref="Type"/> instances: extension grouper, extension marker
		/// and the containing static class type.
		/// </returns>
		public IEnumerable<ExtensionContainerMetadata> GetExtensionContainers(bool includingFileLocalTypes, params Assembly[]? assemblies)
		{
			// Iterate on each assembly.
			foreach (var assembly in assemblies is { Length: not 0 } ? assemblies : [Assembly.GetExecutingAssembly()])
			{
				// Check for all possible data types, no matter whether it satisfied or not.
				foreach (var staticClassType in assembly.GetTypes())
				{
					// Step 1: Check for static class.

					// This type must be 'static'.
					if (!staticClassType.IsStatic)
					{
						continue;
					}

					// This type must be marked as [Extension].
					if (!staticClassType.MightBeExtension)
					{
						continue;
					}

					// Skip for file-local types if 'includingFileLocalTypes' is false.
					if (!includingFileLocalTypes && FileLocalTypeNamePattern.IsMatch(staticClassType.Name))
					{
						continue;
					}

					// Step 2: Check extension grouping type, whose name must be started
					// with <see cref="ExtensionGrouperTypeNamePrefix"/>.
					// This type is a static class.

					// Iterate on all possible types defined in it.
					foreach (var extensionGrouperType in staticClassType.GetMembers(DefaultBindingFlags).OfType<Type>())
					{
						// The name of this type must be started with <see cref="ExtensionGrouperTypeNamePrefix"/>.
						if (!extensionGrouperType.Name.StartsWith(ExtensionGrouperTypeNamePrefix))
						{
							continue;
						}

						// This type must be 'sealed'.
						if (!extensionGrouperType.IsSealed)
						{
							continue;
						}

						// This type must be marked as [Extension].
						if (!extensionGrouperType.MightBeExtension)
						{
							continue;
						}

						// This type must be marked as [SpecialName].
						// However, we should check for this type by using <see cref="Type.Attributes"/>,
						// rather than using <see cref="MemberInfo.IsDefined(Type, bool)"/>.
						if (!extensionGrouperType.IsSpecialName)
						{
							continue;
						}

						// This type must be non-'static'.
						if (extensionGrouperType.IsStatic)
						{
							continue;
						}

						// Step 3: Check extension marker type defined in extension grouping type.

						// This type is also a static class.
						var correspondingExtensionMarkerType = default(Type);
						foreach (var extensionMarkerType in extensionGrouperType.GetMembers(DefaultBindingFlags).OfType<Type>())
						{
							// The name of this type must be started with '<see cref="ExtensionMarkerTypeNamePrefix"/>'.
							if (!extensionMarkerType.Name.StartsWith(ExtensionMarkerTypeNamePrefix))
							{
								continue;
							}

							// This type must be marked [SpecialName].
							if (!extensionGrouperType.IsSpecialName)
							{
								continue;
							}

							// This type must be 'static'.
							if (!extensionMarkerType.IsStatic)
							{
								continue;
							}

							// This type must contain one and only one member - a 'static' method,
							// with name '<see cref="ExtensionMarkerMethodName"/>'.
							var possibleMethodsInfo = extensionMarkerType.GetMembers(DefaultBindingFlags)
								.OfType<MethodInfo>()
								.ToArray();
							if (possibleMethodsInfo is not [
								{
									IsGenericMethod: false,
									Name: ExtensionMarkerMethodName,
									ReturnType: var returnType,
									IsSpecialName: true,
									IsCompilerGenerated: true,
									Parameters: [{ ParameterType: var parameterType }],
								}]
								|| parameterType != type
								|| returnType != typeof(void))
							{
								continue;
							}

							correspondingExtensionMarkerType = extensionMarkerType;
							break;
						}

						// An extension grouping type must include a satisfied extension marker type.
						if (correspondingExtensionMarkerType is null)
						{
							continue;
						}

						// Okay, now we have got a satisfied extension grouping type.
						yield return new(extensionGrouperType, correspondingExtensionMarkerType, staticClassType);
					}
				}
			}
		}


		/// <summary>
		/// Try to find for all possible members (properties, methods and operators) of the specified type,
		/// representing as a <see cref="Type"/> instance, defined as extension members inside the specified assemblies.
		/// </summary>
		/// <param name="memberTypes">The types of members you want to get.</param>
		/// <param name="includingFileLocalTypes">Indicates whether <see langword="file"/> types are included or not.</param>
		/// <param name="assemblies">
		/// The assemblies that you want to find. If specify <see langword="null"/> or an empty array,
		/// this method will defaultly find for extension members in the current-executing assembly.
		/// </param>
		/// <returns>All possible extension members found.</returns>
		public IEnumerable<MemberInfo> FindExtensionMembers(ExtensionMemberTypes memberTypes, bool includingFileLocalTypes, params Assembly[]? assemblies)
		{
			foreach (var metadata in type.GetExtensionContainers(includingFileLocalTypes, assemblies))
			{
				foreach (var (callable, skeleton) in metadata.EnumerateExtensionMembers())
				{
					switch (skeleton)
					{
						case PropertyInfo { IsStatic: false, IndexParameters: [] }
							when memberTypes.HasFlag(ExtensionMemberTypes.Properties):
						case PropertyInfo { IsStatic: true, IndexParameters: [] }
							when memberTypes.HasFlag(ExtensionMemberTypes.StaticProperties):
						case PropertyInfo { IndexParameters: not [] }
							when memberTypes.HasFlag(ExtensionMemberTypes.Indexers):
						case MethodInfo { IsStatic: false, IsSpecialName: false }
							when memberTypes.HasFlag(ExtensionMemberTypes.Methods):
						case MethodInfo { IsStatic: true, IsSpecialName: false }
							when memberTypes.HasFlag(ExtensionMemberTypes.StaticMethods):
						case MethodInfo { IsStatic: false, IsSpecialName: true, Name: var methodName1 }
							when memberTypes.HasFlag(ExtensionMemberTypes.CompoundAssignmentOperators)
							&& methodName1.StartsWith("op_") && methodName1.EndsWith("Assignment"):
						case MethodInfo { IsStatic: true, IsSpecialName: true, Name: var methodName2 }
							when memberTypes.HasFlag(ExtensionMemberTypes.Operators)
							&& methodName2.StartsWith("op_"):
						{
							yield return callable;
							break;
						}
					}
				}
			}
		}
	}


	[GeneratedRegex("""\<.+\>F[\dA-F]{64}__.+""", RegexOptions.Compiled | RegexOptions.Singleline)]
	private static partial Regex FileLocalTypeNamePattern { get; }
}
