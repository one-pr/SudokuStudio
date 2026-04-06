namespace System.Reflection;

/// <summary>
/// Provides a way to find for extension members that are introduced in C# 14.
/// </summary>
public static class ExtensionMemberLookup
{
	/// <summary>
	/// Extends <see cref="Type"/> instances.
	/// </summary>
	extension(Type type)
	{
		/// <summary>
		/// Try to enumerates for all possible extension containers that represents extension members for the current type.
		/// </summary>
		/// <param name="assemblies">
		/// The assemblies that you want to find. If specify <see langword="null"/> or an empty array,
		/// this method will defaultly find for extension members in the current-executing assembly.
		/// </param>
		/// <returns>
		/// A sequence of tuple of <see cref="Type"/> instances: extension grouper, extension marker
		/// and the containing static class type.
		/// </returns>
		public IEnumerable<ExtensionContainerMetadata> GetExtensionContainers(Assembly[]? assemblies)
		{
			// By design, we should find members and types marked <see langword="public"/> and <see langword="static"/>,
			// and it shouldn't be a member overwritten from its base type or ancestor types.
			const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

			// Iterate on each assembly.
			foreach (var assembly in assemblies is { Length: not 0 } ? assemblies : [Assembly.GetExecutingAssembly()])
			{
				// Check for all possible data types, no matter whether it satisfied or not.
				foreach (var staticClassType in assembly.GetTypes())
				{
					// Step 1: Check for static class.

					// This type must be 'static'.
					if (!isStaticClass(staticClassType))
					{
						continue;
					}

					// This type must be marked as [Extension].
					if (!staticClassType.IsDefined(typeof(ExtensionAttribute), false))
					{
						continue;
					}

					// Step 2: Check extension grouping type, whose name must be started with '<G>$'.
					// This type is a static class.

					// Iterate on all possible types defined in it.
					foreach (var extensionGrouperType in staticClassType.GetMembers(defaultBindingFlags).OfType<Type>())
					{
						// The name of this type must be started with '<G>$'.
						if (!extensionGrouperType.Name.StartsWith("<G>$"))
						{
							continue;
						}

						// This type must be 'sealed'.
						if (!extensionGrouperType.IsSealed)
						{
							continue;
						}

						// This type must be marked as [Extension].
						if (!extensionGrouperType.IsDefined(typeof(ExtensionAttribute), false))
						{
							continue;
						}

						// This type must be marked as [SpecialName].
						// However, we should check for this type by using <see cref="Type.Attributes"/>,
						// rather than using <see cref="MemberInfo.IsDefined(Type, bool)"/>.
						if (!extensionGrouperType.Attributes.HasFlag(TypeAttributes.SpecialName))
						{
							continue;
						}

						// This type must be non-'static'.
						if (isStaticClass(extensionGrouperType))
						{
							continue;
						}

						// Step 3: Check extension marker type defined in extension grouping type.

						// This type is also a static class.
						var correspondingExtensionMarkerType = default(Type);
						foreach (var extensionMarkerType in extensionGrouperType.GetMembers(defaultBindingFlags).OfType<Type>())
						{
							// The name of this type must be started with <M>$.
							if (!extensionMarkerType.Name.StartsWith("<M>$"))
							{
								continue;
							}

							// This type must be marked [SpecialName].
							if (!extensionGrouperType.Attributes.HasFlag(TypeAttributes.SpecialName))
							{
								continue;
							}

							// This type must be 'static'.
							if (!isStaticClass(extensionMarkerType))
							{
								continue;
							}

							// This type must contain one and only one member - a 'static' method, with name '<Extension>$'.
							var possibleMethodInfos = extensionMarkerType.GetMembers(defaultBindingFlags)
								.OfType<MethodInfo>()
								.ToArray();
							if (possibleMethodInfos is not [{ Name: "<Extension>$", ReturnType: var returnType } extensionStaticMethodMember]
								|| extensionStaticMethodMember.GetParameters() is not [{ ParameterType: var parameterType, Name: _ }]
								|| parameterType != type
								|| returnType != typeof(void)
								|| !extensionStaticMethodMember.Attributes.HasFlag(MethodAttributes.SpecialName)
								|| !extensionStaticMethodMember.IsDefined(typeof(CompilerGeneratedAttribute), false))
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


			static bool isStaticClass(Type type)
			{
				// In metadata (IL layer), a <see langword="static class"/> will be emitted
				// as an <see langword="abstracted sealed class"/> with no constructors.
				const BindingFlags instanceConstructorFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				return type.IsAbstract && type.IsSealed && type.GetConstructors(instanceConstructorFlags).Length == 0;
			}
		}


		/// <summary>
		/// Try to find for all possible members (properties, methods and operators) of the specified type,
		/// representing as a <see cref="Type"/> instance, defined as extension members inside the specified assemblies.
		/// </summary>
		/// <param name="memberTypes">The types of members you want to get.</param>
		/// <param name="assemblies">
		/// The assemblies that you want to find. If specify <see langword="null"/> or an empty array,
		/// this method will defaultly find for extension members in the current-executing assembly.
		/// </param>
		/// <returns>All possible extension members found.</returns>
		public IEnumerable<MemberInfo> FindExtensionMembers(ExtensionMemberTypes memberTypes, Assembly[]? assemblies)
		{
			// TODO: Unfinished - here we should check for metadata member (defined in extension grouper type, not in static class).
			foreach (var metadata in type.GetExtensionContainers(assemblies))
			{
				foreach (var member in metadata.EnumerateExtensionMembers())
				{
					switch (member, memberTypes)
					{
						case (PropertyInfo { GetMethod: var getter, SetMethod: var setter } p, _)
						when
							// It's an instance property
							memberTypes.HasFlag(ExtensionMemberTypes.Properties)
							&& !((getter?.IsStatic ?? false) || (setter?.IsStatic ?? false))
							&& p.GetIndexParameters().Length == 0
							// Or a static property
							|| memberTypes.HasFlag(ExtensionMemberTypes.StaticProperties)
							&& ((getter?.IsStatic ?? false) || (setter?.IsStatic ?? false))
							&& p.GetIndexParameters().Length == 0
							// Or an indexer (must be instance)
							|| memberTypes.HasFlag(ExtensionMemberTypes.Indexers)
							&& p.GetIndexParameters().Length != 0:
						{
							yield return member;
							break;
						}

						case (MethodInfo { IsStatic: var isStatic, Name: var name, Attributes: var methodAttributes }, _)
						when
							// It's an instance method
							memberTypes.HasFlag(ExtensionMemberTypes.Methods)
							&& !isStatic
							&& !methodAttributes.HasFlag(MethodAttributes.SpecialName)
							// Or a static method
							|| memberTypes.HasFlag(ExtensionMemberTypes.StaticMethods)
							&& isStatic
							&& !methodAttributes.HasFlag(MethodAttributes.SpecialName)
							// Or an instance operator (compound assignment operator)
							|| memberTypes.HasFlag(ExtensionMemberTypes.CompoundAssignmentOperators)
							&& !isStatic
							&& methodAttributes.HasFlag(MethodAttributes.SpecialName)
							&& name.StartsWith("op_") && name.EndsWith("Assignment")
							// Or a static operator
							|| memberTypes.HasFlag(ExtensionMemberTypes.Operators)
							&& isStatic
							&& methodAttributes.HasFlag(MethodAttributes.SpecialName)
							&& name.StartsWith("op_"):
						{
							yield return member;
							break;
						}
					}
				}
			}
		}
	}
}
