namespace Sudoku.Concepts;

using static NearestFarthestCell;

/// <summary>
/// Represents logical position between cells.
/// </summary>
public static class LogicalPosition
{
	/// <summary>
	/// <para>
	/// Represents a lookup table that can know which cell is the nearest cell to a certain cell
	/// and which is the farthest cell, to the target house.
	/// </para>
	/// <para>
	/// Use <c><see cref="NearestFarthestCellsLookup"/>[Cell][House]</c> to get the result;
	/// or use extension member <see cref="extension(Cell).GetNearestFarthestCellTo(House)"/>.
	/// </para>
	/// </summary>
	/// <seealso cref="extension(Cell).GetNearestFarthestCellTo(House)"/>
	private static readonly NearestFarthestCell[][] NearestFarthestCellsLookup = [
		[Untouched, new(3, 5), new(6, 8), new(27, 45), Untouched, Untouched, new(54, 72), Untouched, Untouched, Untouched, new(9, 11), new(18, 20), new(27, 27), new(36, 36), new(45, 45), new(54, 54), new(63, 63), new(72, 72), Untouched, new(1, 19), new(2, 20), new(3, 3), new(4, 4), new(5, 5), new(6, 6), new(7, 7), new(8, 8)],
		[Untouched, new(3, 5), new(6, 8), new(28, 46), Untouched, Untouched, new(55, 73), Untouched, Untouched, Untouched, new(10, 11), new(19, 20), new(28, 28), new(37, 37), new(46, 46), new(55, 55), new(64, 64), new(73, 73), new(0, 18), Untouched, new(2, 20), new(3, 3), new(4, 4), new(5, 5), new(6, 6), new(7, 7), new(8, 8)],
		[Untouched, new(3, 5), new(6, 8), new(29, 47), Untouched, Untouched, new(56, 74), Untouched, Untouched, Untouched, new(11, 9), new(20, 18), new(29, 29), new(38, 38), new(47, 47), new(56, 56), new(65, 65), new(74, 74), new(0, 18), new(1, 19), Untouched, new(3, 3), new(4, 4), new(5, 5), new(6, 6), new(7, 7), new(8, 8)],
		[new(2, 0), Untouched, new(6, 8), Untouched, new(30, 48), Untouched, Untouched, new(57, 75), Untouched, Untouched, new(12, 14), new(21, 23), new(30, 30), new(39, 39), new(48, 48), new(57, 57), new(66, 66), new(75, 75), new(0, 0), new(1, 1), new(2, 2), Untouched, new(4, 22), new(5, 23), new(6, 6), new(7, 7), new(8, 8)],
		[new(2, 0), Untouched, new(6, 8), Untouched, new(31, 49), Untouched, Untouched, new(58, 76), Untouched, Untouched, new(13, 14), new(22, 23), new(31, 31), new(40, 40), new(49, 49), new(58, 58), new(67, 67), new(76, 76), new(0, 0), new(1, 1), new(2, 2), new(3, 21), Untouched, new(5, 23), new(6, 6), new(7, 7), new(8, 8)],
		[new(2, 0), Untouched, new(6, 8), Untouched, new(32, 50), Untouched, Untouched, new(59, 77), Untouched, Untouched, new(14, 12), new(23, 21), new(32, 32), new(41, 41), new(50, 50), new(59, 59), new(68, 68), new(77, 77), new(0, 0), new(1, 1), new(2, 2), new(3, 21), new(4, 22), Untouched, new(6, 6), new(7, 7), new(8, 8)],
		[new(2, 0), new(5, 3), Untouched, Untouched, Untouched, new(33, 51), Untouched, Untouched, new(60, 78), Untouched, new(15, 17), new(24, 26), new(33, 33), new(42, 42), new(51, 51), new(60, 60), new(69, 69), new(78, 78), new(0, 0), new(1, 1), new(2, 2), new(3, 3), new(4, 4), new(5, 5), Untouched, new(7, 25), new(8, 26)],
		[new(2, 0), new(5, 3), Untouched, Untouched, Untouched, new(34, 52), Untouched, Untouched, new(61, 79), Untouched, new(16, 17), new(25, 26), new(34, 34), new(43, 43), new(52, 52), new(61, 61), new(70, 70), new(79, 79), new(0, 0), new(1, 1), new(2, 2), new(3, 3), new(4, 4), new(5, 5), new(6, 24), Untouched, new(8, 26)],
		[new(2, 0), new(5, 3), Untouched, Untouched, Untouched, new(35, 53), Untouched, Untouched, new(62, 80), Untouched, new(17, 15), new(26, 24), new(35, 35), new(44, 44), new(53, 53), new(62, 62), new(71, 71), new(80, 80), new(0, 0), new(1, 1), new(2, 2), new(3, 3), new(4, 4), new(5, 5), new(6, 24), new(7, 25), Untouched],
		[Untouched, new(12, 14), new(15, 17), new(27, 45), Untouched, Untouched, new(54, 72), Untouched, Untouched, new(0, 2), Untouched, new(18, 20), new(27, 27), new(36, 36), new(45, 45), new(54, 54), new(63, 63), new(72, 72), Untouched, new(10, 19), new(11, 20), new(12, 12), new(13, 13), new(14, 14), new(15, 15), new(16, 16), new(17, 17)],
		[Untouched, new(12, 14), new(15, 17), new(28, 46), Untouched, Untouched, new(55, 73), Untouched, Untouched, new(1, 2), Untouched, new(19, 20), new(28, 28), new(37, 37), new(46, 46), new(55, 55), new(64, 64), new(73, 73), new(9, 18), Untouched, new(11, 20), new(12, 12), new(13, 13), new(14, 14), new(15, 15), new(16, 16), new(17, 17)],
		[Untouched, new(12, 14), new(15, 17), new(29, 47), Untouched, Untouched, new(56, 74), Untouched, Untouched, new(2, 0), Untouched, new(20, 18), new(29, 29), new(38, 38), new(47, 47), new(56, 56), new(65, 65), new(74, 74), new(9, 18), new(10, 19), Untouched, new(12, 12), new(13, 13), new(14, 14), new(15, 15), new(16, 16), new(17, 17)],
		[new(11, 9), Untouched, new(15, 17), Untouched, new(30, 48), Untouched, Untouched, new(57, 75), Untouched, new(3, 5), Untouched, new(21, 23), new(30, 30), new(39, 39), new(48, 48), new(57, 57), new(66, 66), new(75, 75), new(9, 9), new(10, 10), new(11, 11), Untouched, new(13, 22), new(14, 23), new(15, 15), new(16, 16), new(17, 17)],
		[new(11, 9), Untouched, new(15, 17), Untouched, new(31, 49), Untouched, Untouched, new(58, 76), Untouched, new(4, 5), Untouched, new(22, 23), new(31, 31), new(40, 40), new(49, 49), new(58, 58), new(67, 67), new(76, 76), new(9, 9), new(10, 10), new(11, 11), new(12, 21), Untouched, new(14, 23), new(15, 15), new(16, 16), new(17, 17)],
		[new(11, 9), Untouched, new(15, 17), Untouched, new(32, 50), Untouched, Untouched, new(59, 77), Untouched, new(5, 3), Untouched, new(23, 21), new(32, 32), new(41, 41), new(50, 50), new(59, 59), new(68, 68), new(77, 77), new(9, 9), new(10, 10), new(11, 11), new(12, 21), new(13, 22), Untouched, new(15, 15), new(16, 16), new(17, 17)],
		[new(11, 9), new(14, 12), Untouched, Untouched, Untouched, new(33, 51), Untouched, Untouched, new(60, 78), new(6, 8), Untouched, new(24, 26), new(33, 33), new(42, 42), new(51, 51), new(60, 60), new(69, 69), new(78, 78), new(9, 9), new(10, 10), new(11, 11), new(12, 12), new(13, 13), new(14, 14), Untouched, new(16, 25), new(17, 26)],
		[new(11, 9), new(14, 12), Untouched, Untouched, Untouched, new(34, 52), Untouched, Untouched, new(61, 79), new(7, 8), Untouched, new(25, 26), new(34, 34), new(43, 43), new(52, 52), new(61, 61), new(70, 70), new(79, 79), new(9, 9), new(10, 10), new(11, 11), new(12, 12), new(13, 13), new(14, 14), new(15, 24), Untouched, new(17, 26)],
		[new(11, 9), new(14, 12), Untouched, Untouched, Untouched, new(35, 53), Untouched, Untouched, new(62, 80), new(8, 6), Untouched, new(26, 24), new(35, 35), new(44, 44), new(53, 53), new(62, 62), new(71, 71), new(80, 80), new(9, 9), new(10, 10), new(11, 11), new(12, 12), new(13, 13), new(14, 14), new(15, 24), new(16, 25), Untouched],
		[Untouched, new(21, 23), new(24, 26), new(27, 45), Untouched, Untouched, new(54, 72), Untouched, Untouched, new(0, 2), new(9, 11), Untouched, new(27, 27), new(36, 36), new(45, 45), new(54, 54), new(63, 63), new(72, 72), Untouched, new(19, 1), new(20, 2), new(21, 21), new(22, 22), new(23, 23), new(24, 24), new(25, 25), new(26, 26)],
		[Untouched, new(21, 23), new(24, 26), new(28, 46), Untouched, Untouched, new(55, 73), Untouched, Untouched, new(1, 2), new(10, 11), Untouched, new(28, 28), new(37, 37), new(46, 46), new(55, 55), new(64, 64), new(73, 73), new(18, 0), Untouched, new(20, 2), new(21, 21), new(22, 22), new(23, 23), new(24, 24), new(25, 25), new(26, 26)],
		[Untouched, new(21, 23), new(24, 26), new(29, 47), Untouched, Untouched, new(56, 74), Untouched, Untouched, new(2, 0), new(11, 9), Untouched, new(29, 29), new(38, 38), new(47, 47), new(56, 56), new(65, 65), new(74, 74), new(18, 0), new(19, 1), Untouched, new(21, 21), new(22, 22), new(23, 23), new(24, 24), new(25, 25), new(26, 26)],
		[new(20, 18), Untouched, new(24, 26), Untouched, new(30, 48), Untouched, Untouched, new(57, 75), Untouched, new(3, 5), new(12, 14), Untouched, new(30, 30), new(39, 39), new(48, 48), new(57, 57), new(66, 66), new(75, 75), new(18, 18), new(19, 19), new(20, 20), Untouched, new(22, 4), new(23, 5), new(24, 24), new(25, 25), new(26, 26)],
		[new(20, 18), Untouched, new(24, 26), Untouched, new(31, 49), Untouched, Untouched, new(58, 76), Untouched, new(4, 5), new(13, 14), Untouched, new(31, 31), new(40, 40), new(49, 49), new(58, 58), new(67, 67), new(76, 76), new(18, 18), new(19, 19), new(20, 20), new(21, 3), Untouched, new(23, 5), new(24, 24), new(25, 25), new(26, 26)],
		[new(20, 18), Untouched, new(24, 26), Untouched, new(32, 50), Untouched, Untouched, new(59, 77), Untouched, new(5, 3), new(14, 12), Untouched, new(32, 32), new(41, 41), new(50, 50), new(59, 59), new(68, 68), new(77, 77), new(18, 18), new(19, 19), new(20, 20), new(21, 3), new(22, 4), Untouched, new(24, 24), new(25, 25), new(26, 26)],
		[new(20, 18), new(23, 21), Untouched, Untouched, Untouched, new(33, 51), Untouched, Untouched, new(60, 78), new(6, 8), new(15, 17), Untouched, new(33, 33), new(42, 42), new(51, 51), new(60, 60), new(69, 69), new(78, 78), new(18, 18), new(19, 19), new(20, 20), new(21, 21), new(22, 22), new(23, 23), Untouched, new(25, 7), new(26, 8)],
		[new(20, 18), new(23, 21), Untouched, Untouched, Untouched, new(34, 52), Untouched, Untouched, new(61, 79), new(7, 8), new(16, 17), Untouched, new(34, 34), new(43, 43), new(52, 52), new(61, 61), new(70, 70), new(79, 79), new(18, 18), new(19, 19), new(20, 20), new(21, 21), new(22, 22), new(23, 23), new(24, 6), Untouched, new(26, 8)],
		[new(20, 18), new(23, 21), Untouched, Untouched, Untouched, new(35, 53), Untouched, Untouched, new(62, 80), new(8, 6), new(17, 15), Untouched, new(35, 35), new(44, 44), new(53, 53), new(62, 62), new(71, 71), new(80, 80), new(18, 18), new(19, 19), new(20, 20), new(21, 21), new(22, 22), new(23, 23), new(24, 6), new(25, 7), Untouched],
		[new(18, 0), Untouched, Untouched, Untouched, new(30, 32), new(33, 35), new(54, 72), Untouched, Untouched, new(0, 0), new(9, 9), new(18, 18), Untouched, new(36, 38), new(45, 47), new(54, 54), new(63, 63), new(72, 72), Untouched, new(28, 46), new(29, 47), new(30, 30), new(31, 31), new(32, 32), new(33, 33), new(34, 34), new(35, 35)],
		[new(19, 1), Untouched, Untouched, Untouched, new(30, 32), new(33, 35), new(55, 73), Untouched, Untouched, new(1, 1), new(10, 10), new(19, 19), Untouched, new(37, 38), new(46, 47), new(55, 55), new(64, 64), new(73, 73), new(27, 45), Untouched, new(29, 47), new(30, 30), new(31, 31), new(32, 32), new(33, 33), new(34, 34), new(35, 35)],
		[new(20, 2), Untouched, Untouched, Untouched, new(30, 32), new(33, 35), new(56, 74), Untouched, Untouched, new(2, 2), new(11, 11), new(20, 20), Untouched, new(38, 36), new(47, 45), new(56, 56), new(65, 65), new(74, 74), new(27, 45), new(28, 46), Untouched, new(30, 30), new(31, 31), new(32, 32), new(33, 33), new(34, 34), new(35, 35)],
		[Untouched, new(21, 3), Untouched, new(29, 27), Untouched, new(33, 35), Untouched, new(57, 75), Untouched, new(3, 3), new(12, 12), new(21, 21), Untouched, new(39, 41), new(48, 50), new(57, 57), new(66, 66), new(75, 75), new(27, 27), new(28, 28), new(29, 29), Untouched, new(31, 49), new(32, 50), new(33, 33), new(34, 34), new(35, 35)],
		[Untouched, new(22, 4), Untouched, new(29, 27), Untouched, new(33, 35), Untouched, new(58, 76), Untouched, new(4, 4), new(13, 13), new(22, 22), Untouched, new(40, 41), new(49, 50), new(58, 58), new(67, 67), new(76, 76), new(27, 27), new(28, 28), new(29, 29), new(30, 48), Untouched, new(32, 50), new(33, 33), new(34, 34), new(35, 35)],
		[Untouched, new(23, 5), Untouched, new(29, 27), Untouched, new(33, 35), Untouched, new(59, 77), Untouched, new(5, 5), new(14, 14), new(23, 23), Untouched, new(41, 39), new(50, 48), new(59, 59), new(68, 68), new(77, 77), new(27, 27), new(28, 28), new(29, 29), new(30, 48), new(31, 49), Untouched, new(33, 33), new(34, 34), new(35, 35)],
		[Untouched, Untouched, new(24, 6), new(29, 27), new(32, 30), Untouched, Untouched, Untouched, new(60, 78), new(6, 6), new(15, 15), new(24, 24), Untouched, new(42, 44), new(51, 53), new(60, 60), new(69, 69), new(78, 78), new(27, 27), new(28, 28), new(29, 29), new(30, 30), new(31, 31), new(32, 32), Untouched, new(34, 52), new(35, 53)],
		[Untouched, Untouched, new(25, 7), new(29, 27), new(32, 30), Untouched, Untouched, Untouched, new(61, 79), new(7, 7), new(16, 16), new(25, 25), Untouched, new(43, 44), new(52, 53), new(61, 61), new(70, 70), new(79, 79), new(27, 27), new(28, 28), new(29, 29), new(30, 30), new(31, 31), new(32, 32), new(33, 51), Untouched, new(35, 53)],
		[Untouched, Untouched, new(26, 8), new(29, 27), new(32, 30), Untouched, Untouched, Untouched, new(62, 80), new(8, 8), new(17, 17), new(26, 26), Untouched, new(44, 42), new(53, 51), new(62, 62), new(71, 71), new(80, 80), new(27, 27), new(28, 28), new(29, 29), new(30, 30), new(31, 31), new(32, 32), new(33, 51), new(34, 52), Untouched],
		[new(18, 0), Untouched, Untouched, Untouched, new(39, 41), new(42, 44), new(54, 72), Untouched, Untouched, new(0, 0), new(9, 9), new(18, 18), new(27, 29), Untouched, new(45, 47), new(54, 54), new(63, 63), new(72, 72), Untouched, new(37, 46), new(38, 47), new(39, 39), new(40, 40), new(41, 41), new(42, 42), new(43, 43), new(44, 44)],
		[new(19, 1), Untouched, Untouched, Untouched, new(39, 41), new(42, 44), new(55, 73), Untouched, Untouched, new(1, 1), new(10, 10), new(19, 19), new(28, 29), Untouched, new(46, 47), new(55, 55), new(64, 64), new(73, 73), new(36, 45), Untouched, new(38, 47), new(39, 39), new(40, 40), new(41, 41), new(42, 42), new(43, 43), new(44, 44)],
		[new(20, 2), Untouched, Untouched, Untouched, new(39, 41), new(42, 44), new(56, 74), Untouched, Untouched, new(2, 2), new(11, 11), new(20, 20), new(29, 27), Untouched, new(47, 45), new(56, 56), new(65, 65), new(74, 74), new(36, 45), new(37, 46), Untouched, new(39, 39), new(40, 40), new(41, 41), new(42, 42), new(43, 43), new(44, 44)],
		[Untouched, new(21, 3), Untouched, new(38, 36), Untouched, new(42, 44), Untouched, new(57, 75), Untouched, new(3, 3), new(12, 12), new(21, 21), new(30, 32), Untouched, new(48, 50), new(57, 57), new(66, 66), new(75, 75), new(36, 36), new(37, 37), new(38, 38), Untouched, new(40, 49), new(41, 50), new(42, 42), new(43, 43), new(44, 44)],
		[Untouched, new(22, 4), Untouched, new(38, 36), Untouched, new(42, 44), Untouched, new(58, 76), Untouched, new(4, 4), new(13, 13), new(22, 22), new(31, 32), Untouched, new(49, 50), new(58, 58), new(67, 67), new(76, 76), new(36, 36), new(37, 37), new(38, 38), new(39, 48), Untouched, new(41, 50), new(42, 42), new(43, 43), new(44, 44)],
		[Untouched, new(23, 5), Untouched, new(38, 36), Untouched, new(42, 44), Untouched, new(59, 77), Untouched, new(5, 5), new(14, 14), new(23, 23), new(32, 30), Untouched, new(50, 48), new(59, 59), new(68, 68), new(77, 77), new(36, 36), new(37, 37), new(38, 38), new(39, 48), new(40, 49), Untouched, new(42, 42), new(43, 43), new(44, 44)],
		[Untouched, Untouched, new(24, 6), new(38, 36), new(41, 39), Untouched, Untouched, Untouched, new(60, 78), new(6, 6), new(15, 15), new(24, 24), new(33, 35), Untouched, new(51, 53), new(60, 60), new(69, 69), new(78, 78), new(36, 36), new(37, 37), new(38, 38), new(39, 39), new(40, 40), new(41, 41), Untouched, new(43, 52), new(44, 53)],
		[Untouched, Untouched, new(25, 7), new(38, 36), new(41, 39), Untouched, Untouched, Untouched, new(61, 79), new(7, 7), new(16, 16), new(25, 25), new(34, 35), Untouched, new(52, 53), new(61, 61), new(70, 70), new(79, 79), new(36, 36), new(37, 37), new(38, 38), new(39, 39), new(40, 40), new(41, 41), new(42, 51), Untouched, new(44, 53)],
		[Untouched, Untouched, new(26, 8), new(38, 36), new(41, 39), Untouched, Untouched, Untouched, new(62, 80), new(8, 8), new(17, 17), new(26, 26), new(35, 33), Untouched, new(53, 51), new(62, 62), new(71, 71), new(80, 80), new(36, 36), new(37, 37), new(38, 38), new(39, 39), new(40, 40), new(41, 41), new(42, 51), new(43, 52), Untouched],
		[new(18, 0), Untouched, Untouched, Untouched, new(48, 50), new(51, 53), new(54, 72), Untouched, Untouched, new(0, 0), new(9, 9), new(18, 18), new(27, 29), new(36, 38), Untouched, new(54, 54), new(63, 63), new(72, 72), Untouched, new(46, 28), new(47, 29), new(48, 48), new(49, 49), new(50, 50), new(51, 51), new(52, 52), new(53, 53)],
		[new(19, 1), Untouched, Untouched, Untouched, new(48, 50), new(51, 53), new(55, 73), Untouched, Untouched, new(1, 1), new(10, 10), new(19, 19), new(28, 29), new(37, 38), Untouched, new(55, 55), new(64, 64), new(73, 73), new(45, 27), Untouched, new(47, 29), new(48, 48), new(49, 49), new(50, 50), new(51, 51), new(52, 52), new(53, 53)],
		[new(20, 2), Untouched, Untouched, Untouched, new(48, 50), new(51, 53), new(56, 74), Untouched, Untouched, new(2, 2), new(11, 11), new(20, 20), new(29, 27), new(38, 36), Untouched, new(56, 56), new(65, 65), new(74, 74), new(45, 27), new(46, 28), Untouched, new(48, 48), new(49, 49), new(50, 50), new(51, 51), new(52, 52), new(53, 53)],
		[Untouched, new(21, 3), Untouched, new(47, 45), Untouched, new(51, 53), Untouched, new(57, 75), Untouched, new(3, 3), new(12, 12), new(21, 21), new(30, 32), new(39, 41), Untouched, new(57, 57), new(66, 66), new(75, 75), new(45, 45), new(46, 46), new(47, 47), Untouched, new(49, 31), new(50, 32), new(51, 51), new(52, 52), new(53, 53)],
		[Untouched, new(22, 4), Untouched, new(47, 45), Untouched, new(51, 53), Untouched, new(58, 76), Untouched, new(4, 4), new(13, 13), new(22, 22), new(31, 32), new(40, 41), Untouched, new(58, 58), new(67, 67), new(76, 76), new(45, 45), new(46, 46), new(47, 47), new(48, 30), Untouched, new(50, 32), new(51, 51), new(52, 52), new(53, 53)],
		[Untouched, new(23, 5), Untouched, new(47, 45), Untouched, new(51, 53), Untouched, new(59, 77), Untouched, new(5, 5), new(14, 14), new(23, 23), new(32, 30), new(41, 39), Untouched, new(59, 59), new(68, 68), new(77, 77), new(45, 45), new(46, 46), new(47, 47), new(48, 30), new(49, 31), Untouched, new(51, 51), new(52, 52), new(53, 53)],
		[Untouched, Untouched, new(24, 6), new(47, 45), new(50, 48), Untouched, Untouched, Untouched, new(60, 78), new(6, 6), new(15, 15), new(24, 24), new(33, 35), new(42, 44), Untouched, new(60, 60), new(69, 69), new(78, 78), new(45, 45), new(46, 46), new(47, 47), new(48, 48), new(49, 49), new(50, 50), Untouched, new(52, 34), new(53, 35)],
		[Untouched, Untouched, new(25, 7), new(47, 45), new(50, 48), Untouched, Untouched, Untouched, new(61, 79), new(7, 7), new(16, 16), new(25, 25), new(34, 35), new(43, 44), Untouched, new(61, 61), new(70, 70), new(79, 79), new(45, 45), new(46, 46), new(47, 47), new(48, 48), new(49, 49), new(50, 50), new(51, 33), Untouched, new(53, 35)],
		[Untouched, Untouched, new(26, 8), new(47, 45), new(50, 48), Untouched, Untouched, Untouched, new(62, 80), new(8, 8), new(17, 17), new(26, 26), new(35, 33), new(44, 42), Untouched, new(62, 62), new(71, 71), new(80, 80), new(45, 45), new(46, 46), new(47, 47), new(48, 48), new(49, 49), new(50, 50), new(51, 33), new(52, 34), Untouched],
		[new(18, 0), Untouched, Untouched, new(45, 27), Untouched, Untouched, Untouched, new(57, 59), new(60, 62), new(0, 0), new(9, 9), new(18, 18), new(27, 27), new(36, 36), new(45, 45), Untouched, new(63, 65), new(72, 74), Untouched, new(55, 73), new(56, 74), new(57, 57), new(58, 58), new(59, 59), new(60, 60), new(61, 61), new(62, 62)],
		[new(19, 1), Untouched, Untouched, new(46, 28), Untouched, Untouched, Untouched, new(57, 59), new(60, 62), new(1, 1), new(10, 10), new(19, 19), new(28, 28), new(37, 37), new(46, 46), Untouched, new(64, 65), new(73, 74), new(54, 72), Untouched, new(56, 74), new(57, 57), new(58, 58), new(59, 59), new(60, 60), new(61, 61), new(62, 62)],
		[new(20, 2), Untouched, Untouched, new(47, 29), Untouched, Untouched, Untouched, new(57, 59), new(60, 62), new(2, 2), new(11, 11), new(20, 20), new(29, 29), new(38, 38), new(47, 47), Untouched, new(65, 63), new(74, 72), new(54, 72), new(55, 73), Untouched, new(57, 57), new(58, 58), new(59, 59), new(60, 60), new(61, 61), new(62, 62)],
		[Untouched, new(21, 3), Untouched, Untouched, new(48, 30), Untouched, new(56, 54), Untouched, new(60, 62), new(3, 3), new(12, 12), new(21, 21), new(30, 30), new(39, 39), new(48, 48), Untouched, new(66, 68), new(75, 77), new(54, 54), new(55, 55), new(56, 56), Untouched, new(58, 76), new(59, 77), new(60, 60), new(61, 61), new(62, 62)],
		[Untouched, new(22, 4), Untouched, Untouched, new(49, 31), Untouched, new(56, 54), Untouched, new(60, 62), new(4, 4), new(13, 13), new(22, 22), new(31, 31), new(40, 40), new(49, 49), Untouched, new(67, 68), new(76, 77), new(54, 54), new(55, 55), new(56, 56), new(57, 75), Untouched, new(59, 77), new(60, 60), new(61, 61), new(62, 62)],
		[Untouched, new(23, 5), Untouched, Untouched, new(50, 32), Untouched, new(56, 54), Untouched, new(60, 62), new(5, 5), new(14, 14), new(23, 23), new(32, 32), new(41, 41), new(50, 50), Untouched, new(68, 66), new(77, 75), new(54, 54), new(55, 55), new(56, 56), new(57, 75), new(58, 76), Untouched, new(60, 60), new(61, 61), new(62, 62)],
		[Untouched, Untouched, new(24, 6), Untouched, Untouched, new(51, 33), new(56, 54), new(59, 57), Untouched, new(6, 6), new(15, 15), new(24, 24), new(33, 33), new(42, 42), new(51, 51), Untouched, new(69, 71), new(78, 80), new(54, 54), new(55, 55), new(56, 56), new(57, 57), new(58, 58), new(59, 59), Untouched, new(61, 79), new(62, 80)],
		[Untouched, Untouched, new(25, 7), Untouched, Untouched, new(52, 34), new(56, 54), new(59, 57), Untouched, new(7, 7), new(16, 16), new(25, 25), new(34, 34), new(43, 43), new(52, 52), Untouched, new(70, 71), new(79, 80), new(54, 54), new(55, 55), new(56, 56), new(57, 57), new(58, 58), new(59, 59), new(60, 78), Untouched, new(62, 80)],
		[Untouched, Untouched, new(26, 8), Untouched, Untouched, new(53, 35), new(56, 54), new(59, 57), Untouched, new(8, 8), new(17, 17), new(26, 26), new(35, 35), new(44, 44), new(53, 53), Untouched, new(71, 69), new(80, 78), new(54, 54), new(55, 55), new(56, 56), new(57, 57), new(58, 58), new(59, 59), new(60, 78), new(61, 79), Untouched],
		[new(18, 0), Untouched, Untouched, new(45, 27), Untouched, Untouched, Untouched, new(66, 68), new(69, 71), new(0, 0), new(9, 9), new(18, 18), new(27, 27), new(36, 36), new(45, 45), new(54, 56), Untouched, new(72, 74), Untouched, new(64, 73), new(65, 74), new(66, 66), new(67, 67), new(68, 68), new(69, 69), new(70, 70), new(71, 71)],
		[new(19, 1), Untouched, Untouched, new(46, 28), Untouched, Untouched, Untouched, new(66, 68), new(69, 71), new(1, 1), new(10, 10), new(19, 19), new(28, 28), new(37, 37), new(46, 46), new(55, 56), Untouched, new(73, 74), new(63, 72), Untouched, new(65, 74), new(66, 66), new(67, 67), new(68, 68), new(69, 69), new(70, 70), new(71, 71)],
		[new(20, 2), Untouched, Untouched, new(47, 29), Untouched, Untouched, Untouched, new(66, 68), new(69, 71), new(2, 2), new(11, 11), new(20, 20), new(29, 29), new(38, 38), new(47, 47), new(56, 54), Untouched, new(74, 72), new(63, 72), new(64, 73), Untouched, new(66, 66), new(67, 67), new(68, 68), new(69, 69), new(70, 70), new(71, 71)],
		[Untouched, new(21, 3), Untouched, Untouched, new(48, 30), Untouched, new(65, 63), Untouched, new(69, 71), new(3, 3), new(12, 12), new(21, 21), new(30, 30), new(39, 39), new(48, 48), new(57, 59), Untouched, new(75, 77), new(63, 63), new(64, 64), new(65, 65), Untouched, new(67, 76), new(68, 77), new(69, 69), new(70, 70), new(71, 71)],
		[Untouched, new(22, 4), Untouched, Untouched, new(49, 31), Untouched, new(65, 63), Untouched, new(69, 71), new(4, 4), new(13, 13), new(22, 22), new(31, 31), new(40, 40), new(49, 49), new(58, 59), Untouched, new(76, 77), new(63, 63), new(64, 64), new(65, 65), new(66, 75), Untouched, new(68, 77), new(69, 69), new(70, 70), new(71, 71)],
		[Untouched, new(23, 5), Untouched, Untouched, new(50, 32), Untouched, new(65, 63), Untouched, new(69, 71), new(5, 5), new(14, 14), new(23, 23), new(32, 32), new(41, 41), new(50, 50), new(59, 57), Untouched, new(77, 75), new(63, 63), new(64, 64), new(65, 65), new(66, 75), new(67, 76), Untouched, new(69, 69), new(70, 70), new(71, 71)],
		[Untouched, Untouched, new(24, 6), Untouched, Untouched, new(51, 33), new(65, 63), new(68, 66), Untouched, new(6, 6), new(15, 15), new(24, 24), new(33, 33), new(42, 42), new(51, 51), new(60, 62), Untouched, new(78, 80), new(63, 63), new(64, 64), new(65, 65), new(66, 66), new(67, 67), new(68, 68), Untouched, new(70, 79), new(71, 80)],
		[Untouched, Untouched, new(25, 7), Untouched, Untouched, new(52, 34), new(65, 63), new(68, 66), Untouched, new(7, 7), new(16, 16), new(25, 25), new(34, 34), new(43, 43), new(52, 52), new(61, 62), Untouched, new(79, 80), new(63, 63), new(64, 64), new(65, 65), new(66, 66), new(67, 67), new(68, 68), new(69, 78), Untouched, new(71, 80)],
		[Untouched, Untouched, new(26, 8), Untouched, Untouched, new(53, 35), new(65, 63), new(68, 66), Untouched, new(8, 8), new(17, 17), new(26, 26), new(35, 35), new(44, 44), new(53, 53), new(62, 60), Untouched, new(80, 78), new(63, 63), new(64, 64), new(65, 65), new(66, 66), new(67, 67), new(68, 68), new(69, 78), new(70, 79), Untouched],
		[new(18, 0), Untouched, Untouched, new(45, 27), Untouched, Untouched, Untouched, new(75, 77), new(78, 80), new(0, 0), new(9, 9), new(18, 18), new(27, 27), new(36, 36), new(45, 45), new(54, 56), new(63, 65), Untouched, Untouched, new(73, 55), new(74, 56), new(75, 75), new(76, 76), new(77, 77), new(78, 78), new(79, 79), new(80, 80)],
		[new(19, 1), Untouched, Untouched, new(46, 28), Untouched, Untouched, Untouched, new(75, 77), new(78, 80), new(1, 1), new(10, 10), new(19, 19), new(28, 28), new(37, 37), new(46, 46), new(55, 56), new(64, 65), Untouched, new(72, 54), Untouched, new(74, 56), new(75, 75), new(76, 76), new(77, 77), new(78, 78), new(79, 79), new(80, 80)],
		[new(20, 2), Untouched, Untouched, new(47, 29), Untouched, Untouched, Untouched, new(75, 77), new(78, 80), new(2, 2), new(11, 11), new(20, 20), new(29, 29), new(38, 38), new(47, 47), new(56, 54), new(65, 63), Untouched, new(72, 54), new(73, 55), Untouched, new(75, 75), new(76, 76), new(77, 77), new(78, 78), new(79, 79), new(80, 80)],
		[Untouched, new(21, 3), Untouched, Untouched, new(48, 30), Untouched, new(74, 72), Untouched, new(78, 80), new(3, 3), new(12, 12), new(21, 21), new(30, 30), new(39, 39), new(48, 48), new(57, 59), new(66, 68), Untouched, new(72, 72), new(73, 73), new(74, 74), Untouched, new(76, 58), new(77, 59), new(78, 78), new(79, 79), new(80, 80)],
		[Untouched, new(22, 4), Untouched, Untouched, new(49, 31), Untouched, new(74, 72), Untouched, new(78, 80), new(4, 4), new(13, 13), new(22, 22), new(31, 31), new(40, 40), new(49, 49), new(58, 59), new(67, 68), Untouched, new(72, 72), new(73, 73), new(74, 74), new(75, 57), Untouched, new(77, 59), new(78, 78), new(79, 79), new(80, 80)],
		[Untouched, new(23, 5), Untouched, Untouched, new(50, 32), Untouched, new(74, 72), Untouched, new(78, 80), new(5, 5), new(14, 14), new(23, 23), new(32, 32), new(41, 41), new(50, 50), new(59, 57), new(68, 66), Untouched, new(72, 72), new(73, 73), new(74, 74), new(75, 57), new(76, 58), Untouched, new(78, 78), new(79, 79), new(80, 80)],
		[Untouched, Untouched, new(24, 6), Untouched, Untouched, new(51, 33), new(74, 72), new(77, 75), Untouched, new(6, 6), new(15, 15), new(24, 24), new(33, 33), new(42, 42), new(51, 51), new(60, 62), new(69, 71), Untouched, new(72, 72), new(73, 73), new(74, 74), new(75, 75), new(76, 76), new(77, 77), Untouched, new(79, 61), new(80, 62)],
		[Untouched, Untouched, new(25, 7), Untouched, Untouched, new(52, 34), new(74, 72), new(77, 75), Untouched, new(7, 7), new(16, 16), new(25, 25), new(34, 34), new(43, 43), new(52, 52), new(61, 62), new(70, 71), Untouched, new(72, 72), new(73, 73), new(74, 74), new(75, 75), new(76, 76), new(77, 77), new(78, 60), Untouched, new(80, 62)],
		[Untouched, Untouched, new(26, 8), Untouched, Untouched, new(53, 35), new(74, 72), new(77, 75), Untouched, new(8, 8), new(17, 17), new(26, 26), new(35, 35), new(44, 44), new(53, 53), new(62, 60), new(71, 69), Untouched, new(72, 72), new(73, 73), new(74, 74), new(75, 75), new(76, 76), new(77, 77), new(78, 60), new(79, 61), Untouched]
	];


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="cell">The current cell.</param>
	extension(Cell cell)
	{
		/// <summary>
		/// Returns the nearest and farthest cell of <paramref name="cell"/>, to <paramref name="house"/>.
		/// </summary>
		/// <param name="house">The house.</param>
		/// <returns>The nearest and farthest cell.</returns>
		public NearestFarthestCell GetNearestFarthestCellTo(House house) => NearestFarthestCellsLookup[cell][house];


