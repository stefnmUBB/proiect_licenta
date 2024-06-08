import os
import cv2
import numpy as np
import itertools
import struct
import tensorflow as tf

from utils import enhance, text_to_labels

q=0

def get_tiles(img, tsize, seed):
    ty=tx=0
    result = []
    h,w = img.shape[:2]
    tt = tsize
    while ty<h:
        tx=0
        while tx<w:
            th, tw = min(ty+tsize, h), min(tx+tsize,w)               
            tile = img[ty:th, tx:tw,:]
            if tile.shape[0]<tsize or tile.shape[1]<tsize:
                tile = np.pad(tile, ((0, tsize-tile.shape[0]),(0,tsize-tile.shape[1]),(0,0)), 'constant', constant_values=0)                            
            result.append(tile)
            tx+=tsize//2
        ty+=tsize//2
    #print(len(result))
    
    for i in range(20):
        x0 = seed[i][0] #np.random.randint(0,w-tt)
        y0 = seed[i][0] #np.random.randint(0,h-tt)
        tile = img[x0:x0+tt, y0:y0+tt,:]
        result.append(tile)
        
    
    return result

def enumerate_dataset(dataset_path, encoding, IMG_SIZE, augment=None):
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
                #img_in = enhance(img_in)                                                            
                                
                               
                #img_out = cv2.resize(img_out, IMG_SIZE[::-1])                
                
                #print(img_out.max(), img_out.min())
                
                img_out = (img_out!=0).astype(np.float32)
                
                img_in = img_in/255.
                
                img_out = np.argmax(img_out, axis=-1)
                img_out = tf.keras.utils.to_categorical(img_out, 3)
                      
                img_in = img_in.reshape((*img_in.shape, 1)).astype(np.float32)
                
                if augment is not None:
                    img_in, img_out = augment(img_in, img_out)
                                
                img_in = cv2.resize(img_in, IMG_SIZE[::-1])                
                img_in = np.reshape(img_in, IMG_SIZE + (1,))                                    
                
                img_out = cv2.resize(img_out, IMG_SIZE[::-1])                
                img_out = np.reshape(img_out, IMG_SIZE + (1,))           

                seed = [(np.random.randint(0,img_in.shape[1]-64), np.random.randint(0,img_in.shape[0]-64)) for _ in range(20)]
                
                for ti, to in list(zip(get_tiles(img_in, 64, seed), get_tiles(img_out, 64, seed))):
                    #print(ti.shape, to.shape)
                    yield ti, to
                      
                #yield img_in, img_out

def dynamic_cycle(f, *args):
    while True:
        for element in f(*args):
            yield element
            
    

def enumerate_datasets(datasets: list[tuple[str, str]], IMG_SIZE, limit=None, augment=None):    
    count = 0
    while True:
        iterators = [dynamic_cycle(enumerate_dataset, path, enc, IMG_SIZE, augment) for (path,enc) in datasets]
        
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
