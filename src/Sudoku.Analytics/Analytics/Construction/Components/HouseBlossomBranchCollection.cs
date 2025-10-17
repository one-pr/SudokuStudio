namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents for a house blossom branch collection.
/// </summary>
public sealed class HouseBlossomBranchCollection :
	DeathBlossomBranchCollection<HouseBlossomBranchCollection, Cell>,
	IEqualityOperators<HouseBlossomBranchCollection, HouseBlossomBranchCollection, bool>
{
	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as HouseBlossomBranchCollection);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] HouseBlossomBranchCollection? other)
	{
		if (other is null)
		{
			return false;
		}

		CellMap thisCells = [.. Keys];
		CellMap otherCells = [.. other.Keys];
		if (thisCells != otherCells)
		{
			return false;
		}

		foreach (var cell in thisCells)
		{
			var thisAls = this[cell];
			var otherAls = other[cell];
			if (thisAls != otherAls)
			{
				return false;
			}
		}

		return true;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var result = default(HashCode);
		foreach (var (key, value) in this)
		{
			result.Add(key << 17 | 135792468);
			result.Add(value.GetHashCode());
		}

		return result.ToHashCode();
	}


	/// <inheritdoc/>
	public static bool operator ==(HouseBlossomBranchCollection? left, HouseBlossomBranchCollection? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(HouseBlossomBranchCollection? left, HouseBlossomBranchCollection? right) => !(left == right);
}