		/// <summary>
		/// Returns distance value between two <see cref="Cell"/>s.
		/// </summary>
		/// <param name="cell1">The first cell to be checked.</param>
		/// <param name="cell2">The second cell to be checked.</param>
		/// <returns>A <see cref="double"/> value indicating the distance.</returns>
		public static double GetDistance(Cell cell1, Cell cell2)
		{
			var (x1, y1) = (cell1 / 9, cell1 % 9);
			var (x2, y2) = (cell2 / 9, cell2 % 9);
			return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
		}

		/// <summary>
		/// Try to get the intermediate cells that are between <paramref name="cell1"/> and <paramref name="cell2"/> in logical position
		/// for sudoku grid.
		/// </summary>
		/// <param name="cell1">The first cell.</param>
		/// <param name="cell2">The second cell. The value should be greater than <paramref name="cell1"/>.</param>
		/// <returns>The intermediate cells.</returns>
		/// <exception cref="InvalidOperationException">
		/// Throws when cells <paramref name="cell1"/> and <paramref name="cell2"/> are not in a same line (row or column).
		/// </exception>
		public static CellMap GetIntermediateCells(Cell cell1, Cell cell2)
		{
			if (cell1 == cell2)
			{
				return [];
			}

			if (cell1 > cell2)
			{
				// Keeps the less value as the first cell.
				Unsafe.Swap(ref cell1, ref cell2);
			}

			if ((cell1.AsCellMap() + cell2).SharedLine is not (var sharedHouse and not FallbackConstants.@int))
			{
				throw new InvalidOperationException(SR.ExceptionMessage("CellsShouldInSameLine"));
			}

			var houseCells = HousesCells[sharedHouse];
			return houseCells[(Array.IndexOf(houseCells, cell1) + 1)..Array.IndexOf(houseCells, cell2)].AsCellMap();
		}
	}
}
