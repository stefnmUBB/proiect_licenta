import os
import cv2
import numpy as np
import itertools
import struct

from utils import enhance, text_to_labels

q=0

def enumerate_dataset(dataset_path, encoding, IMG_SIZE):
    for root, dirs, files in os.walk(dataset_path, topdown=False):
        for name in files:            
            if name.endswith(".png") or name.endswith('.jpg'):
                if not os.path.exists(os.path.join(root, name[:-4] + ".seg.bin")):
                    continue
            
            
                img = cv2.imread(os.path.join(root, name), cv2.IMREAD_GRAYSCALE)
                img = enhance(img)                                                            
                
                img = cv2.resize(img, IMG_SIZE[::-1])
                img = np.reshape(img, IMG_SIZE + (1,))                              
                
                with open(os.path.join(root, name[:-4] + ".seg.bin"), "br") as f:                
                    data = f.read()
                    polys = struct.unpack('f'*(len(data)//4), data)                
                
                yield img/255., polys, len(polys)

def enumerate_datasets(datasets: list[tuple[str, str]], IMG_SIZE, limit=None):    
    count = 0
    while True:
        iterators = [itertools.cycle(enumerate_dataset(path, enc, IMG_SIZE)) for (path,enc) in datasets]
        
        yielded=True
        while yielded:
            yielded=False
            for it in iterators:
                nxt = next(it, None)
                if nxt is not None:        
                    yield nxt
                    yielded=True
                    count += 1
                if limit is not None and count >= limit:
                    return
