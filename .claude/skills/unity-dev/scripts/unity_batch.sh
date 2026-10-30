#!/usr/bin/env bash
# Run Unity in batchmode against this project and summarise the result.
#   unity_batch.sh                      -> import + compile only
#   unity_batch.sh Some.Class.Method    -> also run a static editor method (it must call EditorApplication.Exit)
# Env: UNITY_VERSION (default 6000.5.6f1), LOG (default: scratch file under $TMPDIR)
set -uo pipefail

project="$(cd "$(dirname "$0")/../../../.." && pwd)"
version="${UNITY_VERSION:-6000.5.6f1}"
unity="/Applications/Unity/Hub/Editor/$version/Unity.app/Contents/MacOS/Unity"
log="${LOG:-${TMPDIR:-/tmp}/unity_batch_$(date +%s).log}"

if [[ ! -x "$unity" ]]; then
  echo "Unity $version not found at $unity" >&2
  exit 2
fi

if pgrep -f "Unity.app/Contents/MacOS/Unity -projectpath $project" >/dev/null; then
  echo "The Unity editor has this project open; close it first (batchmode can't share a project)." >&2
  exit 3
fi

args=(-batchmode -nographics -projectPath "$project" -logFile "$log")
if [[ $# -ge 1 ]]; then
  args+=(-executeMethod "$1")
else
  args+=(-quit)
fi

"$unity" "${args[@]}"
code=$?

echo "exit=$code log=$log"
grep -E "error CS|warning CS" "$log" | sort -u
grep -E "[A-Za-z]+Exception: |Scripts have compiler errors|\] Failed" "$log" | sort -u | head -20
exit $code
