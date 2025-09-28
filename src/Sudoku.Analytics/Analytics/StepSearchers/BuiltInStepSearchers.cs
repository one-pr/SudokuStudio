#pragma warning disable IDE0055
#pragma warning disable CS1591

// This file stores built-in step searchers used.
// You can modify the ordering of step searchers to control the priority on invocation
// by an Analyzer instance.

namespace Sudoku.Analytics.StepSearchers;

public partial class SingleStepSearcher								(): StepSearcher(00, 0);
public partial class LawOfLeftoverStepSearcher						(): StepSearcher(01, 0);
public partial class DirectIntersectionStepSearcher					(): StepSearcher(02, 0);
public partial class DirectSubsetStepSearcher						(): StepSearcher(03, 0);
public partial class ComplexSingleStepSearcher						(): StepSearcher(04, 0);
public partial class LockedSubsetStepSearcher						(): StepSearcher(05, 0);
public partial class LockedCandidatesStepSearcher					(): StepSearcher(06, 0);
public partial class NormalSubsetStepSearcher						(): StepSearcher(07, 0);
public partial class NormalFishStepSearcher							(): StepSearcher(08, 1);
public partial class TwoStrongLinksStepSearcher						(): StepSearcher(09, 1);
public partial class GroupedTwoStrongLinksStepSearcher				(): StepSearcher(10, 1);
public partial class RegularWingStepSearcher						(): StepSearcher(11, 1);
public partial class UniqueRectangleStepSearcher					(): StepSearcher(12, 1);
public partial class AlmostLockedCandidatesStepSearcher				(): StepSearcher(13, 1);
public partial class RemotePairStepSearcher							(): StepSearcher(14, 1);
public partial class SueDeCoqStepSearcher							(): StepSearcher(15, 1);
public partial class SueDeCoq3DimensionStepSearcher					(): StepSearcher(16, 1);
public partial class IrregularWingStepSearcher						(): StepSearcher(17, 1);
public partial class ExtendedSubsetPrincipleStepSearcher			(): StepSearcher(18, 1);
public partial class FatRingStepSearcher							(): StepSearcher(19, 1);
public partial class UniqueLoopStepSearcher							(): StepSearcher(20, 1);
public partial class ExtendedRectangleStepSearcher					(): StepSearcher(21, 1);
public partial class EmptyRectangleStepSearcher						(): StepSearcher(22, 1);
public partial class BorescoperDeadlyPatternStepSearcher			(): StepSearcher(23, 1);
public partial class UniqueMatrixStepSearcher						(): StepSearcher(24, 1);
public partial class AnonymousDeadlyPatternStepSearcher				(): StepSearcher(25, 1);
public partial class BivalueUniversalGraveStepSearcher				(): StepSearcher(26, 1);
public partial class QiuDeadlyPatternStepSearcher					(): StepSearcher(27, 1);
public partial class ReverseBivalueUniversalGraveStepSearcher		(): StepSearcher(28, 1);
public partial class EmptyRectangleIntersectionPairStepSearcher		(): StepSearcher(29, 1);
public partial class FireworkStepSearcher							(): StepSearcher(30, 1);
public partial class AntiGurthSymmetricalPlacementStepSearcher		(): StepSearcher(31, 1);
public partial class BivalueOddagonStepSearcher						(): StepSearcher(32, 2);
public partial class TrivalueOddagonStepSearcher					(): StepSearcher(33, 2);
public partial class XyzRingStepSearcher							(): StepSearcher(34, 2);
public partial class UniquenessClueCoverStepSearcher				(): StepSearcher(35, 2);
public partial class ChainStepSearcher								(): StepSearcher(36, 2);
public partial class AlignedExclusionStepSearcher					(): StepSearcher(37, 2);
public partial class GuardianStepSearcher							(): StepSearcher(38, 2);
public partial class ComplexFishStepSearcher						(): StepSearcher(39, 2);
public partial class GroupedChainStepSearcher						(): StepSearcher(40, 2);
public partial class BlossomLoopStepSearcher						(): StepSearcher(41, 2);
public partial class FinnedChainStepSearcher						(): StepSearcher(42, 2);
public partial class GroupedFinnedChainStepSearcher					(): StepSearcher(43, 2);
public partial class FinnedRectangleChainStepSearcher				(): StepSearcher(44, 2);
public partial class MultipleForcingChainsStepSearcher				(): StepSearcher(45, 2);
public partial class RectangleForcingChainsStepSearcher				(): StepSearcher(46, 2);
public partial class BivalueUniversalGraveForcingChainsStepSearcher	(): StepSearcher(47, 2);
public partial class DeathBlossomStepSearcher						(): StepSearcher(48, 2);
public partial class ExocetStepSearcher								(): StepSearcher(49, 3);
public partial class DominoLoopStepSearcher							(): StepSearcher(50, 3);
public partial class MultisectorLockedSetsStepSearcher				(): StepSearcher(51, 3);
public partial class MultifishStepSearcher							(): StepSearcher(52, 3);
public partial class WhipStepSearcher								(): StepSearcher(53, 3);
public partial class DynamicForcingChainsStepSearcher				(): StepSearcher(54, 3);
public partial class BowmanBingoStepSearcher						(): StepSearcher(55, 3, StepSearcherRunningArea.None);
public partial class PatternOverlayStepSearcher						(): StepSearcher(56, 3, StepSearcherRunningArea.Collecting);
public partial class BruteForceStepSearcher							(): StepSearcher(57, 4, StepSearcherRunningArea.Searching);
