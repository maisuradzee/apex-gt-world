import "jsr:@supabase/functions-js/edge-runtime.d.ts";
import { createClient } from "@supabase/supabase-js";

const headers = {
  "Content-Type": "application/json",
  "Cache-Control": "no-store",
};

function jsonResponse(status: number, payload: Record<string, unknown>): Response {
  return new Response(JSON.stringify(payload), { status, headers });
}

function isUuid(value: unknown): value is string {
  return typeof value === "string" &&
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
}

async function sameSecret(candidate: string, expected: string): Promise<boolean> {
  const encoder = new TextEncoder();
  const [leftDigest, rightDigest] = await Promise.all([
    crypto.subtle.digest("SHA-256", encoder.encode(candidate)),
    crypto.subtle.digest("SHA-256", encoder.encode(expected)),
  ]);
  const left = new Uint8Array(leftDigest);
  const right = new Uint8Array(rightDigest);
  let difference = 0;
  for (let i = 0; i < left.length; i++) difference |= left[i] ^ right[i];
  return difference === 0;
}

function hasOnlyKeys(body: Record<string, unknown>, allowed: string[]): boolean {
  return Object.keys(body).every((key) => allowed.includes(key));
}

Deno.serve(async (request: Request) => {
  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204 });
  }
  if (request.method !== "POST") {
    return jsonResponse(405, { error: "Method not allowed" });
  }

  const configuredToken = Deno.env.get("APEX_RACE_SERVER_TOKEN");
  const suppliedToken = request.headers.get("x-apex-race-server-token");
  if (!configuredToken || configuredToken.length < 32) {
    console.error("APEX_RACE_SERVER_TOKEN is missing or too short");
    return jsonResponse(503, { error: "Race control is not configured" });
  }
  if (!suppliedToken || !(await sameSecret(suppliedToken, configuredToken))) {
    return jsonResponse(401, { error: "Unauthorized race server" });
  }

  let body: Record<string, unknown>;
  try {
    const parsed = await request.json();
    if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
      return jsonResponse(400, { error: "Invalid request body" });
    }
    body = parsed as Record<string, unknown>;
  } catch {
    return jsonResponse(400, { error: "Invalid JSON body" });
  }

  const supabaseUrl = Deno.env.get("SUPABASE_URL");
  const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");
  if (!supabaseUrl || !serviceRoleKey) {
    console.error("Supabase service configuration is missing");
    return jsonResponse(503, { error: "Race control is not configured" });
  }

  // The service-role client exists only inside this server-side function.
  const admin = createClient(supabaseUrl, serviceRoleKey, {
    auth: { persistSession: false, autoRefreshToken: false },
  });

  if (body.operation === "create") {
    if (
      !hasOnlyKeys(body, ["operation", "region_id", "discipline"]) ||
      typeof body.region_id !== "string" ||
      body.region_id.length < 1 ||
      body.region_id.length > 80 ||
      typeof body.discipline !== "string" ||
      !["circuit", "time_attack", "drift", "endurance", "motorcycle"].includes(body.discipline)
    ) {
      return jsonResponse(400, { error: "Create requires a valid region_id and discipline" });
    }

    const { data, error } = await admin
      .from("apex_race_sessions")
      .insert({
        region_id: body.region_id,
        discipline: body.discipline,
        status: "created",
      })
      .select("id,region_id,discipline,status,created_at")
      .single();

    if (error) {
      console.error("Race session creation failed:", error.message);
      return jsonResponse(409, { error: "Race session could not be created" });
    }
    return jsonResponse(201, { ok: true, operation: "create", session: data });
  }

  if (!isUuid(body.session_id)) {
    return jsonResponse(400, { error: "A valid session_id is required" });
  }

  if (body.operation === "start") {
    if (!hasOnlyKeys(body, ["operation", "session_id"])) {
      return jsonResponse(400, { error: "Start operation contains unsupported fields" });
    }

    const { data, error } = await admin.rpc("apex_server_start_race_session", {
      p_session_id: body.session_id,
    });
    if (error) {
      console.error("Race start RPC failed:", error.message);
      return jsonResponse(409, { error: "Race session could not be started" });
    }
    const result = Array.isArray(data) ? data[0] : data;
    return jsonResponse(200, { ok: true, operation: "start", result });
  }

  if (body.operation === "finalize") {
    if (
      !hasOnlyKeys(body, ["operation", "session_id", "results"]) ||
      !Array.isArray(body.results) ||
      body.results.length < 1 ||
      body.results.length > 64
    ) {
      return jsonResponse(400, { error: "Finalize requires a results array containing 1–64 players" });
    }

    const { data, error } = await admin.rpc("apex_server_finalize_race_session", {
      p_session_id: body.session_id,
      p_results: body.results,
    });
    if (error) {
      console.error("Race finalize RPC failed:", error.message);
      return jsonResponse(409, { error: "Race results could not be finalized" });
    }
    return jsonResponse(200, { ok: true, operation: "finalize", results: data });
  }

  return jsonResponse(400, { error: "Unsupported race operation" });
});
