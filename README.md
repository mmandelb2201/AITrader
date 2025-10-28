# AITrader

An AI-powered cryptocurrency trading bot that uses LSTM (Long Short-Term Memory) neural networks with asset embeddings to predict price movements and execute trades via the Coinbase API.

## Overview

AITrader combines machine learning and automated trading to analyze cryptocurrency market data and make informed trading decisions. The system consists of three main components:

1. **LSTM Model** - A TensorFlow-based deep learning model that predicts cryptocurrency price movements
2. **Trading Bot** - A C#/.NET application that executes trades on Coinbase based on model predictions
3. **Paper Wallet** - A simulation tool for backtesting trading strategies without risking real funds

## Features

- 🤖 **AI-Driven Predictions**: LSTM neural network with asset embeddings for multi-cryptocurrency price forecasting
- 📊 **Real-Time Trading**: Automated trading execution through Coinbase Advanced API
- 🔄 **Continuous Learning**: Model trained on hourly historical price and volume data
- 🧪 **Backtesting**: Paper trading wallet for strategy validation
- 🐳 **Docker Support**: Containerized deployment for easy setup
- 📈 **Multi-Asset Support**: Tracks and trades multiple cryptocurrencies simultaneously
- 🔐 **Secure Authentication**: JWT-based authentication with Coinbase API

## Architecture

### LSTM Model (`/model/LSTM Model/`)

The machine learning component built with TensorFlow that:
- Processes hourly cryptocurrency price and volume data
- Uses a stacked LSTM architecture with asset embeddings
- Predicts price percentage changes over a configurable horizon (default: 3 hours)
- Supports multiple cryptocurrencies with shared learned representations

**Key Files:**
- `crypto_lstm_embedding_tf.ipynb` - Main training notebook
- `top_cryptos.py` - Fetches top cryptocurrencies by market cap from CoinGecko
- `external_data_collector.py` - Collects historical data from CoinAPI
- `backtest.ipynb` - Backtesting utilities
- `config.py` - Model hyperparameters and configuration

**Model Configuration:**
```python
LOOKBACK_L = 96     # lookback window length (hours)
HORIZON_H  = 3      # prediction horizon (hours)
FEATURES = ['price_pct_change', 'volume_pct_change']
BATCH_TRAIN = 256
EPOCHS = 30
```

### Trading Bot (`/Trading Bot/`)

A .NET 8.0 console application that:
- Loads and runs the trained ONNX model for predictions
- Executes buy/sell orders on Coinbase
- Manages portfolio balances and positions
- Runs continuously with configurable prediction intervals

**Key Components:**
- `Program.cs` - Main entry point and trading loop
- `Coinbase/` - Coinbase API client and JWT authentication
- `Model/` - ONNX runtime integration for predictions
- `Trader/` - Trading logic and execution
- `Config/` - XML-based configuration management

**Technologies:**
- .NET 8.0
- Microsoft.ML.OnnxRuntime
- CoinbaseAdvanced API

### Paper Wallet (`/model/paper_wallet/`)

A Python-based simulation tool for testing trading strategies:
- Simulates buy/sell operations without real funds
- Tracks USD and ETH balances
- Maintains complete trade history
- Calculates portfolio performance metrics

## Prerequisites

### For the LSTM Model:
- Python 3.8+
- TensorFlow 2.x
- Jupyter Notebook
- API Keys:
  - CoinAPI key (for historical data)
  - CoinGecko API (free tier works)

### For the Trading Bot:
- .NET 8.0 SDK
- Docker (optional, for containerized deployment)
- Coinbase Advanced Trade API credentials:
  - API Key
  - API Secret

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/mmandelb2201/AITrader.git
cd AITrader
```

### 2. Set Up the LSTM Model

```bash
cd "model/LSTM Model"

# Install Python dependencies
pip install tensorflow pandas numpy requests matplotlib jupyter

# Set up your CoinAPI key
export COIN_API_KEY="your_coinapi_key_here"

# Run Jupyter notebooks to train the model
jupyter notebook
```

Open `crypto_lstm_embedding_tf.ipynb` to train the model.

### 3. Set Up the Trading Bot

```bash
cd "Trading Bot/Trading Bot"

