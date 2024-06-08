# C:\Users\Stefan\miniconda3\envs\kaggle\python.exe D:\Public\CNNLSTMLineSeg64\main2.py

import matplotlib


import itertools
from math import floor
import tensorflow as tf
#tf.keras.backend.set_floatx('float64')

import cv2
import numpy as np

from dataset import enumerate_datasets
from matplotlib import pyplot as plt

from generator import generator, dataset_generator, transform_generator, get_points
from metrics import CERMetric, WERMetric
from model import create_recognition_model, create_train_model, load_opt, save_opt

characters = " !\"#%&'()*+,-./0123456789:;>?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzĂÂÎȘȚăâîșț\n="
IMG_SIZE = (256,256)
SEQ_LEN = 1500
POLY_LEN = 40
PTS_LEN = 100
np.random.seed(2024)

ALL_PTS_LEN = 1500

def rotate_image(mat, angle):
    """
    Rotates an image (angle in degrees) and expands image to avoid cropping
    """    

    height, width = mat.shape[:2]  # image shape has 3 dimensions
    image_center = (width / 2,
                    height / 2)  # getRotationMatrix2D needs coordinates in reverse order (width, height) compared to shape

    rotation_mat = cv2.getRotationMatrix2D(image_center, angle, 1.)

    # rotation calculates the cos and sin, taking absolutes of those.
    abs_cos = abs(rotation_mat[0, 0])
    abs_sin = abs(rotation_mat[0, 1])

    # find the new width and height bounds
    bound_w = int(height * abs_sin + width * abs_cos)
    bound_h = int(height * abs_cos + width * abs_sin)

    # subtract old image center (bringing image back to origo) and adding the new image center coordinates
    rotation_mat[0, 2] += bound_w / 2 - image_center[0]
    rotation_mat[1, 2] += bound_h / 2 - image_center[1]

    # rotate image with the new bounds and translated rotation matrix
    rotated_mat = cv2.warpAffine(mat, rotation_mat, (bound_w, bound_h))
    return rotated_mat, rotation_mat
    

def augment2(i,o):
    #i,o = i.numpy(), o.numpy()      
    
    old_height, old_width = i.shape[:2]    
    
    resize_shape = (floor(i.shape[1]*(1+np.random.random()/8-0.25)), floor(i.shape[0]*((1+np.random.random()/8-0.25))))
    
    i = cv2.resize(i, resize_shape)
    o = cv2.resize(o, resize_shape)
    height, width = i.shape[:2]

    def move(): return floor(np.random.random()*min(width, height))*0.2

    p_input = np.float32([[0, 0], [width - 1, 0], [width - 1, height - 1], [0, height - 1]])
    p_output = np.float32([[move(), move()], [width - 1 - move(), move()],
                           [width - 1 - move(), height - 1 - move()], [move(), height - 1 - move()]])

    persp = cv2.getPerspectiveTransform(p_input, p_output)
    i = cv2.warpPerspective(i, persp, (width, height), cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT, borderValue=(0, 0, 0))
    o = cv2.warpPerspective(o, persp, (width, height), cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT, borderValue=(0, 0, 0))
    x0,y0,x1,y1 = np.min(p_output[:,0]), np.min(p_output[:,1]), np.max(p_output[:,0]), np.max(p_output[:,1])
    x0,y0,x1,y1 = map(floor, [x0,y0,x1,y1])

    i = i[y0:y1, x0:x1]
    o = o[y0:y1, x0:x1]
   
    a = np.random.random()*10-5
    i, rot_mat = rotate_image(i, a)            
    o, _ = rotate_image(o, a)            
        
    s = np.array([[width/old_width,0,0],[0,height/old_height,0],[0,0,1]], np.float32)
    t = np.array([[1,0,-x0],[0,1,-y0],[0,0,1]], np.float32)     
    
    t = np.matmul(np.concatenate([rot_mat, np.array([[0,0,1]])]), t)    
    
    o = np.argmax(o, axis=-1)
    o = tf.keras.utils.to_categorical(o, 3)
    o = (np.argmax(o,-1)!=0).astype(np.int32)
    o = o.reshape((*o.shape, 1))
    
    i = i.reshape((*i.shape, 1))
    
    noise = np.random.randn(*i.shape[:2])
    k = np.random.randn(11,11)
    noise = cv2.filter2D(noise, -1, cv2.flip(k,-1), borderType=cv2.BORDER_CONSTANT).clip(0,1)        
    noise = noise.reshape((*i.shape[:2], 1))
    f=np.random.rand()*0.1
    i=noise*f+i*(1-f)     
    
    o = o.astype(np.float32)
    
    return i, o

