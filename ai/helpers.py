import numpy as np


def generate_data(
    tuples,
    features,
    window_size,
):
    """
    This function loads the data and returns it as numpy arrays.

    Args:
        tuples (list): list of tuples, (pandas dataframes, label)
        features (list): list of feature names

    Returns:
        x (np.array): feature matrix
        y (np.array): labels (0 or 1)
    """

    x = []
    y = []

    for df, label in tuples:
        feature_vector = df[features].to_numpy()
        x_windows, y_windows = sliding_window(feature_vector, label, window_size)
        for ele in x_windows:
            x.append(ele)
        for ele in y_windows:
            y.append(ele)

    x = np.array(x)

    return x, y


def sliding_window(x, y, window_size):
    """
    This function creates a sliding window representation of the data.

    Args:
        x (np.array): features (no. of samples x no. of time steps x no. of features)
        y (np.array): labels (no. of samples)
        window_size (int): size of sliding window
    """
    if window_size % 2 != 0:
        raise ValueError("Window size must be an even number.")

    stride = window_size // 2

    # Initialize empty lists
    x_window = []
    y_window = []

    # Create sliding windows
    idx = 0
    while idx + window_size <= len(x):
        x_window.append(x[idx : idx + window_size])
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
        y (np.array): labels (0 or 1)

    Returns:
        x (np.array): feature matrix (no. of samples x no. of features)
        y (np.array): labels (0 or 1)
    """
    result = np.apply_along_axis(extract_features, axis=2, arr=x)
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

    freq = np.fft.fft(data)
    fmin = np.min(freq)
    fmax = np.max(freq)
    fpower = np.sum(np.square(np.abs(freq)))

    features = np.array([tmin, tmax, tmean, tstd_dev, trms, fmin, fmax, fpower])

    return features