# Restore dependencies
dotnet restore

# Create a .env file with your Coinbase credentials
cat > .env << EOF
COINBASE_API_KEY=your_api_key
COINBASE_API_SECRET=your_api_secret
EOF

# Configure Config.xml with your settings
# (Set model path, trading parameters, etc.)

# Build the project
dotnet build

# Run the bot
dotnet run
```

### 4. Docker Deployment (Optional)

```bash
cd "Trading Bot"

# Build the Docker image
docker build -t aitrader .

# Run the container
docker run -v $(pwd)/Config.xml:/app/Config.xml \
           -v $(pwd)/.env:/app/.env \
           aitrader
```

## Configuration

### Model Configuration (`model/LSTM Model/config.py`)

Adjust these parameters based on your training needs:
- `LOOKBACK_L` - Number of hours of historical data to use for predictions
- `HORIZON_H` - Number of hours ahead to predict
- `EPOCHS` - Number of training epochs
- `BATCH_TRAIN` - Training batch size

### Trading Bot Configuration (`Trading Bot/Trading Bot/Config.xml`)

Configure the bot's behavior:
- Model path and training data location
- Prediction interval (how often to make predictions)
- Trading parameters and thresholds
- Asset selection

## Usage

### Training the Model

1. Collect data using the data grabber notebooks
2. Run the training notebook (`crypto_lstm_embedding_tf.ipynb`)
3. Convert the model to ONNX format using `model_converter.ipynb`
4. Place the ONNX model in the path specified in `Config.xml`

### Running the Trading Bot

```bash
cd "Trading Bot/Trading Bot"
dotnet run
```

The bot will:
1. Load the configuration and ONNX model
2. Initialize the min-max scaler with training data
3. Enter a continuous loop where it:
   - Makes predictions using the latest market data
   - Executes trades based on predictions
   - Waits for the configured interval
   - Repeats

Press `q` to quit the program gracefully.

### Backtesting with Paper Wallet

```python
from wallet import PaperWallet

# Initialize with starting balance
wallet = PaperWallet(starting_usd=10000.0)

# Simulate trades
wallet.buy_eth(usd_amount=1000, price=2000)
wallet.sell_eth(eth_amount=0.5, price=2100)

# Check performance
wallet.print_summary()
wallet.print_total_value(eth_price=2100)
```

## Project Structure

```
AITrader/
├── model/
│   ├── LSTM Model/
│   │   ├── crypto_lstm_embedding_tf.ipynb    # Main training notebook
│   │   ├── top_cryptos.py                     # Crypto selection
│   │   ├── external_data_collector.py         # Data collection
│   │   ├── backtest.ipynb                     # Backtesting
│   │   ├── config.py                          # Model config
│   │   └── ...
│   └── paper_wallet/
│       └── wallet.py                          # Paper trading wallet
├── Trading Bot/
│   └── Trading Bot/
│       ├── Program.cs                         # Main entry point
│       ├── Coinbase/                          # Coinbase API client
│       ├── Model/                             # ML model integration
│       ├── Trader/                            # Trading logic
│       ├── Config/                            # Configuration
│       ├── Dockerfile                         # Docker configuration
│       ├── Config.xml                         # Bot configuration
│       └── Trading Bot.csproj                 # .NET project file
├── LICENSE                                     # MIT License
└── README.md                                   # This file
```

## Security Considerations

⚠️ **Important Security Notes:**

- Never commit API keys or secrets to version control
- Use environment variables or secure vaults for sensitive data
- The `.env` file is git-ignored by default
- Start with paper trading to validate your strategy
- Only invest what you can afford to lose
- Cryptocurrency trading carries significant risk

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This software is for educational purposes only. Use at your own risk. The authors are not responsible for any financial losses incurred through the use of this software. Cryptocurrency trading carries substantial risk, and past performance does not guarantee future results.

## Acknowledgments

- Built with TensorFlow and .NET 8.0
- Uses Coinbase Advanced Trade API
- Market data from CoinAPI and CoinGecko
- Inspired by modern AI-driven trading strategies

---

**Note**: This is an active development project. Features and functionality may change. Always test thoroughly with paper trading before deploying with real funds.
