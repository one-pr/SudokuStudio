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


	/// <inheritdoc cref="IsKeyword{TStep}(string, out PropertyInfo?)"/>
	public static bool IsKeyword<TStep>(PropertyInfo propertyInfo) where TStep : Step
		=> GetAttribute<KeywordAttribute>(propertyInfo) is not null;

	/// <summary>
	/// Retrieves possible property names that are marked <see cref="KeywordAttribute"/>.
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <returns>A list of property names that are marked <see cref="KeywordAttribute"/>.</returns>
	/// <seealso cref="KeywordAttribute"/>
	public static ReadOnlySpan<string> GetKeywordNames<TStep>() where TStep : Step
		=>
		from propertyInfo in typeof(TStep).GetProperties(PropertyBindingFlags)
		where IsKeyword<TStep>(propertyInfo)
		select propertyInfo.Name;

	/// <summary>
	/// Retrieves possible keyword verbs.
	/// </summary>
	/// <typeparam name="TStep">THe type of step.</typeparam>
	/// <param name="propertyName">The property name.</param>
	/// <returns>All keyword verbs allowed.</returns>
	public static ReadOnlySpan<KeywordVerb> GetKeywordVerbs<TStep>(string propertyName) where TStep : Step
	{
		if (!IsKeyword<TStep>(propertyName, out var propertyInfo))
		{
			return [];
		}

		if (GetAttribute<KeywordAttribute>(propertyInfo) is not { AllowedVerbs: var verbs })
		{
			return [];
		}

		var result = new HashSet<KeywordVerb>();
		foreach (var verb in verbs)
		{
			result.Add(verb);
		}
		return result.AsReadOnlySpan();
	}

	/// <summary>
	/// Retrieves possible keyword conditions of a keyword.
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <param name="propertyName">The property name.</param>
	/// <returns>All marked conditions.</returns>
	public static ReadOnlySpan<KeywordConditionAttribute> GetKeywordConditions<TStep>(string propertyName)
		where TStep : Step
	{
		if (!IsKeyword<TStep>(propertyName, out var propertyInfo))
		{
			return [];
		}

		var attributes = (Attribute[])propertyInfo.GetCustomAttributes();
		var result = new List<KeywordConditionAttribute>(attributes.Length);
		foreach (var attribute in attributes)
		{
			if (attribute is KeywordConditionAttribute instance)
			{
				result.Add(instance);
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Retrieve keyword condition of the specified keyword;
	/// if unset, <see langword="null"/> will be returned.
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <typeparam name="TAttribute">The type of attribute.</typeparam>
	/// <returns>
	/// The valid condition set returned, or <see langword="null"/>
	/// if the target property is either not found or not a valid keyword.
	/// </returns>
	public static TAttribute? GetKeywordCondition<TStep, TAttribute>(string propertyName)
		where TStep : Step
		where TAttribute : KeywordConditionAttribute
		=> IsKeyword<TStep>(propertyName, out var propertyInfo) ? GetAttribute<TAttribute>(propertyInfo) : null;

	/// <summary>
	/// Determine whether the specified property name is a keyword (a property to be used by filtering).
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <param name="propertyName">The property name.</param>
	/// <param name="propertyInfo">The property information instance.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private static bool IsKeyword<TStep>(string propertyName, [NotNullWhen(true)] out PropertyInfo? propertyInfo)
		where TStep : Step
	{
		if (typeof(TStep).GetProperty(propertyName, PropertyBindingFlags) is not { } p)
		{
			propertyInfo = null;
			return false;
		}

		(propertyInfo, var @return) = IsKeyword<TStep>(p) ? (p, true) : (null, false);
		return @return;
	}

	/// <summary>
	/// Retrieves the attribute applied to a property according to custom inheritance rules.
	/// </summary>
	/// <typeparam name="TAttribute">The type of the attribute to retrieve.</typeparam>
	/// <param name="property">The property to inspect.</param>
	/// <returns>
	/// The attribute instance based on these rules:
	/// <list type="number">
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
