"""
Preprocessing function for time-series sequences with missing data handling.
Filters out sequences with >10% missing data in price/volume columns (eth_price, 
eth_volume, btc_price, btc_volume), and fills remaining missing values with 0.
"""

import numpy as np
from typing import List, Tuple


def preprocess_sequences(
    X_sequences: np.ndarray,
    y_labels: np.ndarray,
    price_volume_indices: List[int],
    missing_threshold: float = 0.10,
    verbose: bool = True
) -> Tuple[np.ndarray, np.ndarray]:
    """
    Preprocess and filter sequences based on missing data in price/volume columns.
    
    For each sequence:
    1. Check missing percentage in eth_price, eth_volume, btc_price, btc_volume
    2. If >10% missing: discard the entire sequence
    3. If ≤10% missing: fill all NaN values with 0 and keep the sequence
    
    Parameters:
    -----------
    X_sequences : np.ndarray
        Input sequences of shape (num_sequences, sequence_length, num_features)
    y_labels : np.ndarray
        Labels corresponding to each sequence
    price_volume_indices : List[int]
        Indices of the 4 price/volume features: [eth_price_idx, eth_volume_idx, 
        btc_price_idx, btc_volume_idx]
    missing_threshold : float, default=0.10
        Maximum allowed percentage of missing data (as decimal). Default is 0.10 (10%)
    verbose : bool, default=True
        Whether to print progress and statistics
        
    Returns:
    --------
    Tuple[np.ndarray, np.ndarray]
        Filtered and imputed sequences (X_clean, y_clean)
        
    Example:
    --------
    >>> # Assuming eth_price, eth_volume, btc_price, btc_volume are at indices 50-53
    >>> X_train_clean, y_train_clean = preprocess_sequences(
    ...     X_train, y_train, 
    ...     price_volume_indices=[50, 51, 52, 53],
    ...     missing_threshold=0.10
    ... )
    """
    
    num_sequences, sequence_length, num_features = X_sequences.shape
    
    if verbose:
        print(f"Starting preprocessing of {num_sequences:,} sequences")
        print(f"Sequence shape: ({sequence_length} timesteps, {num_features} features)")
        print(f"Missing data threshold: {missing_threshold*100:.1f}%")
        print(f"Checking price/volume at feature indices: {price_volume_indices}\n")
    
    # Lists to store valid sequences
    valid_sequences = []
    valid_labels = []
    
    # Statistics tracking
    discarded_count = 0
    filled_count = 0
    perfect_count = 0
    
    # Process each sequence individually
    for idx in range(num_sequences):
        sequence = X_sequences[idx].copy()  # Shape: (sequence_length, num_features)
        label = y_labels[idx]
        
        # Extract only the 4 price/volume columns for this sequence
        price_volume_data = sequence[:, price_volume_indices]
        
        # Calculate missing percentage for these 4 columns
        total_values = price_volume_data.size
        missing_values = np.sum(np.isnan(price_volume_data))
        missing_percentage = missing_values / total_values
        
        # Discard if > threshold
        if missing_percentage > missing_threshold:
            discarded_count += 1
            continue
        
        # No missing data
        if missing_percentage == 0:
            perfect_count += 1
            valid_sequences.append(sequence)
            valid_labels.append(label)
            continue
        
        # Fill with 0 if <= threshold
        filled_count += 1
        sequence[np.isnan(sequence)] = 0
        
        valid_sequences.append(sequence)
        valid_labels.append(label)
    
    # Convert lists back to arrays
    X_clean = np.array(valid_sequences)
    y_clean = np.array(valid_labels)
    
    if verbose:
        print(f"{'='*60}")
        print(f"PREPROCESSING RESULTS")
        print(f"{'='*60}")
        print(f"Original sequences:     {num_sequences:,}")
        print(f"Perfect (no missing):   {perfect_count:,} ({100*perfect_count/num_sequences:.2f}%)")
        print(f"Filled with 0 (≤{missing_threshold*100:.0f}%): {filled_count:,} ({100*filled_count/num_sequences:.2f}%)")
        print(f"Discarded (>{missing_threshold*100:.0f}%):    {discarded_count:,} ({100*discarded_count/num_sequences:.2f}%)")
        print(f"\nFinal sequences:        {len(X_clean):,} ({100*len(X_clean)/num_sequences:.2f}%)")
        print(f"Shape: {X_clean.shape}")
        print(f"NaN values remaining:   {np.sum(np.isnan(X_clean))}")
        print(f"{'='*60}\n")
    
    return X_clean, y_clean


if __name__ == "__main__":
    # Example usage demonstration
    print("Example usage of preprocess_sequences:")
    print("="*60)
    
    # Create synthetic data for demonstration
    np.random.seed(42)
    num_sequences = 1000
    sequence_length = 60
    num_features = 10
    
    # Generate sequences
    X_example = np.random.randn(num_sequences, sequence_length, num_features)
    y_example = np.random.randint(0, 3, size=num_sequences)
    
    # Introduce missing data in price/volume columns (indices 6-9)
    price_volume_indices = [6, 7, 8, 9]
    
    for i in range(num_sequences):
        missing_prob = np.random.rand()
        if missing_prob < 0.3:  # 30% have missing data
            # Random percentage between 1% and 20%
            missing_pct = np.random.uniform(0.01, 0.20)
            # Only add missing to price/volume columns
            for col_idx in price_volume_indices:
                num_missing = int(sequence_length * missing_pct / 4)
                missing_rows = np.random.choice(sequence_length, num_missing, replace=False)
                X_example[i, missing_rows, col_idx] = np.nan
    
    print("\nApplying preprocessing...")
    X_clean, y_clean = preprocess_sequences(
        X_example, 
        y_example,
        price_volume_indices=price_volume_indices,
        missing_threshold=0.10,
        verbose=True
    )
    
    print(f"Verification complete!")
