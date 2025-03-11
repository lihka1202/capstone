import numpy as np
from scipy.stats import skew, kurtosis


def generate_data(
    tuples,
    features,
    window_size,
    stride,
):
    """
    This function loads the data and returns it as numpy arrays.

    Args:
        tuples (list): list of tuples, (pandas dataframes, label)
        features (list): list of feature names

    Returns:
        x (np.array): feature matrix
        y (np.array): labels
    """

    x = []
    y = []

    for df, label in tuples:
        feature_vector = df[features].to_numpy()
        x_windows, y_windows = sliding_window(
            feature_vector, label, window_size, stride
        )
        for ele in x_windows:
            x.append(ele)
        for ele in y_windows:
            y.append(ele)

    x = np.array(x)
    y = np.array(y)

    return x, y


def sliding_window(x, y, window_size, stride):
    """
    This function creates a sliding window representation of the data.

    Args:
        x (np.array): features (no. of samples x no. of time steps x no. of features)
        y (np.array): labels (no. of samples)
        window_size (int): size of sliding window
    """

    # Initialize empty lists
    x_window = []
    y_window = []

    # Create sliding windows
    idx = 0
    while idx + window_size <= len(x):
        x_window.append(x[idx : idx + stride])
        # x_window.append(
        # x[idx : idx + window_size] * np.hamming(window_size)[:, None]
        # )  # Apply hamming window
        idx += stride

    # Convert to numpy arrays
    x_window = np.array(x_window)
    y_window = np.repeat(y, len(x_window))

    return x_window, y_window


def convert_time_series_to_features(x):
    """
    This function extracts features from time-series data.

    Args:
        x (np.array): feature matrix (no. of samples x no. of time steps x no. of features)

    Returns:
        x (np.array): feature matrix (no. of samples x no. of features)
    """
    result = np.apply_along_axis(extract_features, axis=1, arr=x)
    features = result.reshape(result.shape[0], -1)

    return features


def extract_features(data):
    """
    This function extracts features from the data.

    Args:
        data (np.array): data (no. of time series)

    Returns:
        features (np.array): extracted features
    """

    tmin = np.min(data)
    tmax = np.max(data)
    tmean = np.mean(data)
    tstd_dev = np.std(data)
    trms = np.sqrt(np.mean(np.square(data)))

    freq = np.fft.rfft(data)
    fft_magnitude = np.abs(freq)
    fmin = np.min(fft_magnitude)
    fmax = np.max(fft_magnitude)
    fpower = np.sum(fft_magnitude**2) / len(fft_magnitude)

    features = np.append(
        np.array([tmin, tmax, tmean, tstd_dev, trms, fmin, fmax, fpower]),
        data.flatten(),
    )

    return features
