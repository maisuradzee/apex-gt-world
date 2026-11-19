# Network Protocol Contract (design + persistence boundary)

## Input message
- protocol_version
- session_id
- entity_id
- monotonic input_sequence
- client_tick
- steering [-1,1], throttle [0,1], brake [0,1], handbrake [0,1]
- requested gear action

## Authoritative snapshot
- protocol_version, server_tick, entity_id
- last acknowledged input_sequence
- validated transform and velocity
- vehicle state, damage and race flags

## Validation
- Reject malformed, stale, replayed, out-of-range and implausible inputs.
- Server clamps input and validates movement envelopes.
- Never trust client-provided rewards, balances, positions, ownership or finishing positions.
- Separate render frame rate, physics tick and network send rate.
- Version protocol messages and keep controlled compatibility windows.

## Race lifecycle persistence contract
1. A trusted race server creates a session in `created` state.
2. It calls service-only `apex_server_start_race_session(session_id)` to transition the session to `running`.
3. When authoritative simulation ends, it sends a complete result array to `apex_server_finalize_race_session(session_id, results_json)`. Each player item contains `user_id`, `placement`, optional `finish_time_ms`, `score` and server-calculated `reward_credits`.
4. The database validates the set, inserts `validated=true` results and marks the session `finished` atomically. An identical retry is idempotent; conflicting retries are rejected.
5. A player may then claim their own result through the JWT-protected `settle-race-result` Edge Function. The client sends only `race_result_id`; the function reads the server-owned payout and invokes the idempotent wallet transaction.

All lifecycle RPCs are restricted to trusted `service_role` access. The dedicated race-server process that performs simulation and calls the RPCs is still to be implemented.
