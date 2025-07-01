namespace Sudoku.Generating.Filtering;

/// <summary>
/// Represents a type or member predicate.
/// </summary>
/// <param name="typeOrMemberName">The name of type or member.</param>
/// <param name="basedOn">The member kind based on.</param>
/// <returns>A <see cref="bool"/> result indicating that.</returns>
public delegate bool TypeOrMemberPredicate(string typeOrMemberName, StepAnnotationAttribute.BasedOn basedOn);
