#pragma warning disable IDE0055
#pragma warning disable CS1591
#pragma warning disable format

namespace Sudoku.Analytics.StepSearchers;

// This file stores built-in step searchers used.
// You can modify the ordering of step searchers to control the priority on invocation
// by an Analyzer instance.

public partial class SingleStepSearcher								(): StepSearcher(00, 0);
public partial class LawOfLeftoverStepSearcher						(): StepSearcher(01, 0);
public partial class DirectIntersectionStepSearcher					(): StepSearcher(02, 0);
public partial class DirectSubsetStepSearcher						(): StepSearcher(03, 0);
public partial class ComplexSingleStepSearcher						(): StepSearcher(04, 1);
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
public partial class BivalueOddagonStepSearcher						(): StepSearcher(28, 2);
public partial class ReverseBivalueUniversalGraveStepSearcher		(): StepSearcher(29, 2);
public partial class EmptyRectangleIntersectionPairStepSearcher		(): StepSearcher(30, 1);
public partial class FireworkStepSearcher							(): StepSearcher(31, 1);
public partial class AntiGurthSymmetricalPlacementStepSearcher		(): StepSearcher(32, 0);
public partial class XyzRingStepSearcher							(): StepSearcher(33, 1);
public partial class UniquenessClueCoverStepSearcher				(): StepSearcher(34, 2);
public partial class ChainStepSearcher								(): StepSearcher(35, 2);
public partial class GroupedChainStepSearcher						(): StepSearcher(36, 2);
public partial class DominoChainStepSearcher						(): StepSearcher(37, 3);
public partial class AlignedExclusionStepSearcher					(): StepSearcher(38, 2);
public partial class GuardianStepSearcher							(): StepSearcher(39, 2);
public partial class ComplexFishStepSearcher						(): StepSearcher(40, 2);
public partial class ChromaticPatternStepSearcher					(): StepSearcher(41, 2);
public partial class BlossomLoopStepSearcher						(): StepSearcher(42, 3);
public partial class FinnedChainStepSearcher						(): StepSearcher(43, 2);
public partial class GroupedFinnedChainStepSearcher					(): StepSearcher(44, 2);
public partial class FinnedRectangleChainStepSearcher				(): StepSearcher(45, 2);
public partial class MultipleForcingChainsStepSearcher				(): StepSearcher(46, 2);
public partial class RectangleForcingChainsStepSearcher				(): StepSearcher(47, 2);
public partial class BivalueUniversalGraveForcingChainsStepSearcher	(): StepSearcher(48, 2);
public partial class DeathBlossomStepSearcher						(): StepSearcher(49, 2);
public partial class WhipStepSearcher								(): StepSearcher(50, 3);
public partial class DynamicForcingChainsStepSearcher				(): StepSearcher(51, 3);
public partial class BowmanBingoStepSearcher						(): StepSearcher(52, 3, StepSearcherRunningArea.None);
public partial class PatternOverlayStepSearcher						(): StepSearcher(53, 3, StepSearcherRunningArea.Collecting);
public partial class ExocetStepSearcher								(): StepSearcher(54, 3);
public partial class DominoLoopStepSearcher							(): StepSearcher(55, 3);
public partial class MultisectorLockedSetsStepSearcher				(): StepSearcher(56, 3);
public partial class BruteForceStepSearcher							(): StepSearcher(57, 4, StepSearcherRunningArea.Searching);
