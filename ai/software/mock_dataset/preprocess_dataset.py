import pandas as pd
import os

path = os.path.join(os.path.dirname(__file__), "dataset.csv")
output_path = os.path.join(os.path.dirname(__file__), "dataset_preprocessed.csv")

df = pd.read_csv(path)
df = df.astype(str)
df["datetime"] = df["date"].str.cat(df["time"].str[0:2], sep="-")
df.drop(["date", "time"], axis=1, inplace=True)
df["datetime_key"] = pd.factorize(df["datetime"])[0]

df.to_csv(output_path, index=False)
