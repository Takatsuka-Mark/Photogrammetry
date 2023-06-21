from os import path, mkdir

def create_dir_if_not_exists(dir):
    if path.isdir(dir):
        return
    mkdir(dir)
