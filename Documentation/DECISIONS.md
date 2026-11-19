# Architecture Decision Records

## ADR-001 — Mobile-first
Decision: Android is the first test target, while code and content pipelines remain iOS-compatible.
Reason: validate performance and touch interaction early.

## ADR-002 — Multi-region world
Decision: several connected regions rather than one monolithic map.
Reason: enable staged content delivery, bounded simulation and independent regional capacity.

## ADR-003 — Dual camera
Decision: first-person and third-person cameras are supported by the same simulation.
Reason: player choice without duplicating vehicle physics.

## ADR-004 — Persistent world with instanced competitive sessions
Decision: social/RP world state persists, while competitive races may use dedicated session instances.
Reason: keep open-world continuity without letting high-speed race simulation overload the social region.

## ADR-005 — Server-authoritative economy and outcomes
Decision: clients never directly set balances, ownership, rewards or race results.
Reason: reduce cheating, duplication and inconsistent state.
