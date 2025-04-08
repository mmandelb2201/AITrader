from datetime import datetime

class PaperWallet:
    def __init__(self, starting_usd=10000.0, starting_eth=0.0):
        self.usd_balance = starting_usd
        self.eth_balance = starting_eth
        self.trade_history = []
        self.starting_balance = starting_usd

    def _log_trade(self, trade_type, eth_amount, price, usd_amount):
        self.trade_history.append({
            "timestamp": datetime.utcnow().isoformat(),
            "type": trade_type,
            "eth": eth_amount,
            "price": price,
            "usd": usd_amount
        })

    def buy_eth(self, usd_amount, price):
        if usd_amount > self.usd_balance:
            raise ValueError(f"Insufficient USD balance. Needed ${usd_amount}, available ${self.usd_balance:.2f}")
        eth_amount = usd_amount/price
        self.usd_balance -= usd_amount
        self.eth_balance += eth_amount
        self._log_trade("BUY", eth_amount, price, -usd_amount)

    def sell_eth(self, eth_amount, price):
        if eth_amount > self.eth_balance:
            raise ValueError(f"Insufficient ETH balance. Needed {eth_amount:.4f}, available {self.eth_balance:.4f}")
        usd_earned = eth_amount * price
        self.eth_balance -= eth_amount
        self.usd_balance += usd_earned
        self._log_trade("SELL", eth_amount, price, usd_earned)

    def get_balance(self):
        return {
            "USD": self.usd_balance,
            "ETH": self.eth_balance
        }

    def get_trade_history(self):
        return self.trade_history

    def print_summary(self):
        print(f"USD Balance: ${self.usd_balance:.2f}")
        print(f"ETH Balance: {self.eth_balance:.6f} ETH")
        print("Recent Trades:")
        for trade in self.trade_history[-5:]:
            print(trade)

    def print_total_value(self, eth_price):
        total_value = self.usd_balance + (self.eth_balance * eth_price)
        print(f"Total Value: ${total_value}")
        print(f"Percent Difference: {((total_value - self.starting_balance)/self.starting_balance) * 100}")
