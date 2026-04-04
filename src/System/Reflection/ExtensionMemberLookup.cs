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
		/// Try to find for all possible members (properties, methods and operators) of the specified type,
		/// representing as a <see cref="Type"/> instance, defined as extension members inside the specified assemblies.
		/// </summary>
		/// <param name="assemblies">
		/// The assemblies that you want to find. If specify <see langword="null"/> or an empty array,
		/// this method will defaultly find for extension members in the current-executing assembly.
		/// </param>
		/// <returns>All possible extension members found.</returns>
		public IEnumerable<MemberInfo> FindExtensionMembers(Assembly[]? assemblies)
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
					#region Comments on extension member design
					// We should check for the following design type hierarchy:
					// <code><![CDATA[
					// [Extension]
					// static class TypeExtensions
					// {
					//     // An "Extension Grouping Type".
					//     [SpecialName, Extension]
					//     public sealed class <G>$ComplexHash
					//     {
					//         // Provides signatures on declaration of an extension block,
					//         // Including type you want to extend (extension type), and its instance parameter name.
					//         // This type is named an "Extesion Marker Type".
					//         [SpecialName]
					//         public static class <M>$ComplexHash
					//         {
					//             [SpecialName, CompilerGenerated]
					//             public static void <Extension>$(ExtensionType extensionInstanceName);
					//         }
					// 
					//         // All types of members will be fully-copied their signatures into this extension grouping type.
					//         // However, such members cannot be callable because they will automatically throw exceptions.
					//         // The only reason they are here is for representing runtime-recognizable signatures.
					//         // In addition, if such extension members has been marked some attributes, they will also be copied here.
					//         // Examples of signatures:
					//         //   [ExtensionMarker("<M>$ComplexHash")]
					//         //   public int Property { [ExtensionMarker("<M>$ComplexHash")] get; }
					//         //   [ExtensionMarker("<M>$ComplexHash")]
					//         //   public static void Method();
					//         //   [ExtensionMarker("<M>$ComplexHash")]
					//         //   public static bool op_True(int value);
					//     }
					// 
					//     // All callable members will be defined here, but they are lowered as *static methods*.
					//     // Examples of signatures:
					//     //   public static int get_Property(int @this);
					//     //   public static void Method();
					//     //   public static bool op_True(int value);
					// }
					// ]]></code>
					#endregion

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

						// Step 4: Find for all possible signatures of members defined in this type.
						// Such types cannot be callable but we should use its names to make a final lookup.
						var targetMemberNames = new List<(string Name, bool IsProperty)>();
						foreach (var member in extensionGrouperType.GetMembers())
						{
							// Only extension properties, methods, operators and indexers (introduced in C# 15)
							// will be supported.
							if (member is { Name: var memberName } and (PropertyInfo or MethodInfo))
							{
								targetMemberNames.Add((memberName, member is PropertyInfo));
							}
						}

						// Step 5: Find for matched members in the static class by names collected in the previous step.
						foreach (var (memberName, isProperty) in targetMemberNames)
						{
							// There's no possible members exists here due to mismatched of name.
							// Although, the name may not be same (which is more intuitive, especially for properties),
							// they are, in fact, same in reflection - though it is represented as a static method now.
							//
							// All possible member types will be lowered like:
							// <list type="bullet">
							// <item>Property getter => <c>get_PropertyName</c></item>
							// <item>Property setter => <c>set_PropertyName</c></item>
							// <item>Indexer getter => <c>get_Item</c> (Depends on name defined in [IndexerName])</item>
							// <item>Indexer setter => <c>set_Item</c> (Depends on name defined in [IndexerName])</item>
							// <item>Method => <c>MethodName</c> (Just copy)</item>
							// <item>Operator => <c>op_OperatorName</c> (Just copy)</item>
							// </list>
							if (staticClassType.GetMember(memberName) is not { Length: not 0 } members)
							{
								continue;
							}

							// Iterate on each matched member of same name.
							foreach (var member in members)
							{
								// This type must be a static method.
								if (member is not MethodInfo { IsStatic: true })
								{
									continue;
								}

								// Okay, now the member is found.
								yield return member;
							}
						}
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
	}
}
