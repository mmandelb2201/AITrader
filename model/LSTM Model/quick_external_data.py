"""
Quick Start: Getting Free External Data for Crypto Prediction
This script shows you how to get meaningful external features without API keys
"""

import pandas as pd
import numpy as np
import requests
from datetime import datetime, timedelta

def get_fear_greed_index(days_back=365):
    """
    Get Fear & Greed Index - completely free, no API key needed
    This is a powerful sentiment indicator for crypto markets
    """
    try:
        url = "https://api.alternative.me/fng/"
        params = {'limit': days_back}
        response = requests.get(url, params=params)
        
        if response.status_code == 200:
            data = response.json()['data']
            df = pd.DataFrame(data)
            df['timestamp'] = pd.to_datetime(df['timestamp'], unit='s')
            df['fear_greed_index'] = df['value'].astype(float)
            
            # Create categorical features
            df['extreme_fear'] = (df['fear_greed_index'] < 25).astype(int)
            df['fear'] = ((df['fear_greed_index'] >= 25) & (df['fear_greed_index'] < 45)).astype(int)
            df['greed'] = ((df['fear_greed_index'] >= 55) & (df['fear_greed_index'] < 75)).astype(int)
            df['extreme_greed'] = (df['fear_greed_index'] >= 75).astype(int)
            
            return df[['timestamp', 'fear_greed_index', 'extreme_fear', 'fear', 'greed', 'extreme_greed']]
    except Exception as e:
        print(f"Error fetching Fear & Greed Index: {e}")
    return pd.DataFrame()

def get_coinmetrics_data(asset='btc', metrics=['AdrActCnt'], start_date='2023-01-01'):
    """
    Get basic on-chain metrics from CoinMetrics (free community tier)
    Available metrics: AdrActCnt, TxCnt, TxTfrValAdjUSD, etc.
    """
    try:
        base_url = "https://community-api.coinmetrics.io/v4/timeseries/asset-metrics"
        params = {
            'assets': asset,
            'metrics': ','.join(metrics),
            'start_time': start_date,
            'frequency': '1d'  # Daily data (free tier)
        }
        
        response = requests.get(base_url, params=params)
        if response.status_code == 200:
            data = response.json()['data']
            df = pd.DataFrame(data)
            df['timestamp'] = pd.to_datetime(df['time'])
            
            # Rename columns to be more descriptive
            rename_map = {
                'AdrActCnt': f'{asset}_active_addresses',
                'TxCnt': f'{asset}_transaction_count',
                'TxTfrValAdjUSD': f'{asset}_transfer_volume_usd'
            }
            df = df.rename(columns=rename_map)
            
            return df.drop(['time', 'asset'], axis=1)
    except Exception as e:
        print(f"Error fetching CoinMetrics data: {e}")
    return pd.DataFrame()

def get_simple_macro_indicators():
    """
    Get simple macro indicators using free APIs
    These often move crypto markets significantly
    """
    macro_data = []
    
    # Bitcoin Rainbow Chart (free)
    try:
        url = "https://api.blockchaincenter.net/api/rainbow/"
        response = requests.get(url)
        if response.status_code == 200:
            rainbow_data = response.json()
            rainbow_df = pd.DataFrame(rainbow_data)
            rainbow_df['timestamp'] = pd.to_datetime(rainbow_df['date'])
            rainbow_df = rainbow_df[['timestamp', 'rainbow_value']].rename(
                columns={'rainbow_value': 'btc_rainbow_indicator'}
            )
            macro_data.append(rainbow_df)
    except:
        pass
    
    # DeFi TVL (free from DefiLlama)
    try:
        url = "https://api.llama.fi/charts"
        response = requests.get(url)
        if response.status_code == 200:
            tvl_data = response.json()
            tvl_df = pd.DataFrame(tvl_data)
            tvl_df['timestamp'] = pd.to_datetime(tvl_df['date'], unit='s')
            tvl_df = tvl_df[['timestamp', 'totalLiquidityUSD']].rename(
                columns={'totalLiquidityUSD': 'defi_tvl_usd'}
            )
            macro_data.append(tvl_df)
    except:
        pass
    
    return macro_data

def create_enhanced_features(crypto_df, external_df):
    """
    Combine crypto price data with external features
    """
    # Merge on timestamp (convert to same timezone)
    crypto_df['time'] = pd.to_datetime(crypto_df['time'])
    external_df['time'] = pd.to_datetime(external_df['timestamp'])
    
    # Merge external data
    enhanced_df = crypto_df.merge(
        external_df.drop('timestamp', axis=1), 
        on='time', 
        how='left'
    )
    
    # Forward fill external data (since it might be daily while crypto is hourly)
    external_cols = [col for col in enhanced_df.columns if col not in crypto_df.columns]
    enhanced_df[external_cols] = enhanced_df[external_cols].fillna(method='ffill')
    
    # Create interaction features
    if 'fear_greed_index' in enhanced_df.columns:
        # Fear/Greed interaction with returns
        enhanced_df['fear_greed_momentum'] = enhanced_df['fear_greed_index'].pct_change(24)
        
        # Market condition based on fear/greed
        enhanced_df['market_condition'] = pd.cut(
            enhanced_df['fear_greed_index'],
            bins=[0, 20, 35, 65, 80, 100],
            labels=['panic', 'fear', 'neutral', 'greed', 'euphoria']
        )
    
    return enhanced_df

# Example usage
if __name__ == "__main__":
    print("🚀 Getting free external data for crypto prediction...")
    
    # 1. Get Fear & Greed Index (always works)
    print("📊 Fetching Fear & Greed Index...")
    fg_df = get_fear_greed_index(days_back=365)
    print(f"Got {len(fg_df)} days of sentiment data")
    
    # 2. Get basic on-chain data (free tier)
    print("⛓️  Fetching basic on-chain data...")
    onchain_df = get_coinmetrics_data('btc', ['AdrActCnt', 'TxCnt'])
    print(f"Got {len(onchain_df)} days of on-chain data")
    
    # 3. Get macro indicators
    print("📈 Fetching macro indicators...")
    macro_data_list = get_simple_macro_indicators()
    
    # Combine all external data
    external_dfs = [fg_df]
    if not onchain_df.empty:
        external_dfs.append(onchain_df)
    external_dfs.extend(macro_data_list)
    
    if len(external_dfs) > 1:
        # Merge all external data
        combined_external = external_dfs[0]
        for df in external_dfs[1:]:
            combined_external = combined_external.merge(df, on='timestamp', how='outer')
        
        print(f"✅ Combined external dataset: {combined_external.shape}")
        print("Available features:", combined_external.columns.tolist())
        
        # Save for use in your model
        combined_external.to_csv('external_features.csv', index=False)
        print("💾 Saved to 'external_features.csv'")
    else:
        print("⚠️  Only basic sentiment data available")