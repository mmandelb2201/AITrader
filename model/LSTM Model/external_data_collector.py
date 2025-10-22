"""
External Data Collector for Crypto Prediction Model
Collects non-price/volume features that can provide real alpha
"""

import pandas as pd
import numpy as np
import requests
import time
from typing import Dict, List, Optional
import json

class ExternalDataCollector:
    """Collects external data sources for crypto prediction"""
    
    def __init__(self):
        self.api_keys = {
            'glassnode': None,  # Get from glassnode.com
            'messari': None,    # Get from messari.io
            'santiment': None,  # Get from santiment.net
            'fred': None,       # Get from fred.stlouisfed.org
        }
    
    def get_onchain_data(self, asset: str, start_date: str, end_date: str) -> pd.DataFrame:
        """
        Get on-chain metrics from Glassnode API
        Free tier available with rate limits
        """
        if not self.api_keys['glassnode']:
            print("⚠️  Set Glassnode API key for on-chain data")
            return pd.DataFrame()
        
        metrics = [
            'addresses_active_count',
            'transactions_count', 
            'transactions_volume_sum',
            'addresses_new_non_zero_count',
            'supply_current',
            'nvt',
            'mvrv',
        ]
        
        data = {}
        base_url = "https://api.glassnode.com/v1/metrics"
        
        for metric in metrics:
            try:
                url = f"{base_url}/{metric}"
                params = {
                    'a': asset.upper(),
                    's': start_date,
                    'u': end_date,
                    'f': 'json',
                    'api_key': self.api_keys['glassnode']
                }
                response = requests.get(url, params=params)
                if response.status_code == 200:
                    df = pd.DataFrame(response.json())
                    df['timestamp'] = pd.to_datetime(df['t'], unit='s')
                    data[metric] = df[['timestamp', 'v']].rename(columns={'v': metric})
                time.sleep(0.1)  # Rate limiting
            except Exception as e:
                print(f"Error fetching {metric}: {e}")
        
        # Merge all metrics
        if data:
            result = list(data.values())[0]
            for df in list(data.values())[1:]:
                result = result.merge(df, on='timestamp', how='outer')
            return result
        return pd.DataFrame()
    
    def get_sentiment_data(self, assets: List[str], start_date: str, end_date: str) -> pd.DataFrame:
        """
        Get sentiment data from various sources
        """
        # Fear & Greed Index (free API)
        try:
            url = "https://api.alternative.me/fng/"
            params = {'limit': 365}  # Get historical data
            response = requests.get(url, params=params)
            
            if response.status_code == 200:
                fg_data = response.json()['data']
                fg_df = pd.DataFrame(fg_data)
                fg_df['timestamp'] = pd.to_datetime(fg_df['timestamp'], unit='s')
                fg_df['fear_greed_index'] = fg_df['value'].astype(float)
                fg_df = fg_df[['timestamp', 'fear_greed_index']]
                
                # Add sentiment categories
                fg_df['sentiment_category'] = pd.cut(
                    fg_df['fear_greed_index'], 
                    bins=[0, 25, 45, 55, 75, 100],
                    labels=['extreme_fear', 'fear', 'neutral', 'greed', 'extreme_greed']
                )
                return fg_df
        except Exception as e:
            print(f"Error fetching Fear & Greed Index: {e}")
        
        return pd.DataFrame()
    
    def get_macro_data(self, start_date: str, end_date: str) -> pd.DataFrame:
        """
        Get macro economic data from various free sources
        """
        macro_data = {}
        
        # VIX from Yahoo Finance (free)
        try:
            import yfinance as yf
            
            tickers = {
                '^VIX': 'vix_level',
                'DX-Y.NYB': 'dxy_price', 
                'GC=F': 'gold_price',
                '^GSPC': 'sp500_price',
                '^TNX': 'us_10y_yield'
            }
            
            for ticker, name in tickers.items():
                try:
                    data = yf.download(ticker, start=start_date, end=end_date, interval='1h')
                    if not data.empty:
                        df = data[['Close']].reset_index()
                        df.columns = ['timestamp', name]
                        macro_data[name] = df
                        time.sleep(0.1)
                except Exception as e:
                    print(f"Error fetching {ticker}: {e}")
            
            # Merge all macro data
            if macro_data:
                result = list(macro_data.values())[0]
                for df in list(macro_data.values())[1:]:
                    result = result.merge(df, on='timestamp', how='outer')
                return result
                
        except ImportError:
            print("Install yfinance: pip install yfinance")
        except Exception as e:
            print(f"Error fetching macro data: {e}")
        
        return pd.DataFrame()
    
    def get_orderbook_features(self, asset: str) -> Dict:
        """
        Calculate order book features from exchange API
        Example using Binance public API
        """
        try:
            url = f"https://api.binance.com/api/v3/depth"
            params = {'symbol': f'{asset}USDT', 'limit': 100}
            response = requests.get(url, params=params)
            
            if response.status_code == 200:
                data = response.json()
                
                bids = np.array([[float(x[0]), float(x[1])] for x in data['bids']])
                asks = np.array([[float(x[0]), float(x[1])] for x in data['asks']])
                
                if len(bids) > 0 and len(asks) > 0:
                    mid_price = (bids[0][0] + asks[0][0]) / 2
                    spread = asks[0][0] - bids[0][0]
                    
                    # Order book imbalance
                    bid_volume = np.sum(bids[:10, 1])  # Top 10 levels
                    ask_volume = np.sum(asks[:10, 1])
                    imbalance = (bid_volume - ask_volume) / (bid_volume + ask_volume)
                    
                    # Depth within 1% of mid price
                    depth_1pct = mid_price * 0.01
                    bid_depth = np.sum(bids[bids[:, 0] >= mid_price - depth_1pct, 1])
                    ask_depth = np.sum(asks[asks[:, 0] <= mid_price + depth_1pct, 1])
                    
                    return {
                        'bid_ask_spread': spread / mid_price,
                        'order_book_imbalance': imbalance,
                        'order_book_depth_1pct': bid_depth + ask_depth,
                        'mid_price': mid_price
                    }
        except Exception as e:
            print(f"Error fetching order book for {asset}: {e}")
        
        return {}

