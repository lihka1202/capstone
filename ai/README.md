## Folder Structure

- `hls/` - Code for HLS
  - `mlp.cpp` - HLS code for the MLP model
  - `test_mlp.cpp` - HLS test code for the MLP model
- `pynq/` - Contains code needed to run the model on the FPGA board
  - `mlp.bit`, `mlp.hwh`, `scaler.pkl` - Files needed to preprocess data and load AI model onto FPGA
  - `pynq_driver.py` - Python driver code to run the model on the FPGA board
  - `test_script.py` - Python script to test the model on the FPGA board
  - `test.ipynb` - Jupyter notebook used when developing to test code on the FPGA board
- `software/` - Contains code for the software model
  - `mlp.ipynb` - Software implementation of the MLP model
  - `mlp_mock.ipynb` - Mock implementation of the MLP model on an online dataset
  - `data_augmentation.ipynb` - Jupyter notebook containing scripts to modify the dataset for training
  - `dataset/` - Contains the dataset used for the software model
    - `augmented_xxx.csv` - Final dataset used for training the model

## Instructions to work with FPGA

Run `ssh -L 9090:localhost:9090 xilinx@makerslab-fpga-42.d2.comp.nus.edu.sg` and access `http://localhost:9090` in your browser to access the Jupyter notebook.

To copy file from local to remote, run `scp <local_file> xilinx@makerslab-fpga-42.d2.comp.nus.edu.sg:.` on local env

To run the python files, run `sudo -i` on Ultra96 and `cd /home/xilinx` to access home directory

## Useful

`scp mlp.bit mlp.hwh scaler.pkl pynq_driver.py test_script.py xilinx@makerslab-fpga-42.d2.comp.nus.edu.sg:.`

https://capitalizemytitle.com/tools/column-to-comma-separated-list/ - to convert csv motion data to comma separated list for python
