namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Provides helper methods for retrieving attributes with <see cref="KeywordAttribute"/> inheritance rules.
/// </summary>
/// <seealso cref="KeywordAttribute"/>
public static class Keyword
{
	/// <summary>
	/// Represents default binding flags on property.
	/// </summary>
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


	/// <summary>
	/// Retrieves the name of the keyword.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="keywordType">The keyword containing type.</param>
	/// <param name="cultureInfo">The culture information.</param>
	/// <returns>The name of the keyword.</returns>
	/// <exception cref="InvalidKeywordException">Throws when the target property specified is not a valid keyword.</exception>
	public static string GetName(string keyword, Type keywordType, CultureInfo? cultureInfo)
		=> GetKeywordAttribute(keyword, keywordType) is { NameResourceKey: var nameResourceKey }
			? SR.Get(nameResourceKey, cultureInfo)
			: throw new InvalidKeywordException();

	/// <summary>
	/// Retrieves the description of the keyword.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="keywordType">The keyword containing type.</param>
	/// <returns>The description of the keyword.</returns>
	/// <exception cref="InvalidKeywordException">Throws when the target property specified is not a valid keyword.</exception>
	public static string? GetDescription(string keyword, Type keywordType)
		=> GetKeywordAttribute(keyword, keywordType) is { DescriptionResourceKey: var descriptionResourceKey }
			? descriptionResourceKey is not null ? SR.Get(descriptionResourceKey) : null
			: throw new InvalidKeywordException();

	/// <summary>
	/// Retrieves possible keyword verbs.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="keywordType">The keyword containing type.</param>
	/// <returns>All keyword verbs allowed.</returns>
	/// <exception cref="InvalidKeywordException">Throws when the target property specified is not a valid keyword.</exception>
	public static KeywordVerbs GetKeywordVerbs(string keyword, Type keywordType)
		=> GetKeywordAttribute(keyword, keywordType) is { AllowedVerbs: var verbs }
		&& (
			GetKeywordType(keyword, keywordType) is var metaType and not KeywordType.Unknown
				? metaType.AllowedVerbs
				: KeywordVerbs.MergeFlags(KeywordVerbs.Values[1..])
		) is var allowedVerbs ? allowedVerbs & verbs : throw new InvalidKeywordException();

	/// <summary>
	/// Retrieves possible keywords that are marked <see cref="KeywordAttribute"/>.
	/// </summary>
	/// <param name="keywordType">The keyword type.</param>
	/// <returns>A list of keywords that are marked <see cref="KeywordAttribute"/>.</returns>
	/// <exception cref="ArgumentException">Throws when the target type is not derived from <see cref="Step"/>.</exception>
	/// <seealso cref="KeywordAttribute"/>
	public static ReadOnlySpan<string> GetKeywords(Type keywordType)
		=> keywordType.IsAssignableTo(typeof(Step))
			?
			from propertyInfo in PropertyCollector.GetAllDeclaredProperties(keywordType)
			where propertyInfo.IsDefined<KeywordAttribute>()
			select propertyInfo.Name
			: throw new ArgumentException(
				string.Format(SR.ExceptionMessage("ArgumentMustBeDerivedFromStepType"), nameof(keywordType)),
				nameof(keywordType)
			);

	/// <summary>
	/// <para>Try to get the meta type of the keyword.</para>
	/// <para>
	/// The value will be inferred from target property type of assembly;
	/// if the target type is not valid keyword type (enumeration and so on),
	/// the value will be fetched from property <see cref="KeywordAttribute.MetaType"/>.
	/// </para>
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="keywordType">The containing type.</param>
	/// <returns>The keyword type.</returns>
	/// <seealso cref="KeywordAttribute.MetaType"/>
	public static KeywordType GetKeywordType(string keyword, Type keywordType)
	{
		if (!IsKeyword(keyword, keywordType, out var propertyInfo))
		{
			throw new InvalidKeywordException();
		}

		var type = propertyInfo.PropertyType;
		if (type.IsAssignableTo(typeof(INumber<>).MakeGenericType(type)))
		{
			// Implements this interface: T implements INumber<T>
			return KeywordType.Number;
		}
		if (type == typeof(string))
		{
			// String type.
			return KeywordType.String;
		}

		// Fallback to read for keyword type configured in attribute.
		return propertyInfo.GetCustomAttribute<KeywordAttribute>()!.MetaType;
	}