# Usage example
def collect_external_features(assets: List[str], start_date: str, end_date: str) -> pd.DataFrame:
    """
    Main function to collect all external features
    """
    collector = ExternalDataCollector()
    
    # Set your API keys here
    # collector.api_keys['glassnode'] = 'your_api_key'
    
    all_data = []
    
    # 1. Get sentiment data (works without API key)
    print("📊 Collecting sentiment data...")
    sentiment_df = collector.get_sentiment_data(assets, start_date, end_date)
    if not sentiment_df.empty:
        all_data.append(sentiment_df)
    
    # 2. Get macro data (works without API key)
    print("📈 Collecting macro economic data...")
    macro_df = collector.get_macro_data(start_date, end_date)
    if not macro_df.empty:
        all_data.append(macro_df)
    
    # 3. Get on-chain data (requires API key)
    for asset in assets:
        print(f"⛓️  Collecting on-chain data for {asset}...")
        onchain_df = collector.get_onchain_data(asset, start_date, end_date)
        if not onchain_df.empty:
            # Add asset prefix to columns
            for col in onchain_df.columns:
                if col != 'timestamp':
                    onchain_df = onchain_df.rename(columns={col: f"{asset}_{col}"})
            all_data.append(onchain_df)
    
    # Merge all external data
    if all_data:
        result = all_data[0]
        for df in all_data[1:]:
            result = result.merge(df, on='timestamp', how='outer')
        
        # Forward fill missing values
        result = result.sort_values('timestamp').fillna(method='ffill')
        return result
    
    return pd.DataFrame()

if __name__ == "__main__":
    # Example usage
    assets = ['BTC', 'ETH']
    start = '2023-01-01'
    end = '2024-01-01'
    
    external_data = collect_external_features(assets, start, end)
    print(f"Collected {len(external_data)} rows with {len(external_data.columns)} features")
    print(external_data.head())