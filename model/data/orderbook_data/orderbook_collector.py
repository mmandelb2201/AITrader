"""
Order Book Data Collector for ETH
Collects order book snapshots from Coinbase and saves to CSV
"""

import requests
import pandas as pd
import numpy as np
import time
from datetime import datetime, timedelta
from pathlib import Path
import json
import logging
from logging.handlers import RotatingFileHandler

# Configure logging with rotation (max 10MB per file, keep 5 files)
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

# Console handler
console_handler = logging.StreamHandler()
console_handler.setLevel(logging.INFO)
console_formatter = logging.Formatter("%(asctime)s [%(levelname)s] %(message)s")
console_handler.setFormatter(console_formatter)

# File handler with rotation
file_handler = RotatingFileHandler(
    'orderbook_collector.log',
    maxBytes=10 * 1024 * 1024,  # 10 MB per file
    backupCount=5  # Keep 5 old files (total max ~60 MB)
)
file_handler.setLevel(logging.INFO)
file_formatter = logging.Formatter("%(asctime)s [%(levelname)s] %(message)s")
file_handler.setFormatter(file_formatter)

logger.addHandler(console_handler)
logger.addHandler(file_handler)

class OrderBookCollector:
    def __init__(self, exchange='coinbase', product_id='ETH-USD', n_levels=10):
        """
        Initialize order book collector
        
        Args:
            exchange: 'coinbase' or 'binance'
            product_id: Trading pair (ETH-USD for Coinbase, ETHUSDT for Binance)
            n_levels: Number of order book levels to collect (default: 10)
        """
        self.exchange = exchange
        self.product_id = product_id
        self.n_levels = n_levels
        self.data = []
        
    def fetch_coinbase_orderbook(self):
        """Fetch order book from Coinbase"""
        url = f'https://api.exchange.coinbase.com/products/{self.product_id}/book'
        params = {'level': 2}  # Top 50 levels
        
        try:
            response = requests.get(url, params=params, timeout=5)
            response.raise_for_status()
            return response.json()
        except Exception as e:
            logger.error(f"Failed to fetch Coinbase orderbook: {e}", exc_info=True)
            return None
    
    def fetch_binance_orderbook(self):
        """Fetch order book from Binance"""
        url = 'https://api.binance.com/api/v3/depth'
        params = {'symbol': self.product_id, 'limit': 100}
        
        try:
            response = requests.get(url, params=params, timeout=5)
            response.raise_for_status()
            return response.json()
        except Exception as e:
            logger.error(f"Failed to fetch Binance orderbook: {e}", exc_info=True)
            return None
    
    def orderbook_to_features(self, orderbook):
        """
        Convert order book to flat feature vector (DeepLOB style)
        
        Returns 40 features for 10 levels:
            [bid_price_1, bid_vol_1, ask_price_1, ask_vol_1, 
             bid_price_2, bid_vol_2, ask_price_2, ask_vol_2, ...]
        """
        features = []
        
        if self.exchange == 'coinbase':
            bids = [[float(b[0]), float(b[1])] for b in orderbook.get('bids', [])[:self.n_levels]]
            asks = [[float(a[0]), float(a[1])] for a in orderbook.get('asks', [])[:self.n_levels]]
        else:  # binance
            bids = [[float(b[0]), float(b[1])] for b in orderbook.get('bids', [])[:self.n_levels]]
            asks = [[float(a[0]), float(a[1])] for a in orderbook.get('asks', [])[:self.n_levels]]
        
        for i in range(self.n_levels):
            if i < len(bids):
                features.extend([bids[i][0], bids[i][1]])
            else:
                features.extend([0.0, 0.0])
            
            if i < len(asks):
                features.extend([asks[i][0], asks[i][1]])
            else:
                features.extend([0.0, 0.0])
        
        return features
    
    def collect_snapshot(self):
        """Collect a single order book snapshot"""
        timestamp = datetime.utcnow()
        
        if self.exchange == 'coinbase':
            orderbook = self.fetch_coinbase_orderbook()
        else:
            orderbook = self.fetch_binance_orderbook()
        
        if orderbook is None:
            logger.warning("Skipped snapshot due to missing orderbook")
            return None
        
        features = self.orderbook_to_features(orderbook)
        
        # Create record with timestamp and features
        record = {'timestamp': timestamp}
        for i in range(self.n_levels):
            idx = i * 4
            record[f'bid_price_{i+1}'] = features[idx]
            record[f'bid_vol_{i+1}'] = features[idx + 1]
            record[f'ask_price_{i+1}'] = features[idx + 2]
            record[f'ask_vol_{i+1}'] = features[idx + 3]
        
        return record
    
    def collect_continuous(self, duration_hours=1, interval_seconds=10, save_path='orderbook_data.csv'):
        """
        Collect order book data continuously
        
        Args:
            duration_hours: How long to collect (hours)
            interval_seconds: Time between snapshots (seconds)
            save_path: Where to save the CSV
        """
        save_path = Path(save_path)
        save_path.parent.mkdir(parents=True, exist_ok=True)
        
        end_time = datetime.utcnow() + timedelta(hours=duration_hours)
        snapshots_collected = 0
        
        logger.info("📊 Starting order book collection...")
        logger.info(f"   Exchange: {self.exchange}")
        logger.info(f"   Product: {self.product_id}")
        logger.info(f"   Duration: {duration_hours} hours")
        logger.info(f"   Interval: {interval_seconds} seconds")
        logger.info(f"   Save path: {save_path}")
        logger.info(f"   Storage mode: Keep only last 100 rows")
        logger.info(f"   Expected snapshots: ~{int(duration_hours * 3600 / interval_seconds):,}")
        print(f"   Expected snapshots: ~{int(duration_hours * 3600 / interval_seconds):,}")
        
        try:
            while datetime.utcnow() < end_time:
                print(f"🔍 Fetching snapshot {snapshots_collected + 1}...")
                snapshot = self.collect_snapshot()
                
                if snapshot:
                    self.data.append(snapshot)
                    snapshots_collected += 1
                    
                    # Keep only last 100 rows in memory to save storage
                    if len(self.data) > 100:
                        self.data = self.data[-100:]
                    
                    # Save every 100 snapshots (but CSV will only have last 100 rows)
                    if snapshots_collected % 100 == 0:
                        logger.info(f"📈 Collected {snapshots_collected} snapshots (latest: {snapshot['timestamp']})")
                        print(f"💾 Saving to CSV (last 100 rows only)...")
                        df = pd.DataFrame(self.data)
                        df.to_csv(save_path, index=False)
                        logger.info(f"✅ Saved last 100 snapshots to {save_path}")
                        print(f"✅ Saved last 100 snapshots to {save_path}")
                
                time.sleep(interval_seconds)
        
        except KeyboardInterrupt:
            logger.warning("\n⚠️  Collection interrupted by user")
        
        finally:
            # Final save (only last 100 rows)
            if self.data:
                df = pd.DataFrame(self.data[-100:])
                df.to_csv(save_path, index=False)
                logger.info(f"\n✅ Final save: {len(df)} snapshots saved to {save_path}")
                logger.info(f"   Time range: {df['timestamp'].min()} to {df['timestamp'].max()}")
                logger.info(f"   File size: {save_path.stat().st_size / 1024 / 1024:.2f} MB")
            else:
                logger.warning("\n⚠️  No data collected")

def main():
    """Example usage"""
    # Collect from Coinbase - FREE, no API key needed, no geo restrictions!
    # 1 snapshot per minute = 1,440 snapshots/day
    # Run continuously - will collect whenever laptop is on
    
    collector = OrderBookCollector(
        exchange='coinbase',
        product_id='ETH-USD',
        n_levels=10
    )
    
    # Collect continuously (1 snapshot every 5 seconds)
    # Set high duration - will run until you stop it
    collector.collect_continuous(
        duration_hours=24 * 365,  # 1 year (will run until stopped)
        interval_seconds=5,        # 5 second intervals
        save_path='eth_orderbook_coinbase_5s.csv'
    )

if __name__ == '__main__':
    main()
