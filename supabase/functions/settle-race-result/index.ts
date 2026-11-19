import "jsr:@supabase/functions-js/edge-runtime.d.ts";
import { createClient } from "@supabase/supabase-js";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, apikey, x-client-info, content-type",
  "Access-Control-Allow-Methods": "POST, OPTIONS",
};

function jsonResponse(status: number, payload: Record<string, unknown>): Response {
  return new Response(JSON.stringify(payload), {
    status,
    headers: {
      ...corsHeaders,
      "Content-Type": "application/json",
      "Cache-Control": "no-store",
    },
  });
}

Deno.serve(async (request: Request) => {
  if (request.method === "OPTIONS") {
    return new Response("ok", { headers: corsHeaders });
  }

  if (request.method !== "POST") {
    return jsonResponse(405, { error: "Method not allowed" });
  }

  const authorization = request.headers.get("Authorization");
  if (!authorization || !authorization.startsWith("Bearer ")) {
    return jsonResponse(401, { error: "Authentication required" });
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

  // Accept only a result identifier. Payout amount/currency/user identity are
  // deliberately not accepted from the player client.
  if (
    Object.keys(body).some((key) => key !== "race_result_id") ||
    typeof body.race_result_id !== "string" ||
    !/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(body.race_result_id)
  ) {
    return jsonResponse(400, { error: "A valid race_result_id is required" });
  }

  const supabaseUrl = Deno.env.get("SUPABASE_URL");
  const anonKey = Deno.env.get("SUPABASE_ANON_KEY") ?? Deno.env.get("SUPABASE_PUBLISHABLE_KEY");
  const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY");

  if (!supabaseUrl || !anonKey || !serviceRoleKey) {
    console.error("Required Supabase Edge Function environment variables are missing");
    return jsonResponse(503, { error: "Settlement service is not configured" });
  }

  // Verify the caller's JWT with Auth. The request body never chooses the player.
  const userClient = createClient(supabaseUrl, anonKey, {
    auth: { persistSession: false, autoRefreshToken: false },
  });
  const jwt = authorization.slice("Bearer ".length);
  const { data: userData, error: authError } = await userClient.auth.getUser(jwt);

  if (authError || !userData.user) {
    return jsonResponse(401, { error: "Invalid or expired session" });
  }

  // This privileged client is kept inside the Edge Function only.
  const admin = createClient(supabaseUrl, serviceRoleKey, {
    auth: { persistSession: false, autoRefreshToken: false },
  });

  const { data: raceResult, error: resultError } = await admin
    .from("apex_race_results")
    .select("id,user_id,session_id,placement,validated,reward_credits")
    .eq("id", body.race_result_id)
    .eq("user_id", userData.user.id)
    .maybeSingle();

  if (resultError) {
    console.error("Race result lookup failed:", resultError.message);
    return jsonResponse(500, { error: "Could not verify race result" });
  }
  if (!raceResult) {
    return jsonResponse(404, { error: "Race result not found" });
  }
  if (raceResult.validated !== true) {
    return jsonResponse(409, { error: "Race result has not been validated by the race server" });
  }

  const rewardCredits = Number(raceResult.reward_credits);
  if (!Number.isSafeInteger(rewardCredits) || rewardCredits < 1 || rewardCredits > 1_000_000_000) {
    return jsonResponse(409, { error: "No valid server-approved reward is available" });
  }

  const { data: session, error: sessionError } = await admin
    .from("apex_race_sessions")
    .select("id,region_id,discipline,status")
    .eq("id", raceResult.session_id)
    .maybeSingle();

  if (sessionError) {
    console.error("Race session lookup failed:", sessionError.message);
    return jsonResponse(500, { error: "Could not verify race session" });
  }
  if (!session || session.status !== "finished") {
    return jsonResponse(409, { error: "Race session is not finished" });
  }

  // The stable per-result key makes repeated requests safe. The SQL function
  // serializes wallet updates and returns applied=false for a duplicate.
  const idempotencyKey = `race-result:${raceResult.id}:credits`;
  const { data: settlementRows, error: settlementError } = await admin.rpc(
    "apex_server_apply_wallet_delta",
    {
      p_user_id: userData.user.id,
      p_currency: "credits",
      p_amount: rewardCredits,
      p_reason: "race_reward",
      p_idempotency_key: idempotencyKey,
      p_metadata: {
        source: "validated_race_result",
        race_result_id: raceResult.id,
        session_id: raceResult.session_id,
        placement: raceResult.placement,
        discipline: session.discipline,
        region_id: session.region_id,
      },
    },
  );

  if (settlementError) {
    console.error("Wallet settlement failed:", settlementError.message);
    return jsonResponse(500, { error: "Wallet settlement failed" });
  }

  const settlement = Array.isArray(settlementRows) ? settlementRows[0] : settlementRows;
  if (!settlement) {
    return jsonResponse(500, { error: "Wallet settlement returned no result" });
  }

  return jsonResponse(200, {
    ok: true,
    applied: settlement.applied === true,
    reward_credits: rewardCredits,
    credits_balance: settlement.credits,
    premium_tokens_balance: settlement.premium_tokens,
    race_result_id: raceResult.id,
  });
});
