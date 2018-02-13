from fs import open_fs
from fs.copy import copy_file
from fs.mountfs import MountFS
from fs.walk import Walker
import sys
import os
import subprocess

profiles_dir = sys.argv[3]
mods_dir = sys.argv[2]
exe_path = sys.argv[1]
game_path = sys.argv[4]

modlist = open(r'{}\modlist.txt'.format(profiles_dir), 'r')
modlist_lines = modlist.readlines()

data_fs = open_fs(game_path)
mods_fs = open_fs(mods_dir)
full_fs = MountFS()
full_fs.mount('data', data_fs)

for m in modlist_lines:
	currdir = r'{}\{}'.format(mods_dir, m[2:-1])
	for f in os.walk(currdir):
		src = r'{}\{}'.format(currdir, f[2][0])
		src = r'%s' % src
		dst = r'{}\{}'.format(game_path, f[2][0])
		copy_file(mods_fs, r'%s' % src, data_fs, r'%s' % dst)

subprocess.run(exe_path, shell=True, check=True)
mods_fs.close()
data_fs.close()
modlist.close()
full_fs.close()