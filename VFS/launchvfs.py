from fs import open_fs
from fs.copy import copy_fs
from fs.mountfs import MountFS
from fs.walk import Walker
import sys
import os
import subprocess

print(sys.argv)

profiles_dir = sys.argv[3]
mods_dir = sys.argv[2]
exe_path = sys.argv[1]
game_path = sys.argv[4]

modlist = open('{}\modlist.txt'.format(profiles_dir), 'r')
modlist_lines = modlist.readlines()
print(modlist_lines)

data_fs = open_fs('./Data')
game_fs = open_fs(game_path)
full_fs = MountFS()
full_fs.mount('game', game_fs)
full_fs.mount('game/data', data_fs)

for m in modlist_lines:
    print(m[2:])
    for f in os.walk('{}/{}'.format(mods_dir, m[2:])):
            copy_fs(data_fs, currdir)

subprocess.run(exe_path, shell=True, check=True)
game_fs.close()
#mods_fs.close()
data_fs.close()
modlist.close()