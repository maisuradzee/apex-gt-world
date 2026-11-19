# Network Protocol Contract (design only)

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
