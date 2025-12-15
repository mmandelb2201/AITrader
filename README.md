# AITrader

An AI-powered cryptocurrency trading bot that uses a hybrid CNN-LSTM neural network to predict price direction from L2 order book data and execute trades via the Coinbase API.

## Overview

AITrader combines machine learning and automated trading to analyze cryptocurrency market data and make informed trading decisions. The system consists of three main components:

1. **CNN-LSTM Classifier Model** - A hybrid deep learning model using convolutional and LSTM layers to predict cryptocurrency price direction from L2 order book data
2. **Trading Bot** - A C#/.NET application that executes trades on Coinbase based on model predictions
3. **Paper Wallet** - A simulation tool for backtesting trading strategies without risking real funds

## Features

- 🤖 **AI-Driven Predictions**: CNN-LSTM hybrid neural network for cryptocurrency price direction classification using L2 order book data
- 📊 **Real-Time Trading**: Automated trading execution through Coinbase Advanced API
- 🔄 **Continuous Learning**: Model trained on high-frequency (5-second) L2 order book data
- 🧪 **Backtesting**: Paper trading wallet for strategy validation
- 🐳 **Docker Support**: Containerized deployment for easy setup
- 📈 **Multi-Asset Support**: Tracks and trades multiple cryptocurrencies simultaneously
- 🔐 **Secure Authentication**: JWT-based authentication with Coinbase API

## Architecture

### CNN-LSTM Classifier Model (`/model/classifier_model/`)

The machine learning component built with TensorFlow that:
- Processes high-frequency (5-second) Level 2 order book data
- Uses a hybrid CNN-LSTM architecture to capture both local patterns and temporal dependencies
- Predicts short-term price direction (up/down) using dynamic triple barrier labeling
- Features advanced preprocessing: log returns, robust scaling, and volatility-based thresholds

**Key Files:**
- `simple_l2_cnn.ipynb` - **Main training notebook** for the CNN-LSTM classifier
- `midprice_direction_classifier.ipynb` - Alternative classifier implementation
- `feature_analysis.ipynb` - Feature engineering and analysis tools
- `config.py` - Model hyperparameters and configuration

**Model Architecture:**
- **Block 1 (CNN):** Local feature extraction with 64 filters capturing short-term order book patterns
- **Block 2 (LSTM):** Temporal memory layers (2x64 units) for long-term context
- **Block 3 (Dense):** Classification head with softmax output

**Model Configuration:**
```python
SEQUENCE_LENGTH = 100    # 500 seconds lookback window (5s intervals)
LOOKAHEAD_STEPS = 180    # 15 minutes prediction horizon (5s intervals)
TAKE_PROFIT_PCT = 0.0015 # +0.15% dynamic threshold
STOP_LOSS_PCT = 0.0015   # -0.15% dynamic threshold
```

**Key Features:**
- Dynamic triple barrier labeling using rolling volatility
- Robust feature engineering (log returns, log volume changes, relative spreads)
- RobustScaler for outlier resistance (crypto whale orders, flash crashes)
- Chronological train/validation split to prevent look-ahead bias

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

### For the CNN-LSTM Classifier Model:
- Python 3.8+
- TensorFlow 2.x
- scikit-learn
- Jupyter Notebook
- L2 order book data (5-second interval snapshots)

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

### 2. Set Up the CNN-LSTM Classifier Model

```bash
cd "model/classifier_model"

# Install Python dependencies
pip install tensorflow pandas numpy scikit-learn matplotlib jupyter

# Ensure you have L2 order book data in the correct path
# Default: ../data/5s_data/eth_orderbook_coinbase_5s_with_price_volume.csv

# Run Jupyter notebooks to train the model
jupyter notebook
```

Open `simple_l2_cnn.ipynb` to train the CNN-LSTM classifier model.

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

### Model Configuration (`model/classifier_model/simple_l2_cnn.ipynb`)

Adjust these parameters in the notebook based on your training needs:
- `SEQUENCE_LENGTH` - Number of timesteps in lookback window (default: 100 = 500 seconds)
- `LOOKAHEAD_STEPS` - Prediction horizon in timesteps (default: 180 = 15 minutes)
- `TAKE_PROFIT_PCT` / `STOP_LOSS_PCT` - Dynamic threshold multipliers for triple barrier labeling
- Training parameters: batch size, epochs, learning rate, etc.

### Trading Bot Configuration (`Trading Bot/Trading Bot/Config.xml`)

Configure the bot's behavior:
- Model path and training data location
- Prediction interval (how often to make predictions)
- Trading parameters and thresholds
- Asset selection

## Usage

### Training the Model

1. Collect L2 order book data using the data grabber notebooks in `/model/data/`
2. Run the main training notebook (`simple_l2_cnn.ipynb`) to train the CNN-LSTM classifier
3. Convert the model to ONNX format for deployment (if needed)
4. Place the trained model in the path specified in `Config.xml`

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
│   ├── classifier_model/
│   │   ├── simple_l2_cnn.ipynb                # Main CNN-LSTM training notebook
│   │   ├── midprice_direction_classifier.ipynb # Alternative classifier
│   │   ├── feature_analysis.ipynb             # Feature engineering tools
│   │   ├── config.py                          # Model configuration
│   │   └── models/                            # Saved model files
│   ├── data/
│   │   ├── data_grabber.ipynb                 # Data collection notebooks
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