	/// <summary>
	/// Gets <see cref="KeywordAttribute"/> configured.
	/// </summary>
	/// <param name="keyword">The keyword.</param>
	/// <param name="keywordType">The keyword type.</param>
	/// <returns>
	/// The <see cref="KeywordAttribute"/> instance configured; or <see langword="null"/> if it is not a keyword.
	/// </returns>
	public static KeywordAttribute? GetKeywordAttribute(string keyword, Type keywordType)
		=> IsKeyword(keyword, keywordType, out var propertyInfo) ? GetAttribute<KeywordAttribute>(propertyInfo) : null;

	/// <summary>
	/// Determine whether the specified property name is a keyword.
	/// </summary>
	/// <param name="propertyName">The property name.</param>
	/// <param name="containingType">The property containing type.</param>
	/// <param name="propertyInfo">The property information instance.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	internal static bool IsKeyword(string propertyName, Type containingType, [NotNullWhen(true)] out PropertyInfo? propertyInfo)
	{
		if (containingType.GetProperty(propertyName, PropertyBindingFlags) is not { } p)
		{
			propertyInfo = null;
			return false;
		}

		(propertyInfo, var @return) = p.IsDefined<KeywordAttribute>() ? (p, true) : (null, false);
		return @return;
	}

	/// <summary>
	/// Retrieves the attribute applied to a property according to custom inheritance rules.
	/// </summary>
	/// <typeparam name="TAttribute">The type of the attribute to retrieve.</typeparam>
	/// <param name="property">The property to inspect.</param>
	/// <returns>
	/// The attribute instance based on these rules:
	/// <list type="bullet">
	/// <item>For <see langword="override"/> properties: Returns base class attribute</item>
	/// <item>For <see langword="new"/> properties: Returns only if defined on current property</item>
	/// <item>For non-overridden properties: Returns base class attribute</item>
	/// </list>
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown when property is <see langword="null"/>.</exception>
	private static TAttribute? GetAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute
	{
		ArgumentNullException.ThrowIfNull(property);

		// Check if the property uses 'new' keyword (hides base member).
		var isNewProperty = IsNewProperty(property);

		// Case 2: Property uses 'new' keyword.
		if (isNewProperty)
		{
			// Only inspect current property, ignore base classes.
			return property.GetCustomAttribute<TAttribute>(false);
		}

		// Case 1 & 3: Property uses override or has no override.
		return FindInBaseClasses<TAttribute>(property);
	}

	/// <summary>
	/// Determines if the property is declared with the <see langword="new"/> keyword.
	/// </summary>
	/// <param name="property">Property to check.</param>
	/// <returns>
	/// True if property hides base member with <see langword="new"/> keyword, False otherwise.
	/// </returns>
	private static bool IsNewProperty(PropertyInfo property)
	{
		// Get the property accessor (prefer get method if available).
		if ((property.GetGetMethod(true) ?? property.GetSetMethod(true)) is not { } accessor)
		{
			// Cannot determine without accessor.
			return false;
		}

		// Get the base definition of the accessor method.
		var baseDefinition = accessor.GetBaseDefinition();

		// Check if method is an override (declaring type differs from base definition).
		var isOverride = accessor.DeclaringType != baseDefinition.DeclaringType;

		// Conditions for 'new' property:
		// 1. Not an override method
		// 2. Base class has property with same name
		return !isOverride && baseDefinition.DeclaringType is not null
			&& baseDefinition.DeclaringType.GetProperty(property.Name, PropertyBindingFlags) is not null;
	}

