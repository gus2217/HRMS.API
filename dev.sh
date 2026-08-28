#!/bin/bash
# ============================================================
# Jacana HRMS — one-command dev stack.
# Works with EITHER Docker infra OR a locally-running Postgres.
# Every step is verified before the next runs, so a missing piece
# fails loudly instead of producing a silent 502 from the Vite proxy.
#
# Usage:
#   ./dev.sh            # infra + API + UI
#   ./dev.sh --api-only # infra + API (run the UI yourself)
# ============================================================
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_ONLY=0
[ "${1:-}" = "--api-only" ] && API_ONLY=1

say()  { printf '\n\033[1;34m==> %s\033[0m\n' "$*"; }
ok()   { printf '\033[1;32m    ✓ %s\033[0m\n' "$*"; }
fail() { printf '\033[1;31m    ✗ %s\033[0m\n' "$*"; exit 1; }

DB_HOST="${DB_HOST:-localhost}"
DB_PORT="${DB_PORT:-5432}"
DB_NAME="${DB_NAME:-jacana_hrms}"
DB_USER="${DB_USER:-jacana}"
DB_PASS="${DB_PASS:-jacana}"

pg_ready() {
  # Best-effort Postgres reachability check (psql if present, else TCP).
  if command -v pg_isready >/dev/null 2>&1; then
    PGPASSWORD="$DB_PASS" pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" >/dev/null 2>&1
  else
    (exec 3<>"/dev/tcp/$DB_HOST/$DB_PORT") 2>/dev/null && exec 3>&- 3<&-
  fi
}

# ── 1. Infrastructure (Postgres + Redis) — Docker OR local ──────────────
say "Checking database ($DB_HOST:$DB_PORT/$DB_NAME)"
if pg_ready; then
  ok "Postgres already reachable at $DB_HOST:$DB_PORT — skipping Docker"
else
  if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
    say "Starting Postgres + Redis via Docker"
    docker compose up -d postgres redis
    for i in $(seq 1 20); do
      pg_ready && { ok "Postgres ready"; break; }
      [ "$i" = 20 ] && fail "Postgres did not become ready"
      sleep 2
    done
  else
    fail "No Postgres reachable at $DB_HOST:$DB_PORT and Docker is not available.
    Start your local Postgres (must have DB '$DB_NAME', user '$DB_USER') or
    set DB_HOST/DB_PORT/DB_NAME/DB_USER/DB_PASS, then re-run."
  fi
fi

# ── 2. Seed (idempotent — safe to run every time) ───────────────────────
say "Seeding database (migrations + users)"
dotnet run --project "$ROOT/tools/Jacana.HRMS.DbInitializer" >/dev/null 2>&1 \
  || fail "DbInitializer failed — check Postgres credentials (DB_HOST=$DB_HOST DB_PORT=$DB_PORT DB_NAME=$DB_NAME)"
ok "Database seeded (admin@stfrancis.local / ChangeMe123!)"

# ── 3. API on :5099 ─────────────────────────────────────────────────────
say "Starting API on http://localhost:5099"
if curl -s -o /dev/null --max-time 2 http://localhost:5099/health; then
  ok "API already running on :5099"
else
  (cd "$ROOT" && nohup dotnet run --project src/Api/Jacana.HRMS.Api \
      > /tmp/jacana-api-dev.log 2>&1 &)
  for i in $(seq 1 25); do
    if curl -s -o /dev/null --max-time 2 http://localhost:5099/health; then
      ok "API healthy on :5099"
      break
    fi
    [ "$i" = 25 ] && {
      echo "    API log (tail):"
      tail -25 /tmp/jacana-api-dev.log
      fail "API failed to start — see log above. This is why the UI proxy 502s."
    }
    sleep 2
  done
fi

# ── 4. UI (optional) ────────────────────────────────────────────────────
if [ "$API_ONLY" = 0 ]; then
  say "Starting UI on http://localhost:5173"
  UI_DIR=""
  [ -d "$ROOT/../jacana-ui" ] && UI_DIR="$ROOT/../jacana-ui"
  [ -z "$UI_DIR" ] && [ -d "$ROOT/../HRMS.UI" ] && UI_DIR="$ROOT/../HRMS.UI"
  [ -z "$UI_DIR" ] && fail "Frontend repo not found next to this one (expected ../jacana-ui or ../HRMS.UI)"
  if curl -s -o /dev/null --max-time 2 http://localhost:5173/; then
    ok "UI already running on :5173"
  else
    (cd "$UI_DIR" && nohup npm run dev > /tmp/jacana-ui-dev.log 2>&1 &)
    for i in $(seq 1 15); do
      curl -s -o /dev/null --max-time 2 http://localhost:5173/ && { ok "UI ready on :5173"; break; }
      [ "$i" = 15 ] && fail "UI failed to start — see /tmp/jacana-ui-dev.log"
      sleep 1
    done
  fi
fi

say "Done."
echo ""
echo "  API:  http://localhost:5099/health"
echo "  Login: admin@stfrancis.local / ChangeMe123!"
[ "$API_ONLY" = 0 ] && echo "  UI:   http://localhost:5173"
