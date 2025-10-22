"""
top_cryptos.py

Fetch top cryptocurrencies by market cap (excluding stablecoins), then map them
to clean USD spot trading pairs from CoinAPI. Prefers reputable venues.
"""

import os
import requests
from typing import List, Dict

# --- Config ---
TOP_CRYPTO_LIMIT = 40
COINGECKO_URL = "https://api.coingecko.com/api/v3/coins/markets"
COINAPI_SYMBOLS_URL = "https://rest.coinapi.io/v1/symbols"

# Load API key from environment
COINAPI_KEY = os.getenv("COIN_API_KEY")
if not COINAPI_KEY:
    raise RuntimeError("Missing environment variable COIN_API_KEY")

COINAPI_HEADERS = {"X-CoinAPI-Key": COINAPI_KEY}

# Stablecoins to exclude as bases
STABLE_BASES = {
    "USDT","USDC","DAI","BUSD","TUSD","USDP","GUSD","FRAX","LUSD","USDD",
    "FDUSD","PYUSD","USDE","UST","USTC","SUSD","USDN","USDL"
}

# Non-spot instrument types to exclude
BAD_TYPES = {"PERPETUAL","FUTURES","OPTION","INDEX","FORWARD","SWAP","SPOT_INDEX"}

# Exchange tokens (excluded when allow_exchange_tokens=False)
EXCHANGE_TOKENS = {"BNB","LEO","BGB","WBT","CRO"}

# Venue preferences & blocks
REPUTABLE_VENUES = ["COINBASE", "KRAKEN", "BITSTAMP", "BITFINEX", "BINANCE", "GEMINI"]
BAD_VENUES = {"YOBIT"}

# Wrapped / synthetic markers
WRAPPED = {"WBTC","WETH","STETH","WSTETH"}


# --- Helpers ---

def venue_rank(symbol_id_or_exchange: str) -> int:
    sid = (symbol_id_or_exchange or "").upper()
    for i, v in enumerate(REPUTABLE_VENUES):
        if sid.startswith(v):
            return i
    return len(REPUTABLE_VENUES) + 1

def is_wrapped(base: str) -> bool:
    b = (base or "").upper()
    return b in WRAPPED or b.startswith(("W", "ST"))

def is_probable_stable(c: dict) -> bool:
    sym = (c.get("symbol") or "").upper()
    name = (c.get("name") or "").upper()
    return (sym in STABLE_BASES) or ("USD" in sym) or ("USD" in name)

def fetch_top_bases_by_mcap(limit: int) -> List[str]:
    params = {
        "vs_currency": "usd",
        "order": "market_cap_desc",
        "per_page": limit * 2,  # slight overfetch to prune stables/wrapped later
        "page": 1,
        "price_change_percentage": "24h",
    }
    r = requests.get(COINGECKO_URL, params=params, timeout=20)
    r.raise_for_status()
    coins = r.json()

    bases: List[str] = []
    for c in coins:
        sym = (c.get("symbol") or "").upper()
        if sym and not is_probable_stable(c):
            bases.append(sym)
        if len(bases) >= limit:
            break
    return bases

def fetch_coinapi_symbols() -> List[Dict]:
    r = requests.get(COINAPI_SYMBOLS_URL, headers=COINAPI_HEADERS, timeout=30)
    r.raise_for_status()
    return r.json()

def is_spot_usd_symbol(s: Dict) -> bool:
    base  = (s.get("asset_id_base")  or "").upper()
    quote = (s.get("asset_id_quote") or "").upper()
    stype = (s.get("symbol_type")    or "").upper()
    sid   = (s.get("symbol_id")      or "").upper()
    exch  = (s.get("exchange_id")    or "").upper()

    if not base or not quote:
        return False
    if base in STABLE_BASES:
        return False
    if quote != "USD":
        return False

    # Must be SPOT (not SPOT_INDEX etc.)
    if stype:
        if stype in BAD_TYPES:
            return False
        if stype != "SPOT":
            return False
    else:
        parts = sid.split("_")
        if len(parts) < 4 or parts[1] != "SPOT":
            return False
        if "INDEX" in sid:
            return False

    # Optional early venue block (you can leave this for finalize if you prefer)
    if exch in BAD_VENUES or sid.split("_", 1)[0] in BAD_VENUES:
        return False

    return True

def resolve_usd_spot_symbols(
    top_bases_ranked: List[str],
    coinapi_symbols: List[Dict],
    limit: int,
    allow_wrapped: bool,
    allow_exchange_tokens: bool
) -> Dict[str, str]:
    # Collect candidates per base
    candidates: Dict[str, List[Dict]] = {}
    for s in coinapi_symbols:
        if not is_spot_usd_symbol(s):
            continue
        base  = (s.get("asset_id_base") or "").upper()
        exch  = (s.get("exchange_id")  or s.get("symbol_id","")).upper()
        candidates.setdefault(base, []).append({
            "symbol_id": s["symbol_id"],
            "venue": exch
        })

    def score(base: str, venue: str) -> tuple:
        wrapped_penalty = 1 if (not allow_wrapped and is_wrapped(base)) else 0
        exch_penalty = venue_rank(venue)
        return (wrapped_penalty, exch_penalty)  # lower is better

    resolved: Dict[str, str] = {}
    for base in top_bases_ranked:
        b = base.upper()
        if b in STABLE_BASES:
            continue
        if not allow_exchange_tokens and b in EXCHANGE_TOKENS:
            continue  # <-- removed the old "and b != 'BNB'" exception
        opts = candidates.get(b, [])
        if not opts:
            continue
        best = min(opts, key=lambda o: score(b, o["venue"]))
        resolved[b] = best["symbol_id"]
        if len(resolved) == limit:
            break
    return resolved

def finalize_top_map(top_map: Dict[str, str],
                     allow_exchange_tokens: bool = False,
                     allow_wrapped: bool = False,
                     allow_bad_venues: bool = False) -> Dict[str, str]:
    out: Dict[str, str] = {}
    for base, sym in top_map.items():
        exch = sym.split("_", 1)[0].upper()
        if not allow_exchange_tokens and base.upper() in EXCHANGE_TOKENS:
            continue
        if not allow_wrapped and base.upper() in WRAPPED:
            continue
        if not allow_bad_venues and (exch in BAD_VENUES):
            continue
        out[base] = sym
    return out


# --- Public API ---

def get_top_cryptos(limit: int = TOP_CRYPTO_LIMIT,
                    allow_wrapped: bool = False,
                    allow_exchange_tokens: bool = False,
                    allow_bad_venues: bool = False) -> Dict[str, str]:
    """
    Returns dict {base: coinapi_symbol_id} for top cryptos by market cap.
    Excludes stablecoins. Prefers USD spot pairs on reputable venues.
    """
    top_bases = fetch_top_bases_by_mcap(limit=limit)
    symbols = fetch_coinapi_symbols()
    usd_symbols = resolve_usd_spot_symbols(
        top_bases, symbols, limit=limit,
        allow_wrapped=allow_wrapped,
        allow_exchange_tokens=allow_exchange_tokens
    )
    return finalize_top_map(
        usd_symbols,
        allow_exchange_tokens=allow_exchange_tokens,
        allow_wrapped=allow_wrapped,
        allow_bad_venues=allow_bad_venues
    )
