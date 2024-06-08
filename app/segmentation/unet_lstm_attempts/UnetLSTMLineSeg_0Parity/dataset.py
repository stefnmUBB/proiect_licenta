import os
import cv2
import numpy as np
import itertools
import struct
import tensorflow as tf

from utils import enhance, text_to_labels

q=0

def enumerate_dataset(dataset_path, encoding, IMG_SIZE):
    for root, dirs, files in os.walk(dataset_path, topdown=False):
        for name in files:            
            #print(name)
            if name.endswith(".png") or name.endswith('.jpg'):
                inner_name = name[:-4]
                #print(inner_name)
                if not inner_name.endswith("_in"):
                    continue                 
                name_out = inner_name[:-3] + "_out.png"                
                if not os.path.exists(os.path.join(root, name_out)):
                    continue
                                    
                img_in = cv2.imread(os.path.join(root, name), cv2.IMREAD_GRAYSCALE)
                img_out = cv2.imread(os.path.join(root, name_out))
                img_out = cv2.cvtColor(img_out, cv2.COLOR_RGB2BGR)
                img_in = enhance(img_in)                                                            
                
                img_in = cv2.resize(img_in, IMG_SIZE[::-1])
                #img_in = np.reshape(img_in, IMG_SIZE + (1,))    

                img_out = cv2.resize(img_out, IMG_SIZE[::-1])                
                
                #print(img_out.max(), img_out.min())
                
                img_out = (img_out!=0).astype(np.float32)
                
                img_in = img_in/255.
                
                img_out = np.argmax(img_out, axis=-1)
                img_out = tf.keras.utils.to_categorical(img_out, 3)
                                
                yield img_in.reshape((*img_in.shape, 1)).astype(np.float32), img_out

def dynamic_cycle(f, *args):
    while True:
        for element in f(*args):
            yield element
            
    

def enumerate_datasets(datasets: list[tuple[str, str]], IMG_SIZE, limit=None):    
    count = 0
    while True:
        iterators = [dynamic_cycle(enumerate_dataset, path, enc, IMG_SIZE) for (path,enc) in datasets]
        
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
