namespace System.Reflection;

/// <summary>
/// Represents a list of extension container type information.
/// </summary>
/// <param name="ExtensionGrouper">Indicates the type of extension grouper.</param>
/// <param name="ExtensionMarker">Indicates the type of extension marker.</param>
/// <param name="ContainingStaticClass">Indicates the containing static class type.</param>
/// <remarks>
/// For extension members, C# design team defines the following type hierarchy:
/// <code><![CDATA[
/// [Extension]
/// static class TypeExtensions
/// {
///     // An "Extension Grouping Type".
///     [SpecialName, Extension]
///     public sealed class <G>$ComplexHash
///     {
///         // Provides signatures on declaration of an extension block,
///         // Including type you want to extend (extension type), and its instance parameter name.
///         // This type is named an "Extesion Marker Type".
///         [SpecialName]
///         public static class <M>$ComplexHash
///         {
///             [SpecialName, CompilerGenerated]
///             public static void <Extension>$(ExtensionType extensionInstanceName);
///         }
/// 
///         // All types of members will be fully-copied their signatures into this extension grouping type.
///         // However, such members cannot be callable because they will automatically throw exceptions.
///         // The only reason they are here is for representing runtime-recognizable signatures.
///         // In addition, if such extension members has been marked some attributes,
///         // they will also be copied here.
///         // Examples of signatures:
///         //   [ExtensionMarker("<M>$ComplexHash")]
///         //   public int Property { [ExtensionMarker("<M>$ComplexHash")] get; }
///         //   [ExtensionMarker("<M>$ComplexHash")]
///         //   public static void Method();
///         //   [ExtensionMarker("<M>$ComplexHash")]
///         //   public static bool op_True(int value);
///     }
/// 
///     // All callable members will be defined here, but they are lowered as *static methods*.
///     // Examples of signatures:
///     //   public static int get_Property(int @this);
///     //   public static void Method();
///     //   public static bool op_True(int value);
/// }
/// ]]></code>
/// To visit members for extension members in reflection (metadata), we can lookup such extension grouper and marker types.
/// </remarks>
public sealed record ExtensionContainerMetadata(Type ExtensionGrouper, Type ExtensionMarker, Type ContainingStaticClass)
{
	/// <summary>
	/// Try to enumerate all members defined in this extension container.
	/// </summary>
	/// <returns>A sequence of extension members.</returns>
	public IEnumerable<MemberInfo> EnumerateExtensionMembers()
	{
		// Find for all possible signatures of members defined in this type.
		// Such types cannot be callable but we should use its names to make a final lookup.
		var targetMemberNames = new List<(string Name, bool IsProperty)>();
		foreach (var member in ExtensionGrouper.GetMembers())
		{
			// Only extension properties, methods, operators and indexers (introduced in C# 15)
			// will be supported.
			if (member is { Name: var memberName } and (PropertyInfo or MethodInfo))
			{
				targetMemberNames.Add((memberName, member is PropertyInfo));
			}
		}

		// Then find for matched members in the static class by names collected in the previous step.
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
			if (ContainingStaticClass.GetMember(memberName) is not { Length: not 0 } members)
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
