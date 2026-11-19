# Database Plan (not yet applied)

Use schema migrations and keep privileged tables/functions outside exposed schemas where possible.

## Core tables planned
- profiles
- player_settings
- regions
- world_cells
- player_world_state
- region_transfer_events
- vehicle_catalog
- player_vehicles
- vehicle_upgrades
- motorcycle_catalog
- player_motorcycles
- motorcycle_upgrades
- garages
- properties
- player_properties
- wallets
- wallet_transactions
- inventory_items
- player_inventory
- professions
- player_jobs
- businesses
- race_championships
- race_sessions
- race_results
- player_progress
- clubs
- club_members
- friends
- moderation_actions
- audit_events

## Security rules
- Enable RLS on every exposed table.
- Use ownership predicates, not merely `TO authenticated`.
- Keep wallet and reward writes behind server-trusted operations.
- Never use user-editable profile metadata for authorization.
- Never put service-role credentials in the client.
- Make wallet operations idempotent with unique transaction keys.
- Record immutable ledger entries and derive balances transactionally.
- Review views for `security_invoker` behavior and inspect security/performance advisors.

No SQL has been applied to a live Supabase project. This file is a schema design checklist only.
