"""
Crypto Data Collector (ETH + BTC)
Collects Order Book + Price/Volume data for multiple pairs simultaneously.
Logs consolidated status updates to console only.
"""

import requests
import pandas as pd
import time
from datetime import datetime
from pathlib import Path
import logging

# --- LOGGING SETUP ---
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

# Console Handler
console_handler = logging.StreamHandler()
# Format: [INFO][12:00:00] Message
console_handler.setFormatter(logging.Formatter("[%(levelname)s][%(asctime)s] %(message)s", datefmt="%H:%M:%S"))

logger.addHandler(console_handler)

class CryptoCollector:
    def __init__(self, products=['ETH-USD', 'BTC-USD'], n_levels=10):
        """
        Args:
            products: List of pairs to collect (e.g. ['ETH-USD', 'BTC-USD'])
            n_levels: Depth of order book to save
        """
        self.products = products
        self.n_levels = n_levels
        self.buffers = {p: [] for p in products}
        self.session = requests.Session()
        
        # Track total collection cycles
        self.api_calls = 0

    def fetch_data(self, product_id):
        """Fetch both OrderBook and Ticker (Price/Vol) data"""
        try:
            # 1. Get Order Book (Level 2)
            book_url = f'https://api.exchange.coinbase.com/products/{product_id}/book?level=2'
            book_res = self.session.get(book_url, timeout=2)
            book_res.raise_for_status()
            book = book_res.json()

            # 2. Get Ticker (Last Price & 24h Volume)
            ticker_url = f'https://api.exchange.coinbase.com/products/{product_id}/ticker'
            ticker_res = self.session.get(ticker_url, timeout=2)
            ticker_res.raise_for_status()
            ticker = ticker_res.json()

            return book, ticker

        except Exception as e:
            logger.error(f"Error fetching {product_id}: {e}")
            return None, None

    def process_snapshot(self, product_id, book, ticker):
        """Flatten data into a single row"""
        timestamp = datetime.utcnow()
        
        # Base record
        record = {
            'timestamp': timestamp,
            'price': float(ticker.get('price', 0)),
            'volume': float(ticker.get('volume', 0)), # 24h volume
        }

        # Flatten Order Book
        bids = book.get('bids', [])
        asks = book.get('asks', [])

        for i in range(self.n_levels):
            # Bids [price, size, num_orders]
            if i < len(bids):
                record[f'bid_price_{i+1}'] = float(bids[i][0])
                record[f'bid_vol_{i+1}']   = float(bids[i][1])
            else:
                record[f'bid_price_{i+1}'] = 0.0
                record[f'bid_vol_{i+1}']   = 0.0
            
            # Asks
            if i < len(asks):
                record[f'ask_price_{i+1}'] = float(asks[i][0])
                record[f'ask_vol_{i+1}']   = float(asks[i][1])
            else:
                record[f'ask_price_{i+1}'] = 0.0
                record[f'ask_vol_{i+1}']   = 0.0

        return record

    def save_buffers(self, data_dir='data'):
        """Append in-memory buffers to CSVs and clear memory"""
        Path(data_dir).mkdir(parents=True, exist_ok=True)

        for product_id, rows in self.buffers.items():
            if not rows:
                continue
            
            # Generate filename: data/ETH-USD_5s.csv
            filename = f"{product_id.replace('-','')}_5s.csv"
            filepath = Path(data_dir) / filename
            
            df = pd.DataFrame(rows)
            
            # Append if exists, write header if new
            header = not filepath.exists()
            df.to_csv(filepath, mode='a', header=header, index=False)
            
            # Clear buffer to free RAM
            self.buffers[product_id] = []
        
        logger.info(f"💾 Flushed data to disk")

    def run(self, interval=5):
        """Main loop"""
        logger.info(f"🚀 Starting collection for {self.products}")
        logger.info(f"⏱️  Interval: {interval}s")
        
        try:
            while True:
                start_time = time.time()
                
                # UPDATED: Increment counter once per loop iteration
                self.api_calls += 1
                
                current_stats = {} # Store stats for printing
                
                # 1. Fetch data for ALL products
                for product in self.products:
                    book, ticker = self.fetch_data(product)
                    
                    if book and ticker:
                        row = self.process_snapshot(product, book, ticker)
                        self.buffers[product].append(row)
                        
                        # Save stats for the log message
                        current_stats[product] = {
                            'price': row['price'],
                            'volume': row['volume']
                        }

                # 2. Print Consolidated Log
                if current_stats:
                    msg_parts = [f"Collected L2 Data (Iter: {self.api_calls})"]
                    
                    for prod in self.products:
                        if prod in current_stats:
                            stats = current_stats[prod]
                            # Format: ETH-USD: $2000.50 (Vol: 15400.0)
                            prod_msg = f"{prod}: ${stats['price']:.2f} (Vol: {stats['volume']:.0f})"
                            msg_parts.append(prod_msg)
                    
                    logger.info(" | ".join(msg_parts))

                # 3. Save to disk every 50 snapshots
                if len(list(self.buffers.values())[0]) >= 50:
                    self.save_buffers()

                # 4. Sleep
                elapsed = time.time() - start_time
                sleep_time = max(0, interval - elapsed)
                time.sleep(sleep_time)

        except KeyboardInterrupt:
            logger.info("\n🛑 Stopping... Saving remaining data.")
            self.save_buffers()
            logger.info("✅ Done.")

if __name__ == "__main__":
    # Collects ETH and BTC simultaneously
    collector = CryptoCollector(products=['ETH-USD', 'BTC-USD'])
    collector.run(interval=5)