	/// <summary>
	/// Finds the attribute in the property's inheritance hierarchy.
	/// </summary>
	/// <typeparam name="TAttribute">Attribute type to find.</typeparam>
	/// <param name="property">Starting property.</param>
	/// <returns>
	/// First found attribute in base classes, or <see langword="null"/> if not found.
	/// </returns>
	private static TAttribute? FindInBaseClasses<TAttribute>(PropertyInfo property) where TAttribute : Attribute
	{
		var currentType = property.DeclaringType;
		var propertyName = property.Name;

		// Traverse base classes upward.
		while (currentType is not null)
		{
			// Get property declared in current type.
			var currentProperty = currentType.GetProperty(propertyName, PropertyBindingFlags);
			if (currentProperty?.GetCustomAttribute<TAttribute>(false) is { } attribute)
			{
				return attribute;
			}

			// Move to base type.
			currentType = currentType.BaseType;
		}

		return null; // Attribute not found.
	}
}

/// <summary>
/// Collect all instance properties declared in a type and its base types,
/// including private ones, excluding static and interface properties.
/// Overrides are only counted once (even if re-<see langword="abstract"/>ed),
/// while <see langword="new"/> properties are counted separately.
/// </summary>
file static class PropertyCollector
{
	/// <summary>
	/// Represents instance property flags.
	/// </summary>
	private const BindingFlags InstancePropertyFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;


	/// <summary>
	/// Retrieves all instance properties declared in the given type and its base types.
	/// This includes non-<see langword="public"/> properties. Overrides are only counted once.
	/// Properties declared with <see langword="new"/> are treated as distinct.
	/// </summary>
	/// <param name="type">The leaf type to begin property collection from.</param>
	/// <returns>A list of PropertyInfo instances that match the criteria.</returns>
	public static List<PropertyInfo> GetAllDeclaredProperties(Type type)
	{
		var result = new List<PropertyInfo>();
		var overriddenMethods = new HashSet<MethodInfo>(); // Tracks already counted base method definitions.
		var collectedProperties = new Dictionary<string, PropertyInfo>(); // Unique key => property.

		var current = type;
		while (current is not null && current != typeof(object))
		{
			// Get only instance-level, declared-only properties of current type.
			var properties = current.GetProperties(InstancePropertyFlags);

			foreach (var prop in properties)
			{
				if (prop.GetIndexParameters().Length != 0)
				{
					// Skip for indexers.
					continue;
				}

				// Get accessor methods (getter/setter).
				var getMethod = prop.GetGetMethod(true);
				var setMethod = prop.GetSetMethod(true);
				var method = getMethod ?? setMethod;
				if (method is null)
				{
					continue;
				}

				var isOverride = method.GetBaseDefinition() != method;
				var isNew = IsNewMethod(method);

				// Generate a key based on property name and signature.
				var key = prop.Name;

				// Skip overridden methods if already seen.
				if (isOverride)
				{
					var baseDef = method.GetBaseDefinition();
					if (overriddenMethods.Contains(baseDef))
					{
						continue;
					}
					overriddenMethods.Add(baseDef);
				}

				// Skip duplicate properties unless marked with 'new'.
				if (collectedProperties.ContainsKey(key) && !isNew)
				{
					continue;
				}

				collectedProperties[key] = prop;
				result.Add(prop);
			}

			current = current.BaseType;
		}

		return result;
	}

	/// <summary>
	/// Determines whether a method is using the <see langword="new"/> keyword (i.e., it hides a base method).
	/// </summary>
	/// <param name="method">The method to inspect.</param>
	/// <returns>True if the method is <see langword="new"/>, false otherwise.</returns>
	private static bool IsNewMethod(MethodInfo method)
		=> method.GetBaseDefinition() is var baseDef && baseDef != method && method.DeclaringType != baseDef.DeclaringType;
}
