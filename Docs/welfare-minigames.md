# Welfare minigame integration

MainGame installs WelfareInteractionController through InvestigationFlowController. It intercepts the existing children A/B/C and reuses their dialogue after information is unlocked. The RPS UI is extracted from MiniGame(1) into Resources/WelfareRockPaperScissors.prefab; no second scene, camera or EventSystem is loaded.

Each child receives a shuffled fixed assignment: RPS, rope, and one random game. RPS has three decisive rounds, with draws replayed. Closing between rounds preserves progress. Completed matches cannot repeat. Winning unlocks dialogue; losing enables giving one collected candy instead. Three candy pickups use inventory item IDs 9100?9102. Child state, rounds and candy state are stored in welfare_progress.json inside each manual save slot. New games clear the working state; old slots without this optional file initialize fresh encounters.

## Rope adapter

After initializing the rope minigame, register:

```csharp
WelfareInteractionController.Instance.RopeLauncher = finished =>
{
    // Open rope UI. Close that UI when the game finishes.
    // Call finished(true) for a win or finished(false) for a loss exactly once.
};
```

The provider must allow the game to reach a result and clean up its own UI before invoking the callback. Do not clear the registration until the provider is destroyed. Until a provider is registered, rope encounters remain unavailable and do not consume an attempt. Do not substitute RPS for rope, as that would violate the mixed-game guarantee.

## Manual verification pending

- New game: menu/opening does not expose candy; investigation reveals each pickup at home, park and staff room.
- Children cannot disclose information through the inspect handler before unlocking it.
- RPS win/loss/draw, return between rounds, no repeated result, no fourth round.
- Losing enables candy; giving one consumes exactly one item and unlocks only that child.
- Save, reload, return to each child: assignment, rounds, outcome, candy pickup and consumption persist.
- Switch Chinese/English and inspect candy; verify pickup positions visually.

Code compilation and extracted prefab local-reference checks passed. No Unity playtest was performed.
