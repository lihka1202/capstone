## Pynq

Run `ssh -L 9090:localhost:9090 xilinx@makerslab-fpga-42.d2.comp.nus.edu.sg` and access `http://localhost:9090` in your browser to access the Jupyter notebook.

To copy file from local to remote, run `scp <local_file> xilinx@makerslab-fpga-42.d2.comp.nus.edu.sg:.` on local env

To source venv, run `sudo -i` followed by `source /usr/local/share/pynq-venv/bin/activate` on Ultra96 and `cd /home/xilinx` to access home directory
