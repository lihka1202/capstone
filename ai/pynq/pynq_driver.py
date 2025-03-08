from pynq import Overlay, allocate
import os
import numpy as np
import pickle

INPUT_NODES = 168
OUTPUT_NODES = 7

cwd = os.getcwd()
bitstream_path = os.path.join(cwd, "mlp.bit")

# ol.free()
ol = Overlay(bitstream_path)

ol.ip_dict.keys()

dma = ol.dma
dma_send = dma.sendchannel
dma_receive = dma.recvchannel

dma.sendchannel.stop()
dma.recvchannel.stop()
dma.sendchannel.start()
dma.recvchannel.start()

input_buffer = allocate(shape=(INPUT_NODES,), dtype=np.float32)
output_buffer = allocate(shape=(OUTPUT_NODES,), dtype=np.float32)

mlp = ol.mlp_0

CONTROL_REGISTER = 0x0
mlp.write(CONTROL_REGISTER, 0x81)

print("FPGA initialized")

with open("scaler.pkl", "rb") as f:
    scaler = pickle.load(f)


def get_result(input_data):
    """
    Takes in 20 time steps * 6 imu data points and returns an action
    """
    input_data = _convert_time_series_to_features(input_data)
    input_data = scaler.transform(input_data.reshape(1, -1))
    return _get_fpga_output(input_data.flatten())


def _get_fpga_output(input_data):
    try:
        if len(input_data) != INPUT_NODES:
            print("Input data length doesn't match neural network input")
            return

        for i in range(len(input_data)):
            input_buffer[i] = input_data[i]
        dma_send.transfer(input_buffer)
        dma_send.wait()
        dma_receive.transfer(output_buffer)
        dma_receive.wait()
        # --------------- TIMING ---------------- #
        action = np.argmax(output_buffer)

    except Exception as e:
        print("FPGA error")
        print(e)
        return
    return action


def _convert_time_series_to_features(x):
    """
    This function extracts features from time-series data.

    Args:
        x (np.array): feature matrix (no. of samples x no. of time steps x no. of features)

    Returns:
        x (np.array): feature matrix (no. of samples x no. of features)
    """
    result = np.apply_along_axis(_extract_features, axis=0, arr=x)
    features = result.flatten()

    return features


def _extract_features(data):
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
    fpower = np.sum(fft_magnitude**2)

    features = np.append(
        np.array([tmin, tmax, tmean, tstd_dev, trms, fmin, fmax, fpower]),
        data.flatten(),
    )
    # features = np.array([tmin, tmax, tmean, tstd_dev, trms, fmin, fmax, fpower])

    return features