dataset_paths = [
       ("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\IAM", None),    
       ("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\Compuneri", None),    
       ("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\JL", None),    
       ("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\tmp", None),    
       #("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSeg\\IAM_fullpage", None),    
       #("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSeg\\IAM_lines", None),    
       #("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSeg\\JL" , None),    
       #("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSeg\\tmp", None),    
    ]


def main():   
    matplotlib.use('Agg')

    ds = enumerate_datasets(dataset_paths, IMG_SIZE, augment=augment2)
    ds = dataset_generator(ds, (64,64))

    #ds = enumerate_datasets(dataset_paths, IMG_SIZE)
    #ds = dataset_generator(ds, IMG_SIZE, POLY_LEN, PTS_LEN, augment2)    
    #ds = transform_generator(ds, lambda d:augment2(*d))    
    
    test_ds = enumerate_datasets([("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\test", None)], IMG_SIZE, augment=augment2)    
    test_ds = dataset_generator(test_ds, (64,64))
    
    q_ds = enumerate_datasets(dataset_paths, IMG_SIZE, augment=augment2)
    q_ds = dataset_generator(q_ds, (64,64))
    
    q_testds = enumerate_datasets([("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\test", None)], IMG_SIZE, augment=augment2)
    q_testds = dataset_generator(q_testds, (64,64))
       
    
    #test_ds = enumerate_datasets([("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\test", None)], IMG_SIZE)
    #test_ds = dataset_generator(test_ds, IMG_SIZE, POLY_LEN, PTS_LEN, augment2)    
    #test_ds = transform_generator(test_ds, lambda d:augment2(*d))               
    
    dst = ds    
    ds = ds.shuffle(500).batch(2)
    
    test_dst = test_ds    
    test_ds = test_ds.batch(5)

    def soft_argmax(x):
        beta=100
        x = tf.convert_to_tensor(x)        
        x_range = tf.range(x.shape.as_list()[-1], dtype=x.dtype)
        return tf.reduce_sum(tf.nn.softmax(x*beta) * x_range, axis=-1)
    
    TIMG_SIZE = (64,64)

    def create_segmentation_model():
        def enc_block(l, units):
            l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'random_normal')(l)
            l = tf.keras.layers.Activation("leaky_relu")(l)
            #l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'he_normal')(l)
            #l = tf.keras.layers.Activation("leaky_relu")(l)            
            l = tf.keras.layers.MaxPooling2D()(l)              
            l = tf.keras.layers.Dropout(0.25/units)(l)
            return l
            
        def dec_block(l1, l2, units):
            l1 = tf.keras.layers.Conv2DTranspose(units, kernel_size=3, strides=2,padding="same", kernel_initializer = 'he_normal')(l1)
            l2 = tf.keras.layers.UpSampling2D()(l2)
            l = tf.keras.layers.Concatenate()([l1, l2])
            l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'he_normal')(l)
            l = tf.keras.layers.Activation("leaky_relu")(l)      
            #l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'he_normal')(l)
            #l = tf.keras.layers.Activation("leaky_relu")(l)
            return l
                    
        xi = x = tf.keras.layers.Input(TIMG_SIZE+(1,))                
        
        x = x1 = enc_block(x, 4)
        x = x2 = enc_block(x, 8)
        x = x3 = enc_block(x, 16)               
        x = x4 = enc_block(x, 32)               
        x = x5 = enc_block(x, 64)               
        #x = x6 = enc_block(x, 256)               
            
        
        x = tf.keras.layers.Reshape((64,4))(x)    
        x = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(2, return_sequences=True, dropout=0.025))(x)        
        x = tf.keras.layers.Reshape((2,2,64))(x)      
        
        #x = dec_block(x, x6, 256)
        x = dec_block(x, x5, 64)
        x = dec_block(x, x4, 32)
        x = dec_block(x, x3, 16)
        x = dec_block(x, x2, 8)
        x = dec_block(x, x1, 4)
        
        x = tf.keras.layers.Conv2D(1, kernel_size=3, padding="same", activation="sigmoid")(x)
        return tf.keras.Model(xi,x)

    #"""
    model = create_segmentation_model()
    model.summary()   
    
    ce = tf.keras.losses.BinaryCrossentropy(from_logits=False)
    
    def loss(y_true,y_pred):
        l = ce(y_true, y_pred)        
        vars   = model.trainable_weights
        lossL2 = tf.add_n([ tf.nn.l2_loss(v) for v in vars ]) * 0.00002        
        return l+lossL2
    

    model.compile(optimizer= tf.keras.optimizers.Adam(learning_rate = 1e-3), 
                  loss=loss, metrics="accuracy")
    #model.compile(optimizer= tf.keras.optimizers.Adam(learning_rate = 1e-3), loss=tf.keras.losses.MeanSquaredError(), metrics="accuracy")


    def callback_plot(epoch, logs):
        if epoch % 1 == 0:
            n = 4
            fig, ax = plt.subplots(3, n)
                   
            k=0
            for x, y0 in list(itertools.islice(q_ds,0, n-1)) + list(itertools.islice(q_testds, 0, 1)):
                x = x.numpy()                                        
                x = x.reshape(1, *TIMG_SIZE, 1)           
                
                y = model.predict(x)[0]
                x = x[0]
                                                                
                x = np.concatenate([x,x,x], axis=-1)
                z = np.zeros(y.shape)
                x = 0.4*x + 0.6*np.concatenate([z,y,z], -1) 
                #x = np.clip(x+y, 0, 1)            
                ax[0,k].imshow(x)
                ax[0,k].axis('off')
    
                ax[1,k].imshow(np.concatenate([y,y,y],-1))
                ax[1,k].axis('off')
                
                ax[2,k].imshow(np.concatenate([y0,y0,y0], -1))
                ax[2,k].axis('off')
                
                k+=1            
                
            plt.savefig(f"D:\\Public\\CNNLSTMLineSeg64\\figures\\ctc_{epoch}_{logs['loss']}.png")
            plt.close()       
            
        
        if epoch % 2 == 0:                
            #save_opt(train_model, "D:\\Public\\CNNLSTMFullPage\\train_model_0")
            model.save("D:\\Public\\CNNLSTMLineSeg64\\train_model_2")



    history = model.fit(ds, epochs=2000, steps_per_epoch=400,
        validation_data = test_ds,
        validation_steps = 6,
        callbacks=[tf.keras.callbacks.LambdaCallback(on_epoch_end=callback_plot), tf.keras.callbacks.ReduceLROnPlateau(monitor='loss', factor=0.8, patience=2, cooldown=10, min_lr=1e-6, mode="min")
])

main()


def disp():    
    imgs = []
    ds = enumerate_datasets(dataset_paths, IMG_SIZE, limit=None, augment=augment2)
    gen = dataset_generator(ds, (64,64))
    
    #gen = transform_generator(gen, lambda d:augment2(*d))    
    
    k=0
    for i,o in gen:    
        print(k)
        k+=1
        #print("i:",i.shape)
        #i = np.concatenate([i,i,i], axis=-1)
        #plt.imshow(i)
        #plt.imshow(i*0.5+o*0.5)
        #print(i.shape, o.shape)
        #plt.imshow(np.concatenate([i,o], axis=1))
        #plt.show()
       

disp()