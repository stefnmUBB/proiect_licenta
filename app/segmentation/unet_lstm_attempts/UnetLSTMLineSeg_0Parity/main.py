import matplotlib


import itertools
from math import floor
import tensorflow as tf
tf.keras.backend.set_floatx('float64')

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
    i,o = i.numpy(), o.numpy()      
    
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
                   
    i = cv2.resize(i, IMG_SIZE[::-1], interpolation = cv2.INTER_AREA)
    o = cv2.resize(o, IMG_SIZE[::-1], interpolation = cv2.INTER_AREA)    
    
    o = np.argmax(o, axis=-1)
    o = tf.keras.utils.to_categorical(o, 3)
    
    return i.reshape((*IMG_SIZE, 1)), o

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

    ds = enumerate_datasets(dataset_paths, IMG_SIZE)
    ds = dataset_generator(ds, IMG_SIZE, POLY_LEN, PTS_LEN, augment2)    
    ds = transform_generator(ds, lambda d:augment2(*d))    
    
    test_ds = enumerate_datasets([("D:\\Users\\Stefan\\Datasets\\hw_flex\\LineSegRaster\\test", None)], IMG_SIZE)
    test_ds = dataset_generator(test_ds, IMG_SIZE, POLY_LEN, PTS_LEN, augment2)    
    test_ds = transform_generator(test_ds, lambda d:augment2(*d))        
    
    def prepare_for_training(data):
        x,points,indexed_poly = data                              
        x = cv2.GaussianBlur(x, (3, 3), cv2.BORDER_DEFAULT)               
            
        y = indexed_poly.reshape((*(indexed_poly.shape), 1))                
        y = np.apply_along_axis(lambda t: points[t[0]] if t>=0 else np.array([-1,-1]), axis=2, arr=y)                
        
        h, w = x.shape
        
        polys = []
        for i in range(len(y)):
            poly = y[i]        
            l = min(np.argmax(poly<0, axis=0))        
            if l>2:
                polys.append(poly[:l])      

        t=2
        yy = np.zeros(x.shape)
        for poly in polys:
            yy = cv2.fillPoly(yy, [poly], t)
            t=3-t
        
        #plt.imshow(yy)
        #plt.savefig(f"D:\\Public\\CNNLSTMLineSeg\\figures\\test.png")
        #plt.close()
        
        """
        yy = np.zeros((*IMG_SIZE, 1))
        
        for point in y:
            point = point.astype(np.int32)
            if point[0]<0 or point[1]<0: continue
            yy = cv2.circle(yy,tuple(point),2,(255,0,0))    
        """
        
        return np.reshape(x, (*IMG_SIZE, 1)), tf.keras.utils.to_categorical(np.reshape(yy, (*IMG_SIZE, 1)).astype(np.int32), num_classes=3)
        #return x, tf.keras.utils.to_categorical(y.clip(-1, 512), num_classes=514)
    
    dst = ds
    #ds = dst = transform_generator(ds, lambda d:prepare_for_training(get_points(d, ALL_PTS_LEN))) # ignore indexed_poly                    
    ds = ds.shuffle(150).batch(2)
    
    test_dst = test_ds
    #test_ds = test_dst = transform_generator(test_ds, lambda d:prepare_for_training(get_points(d, ALL_PTS_LEN))) # ignore indexed_poly                    
    test_ds = test_ds.batch(5)

    def soft_argmax(x):
        beta=100
        x = tf.convert_to_tensor(x)        
        x_range = tf.range(x.shape.as_list()[-1], dtype=x.dtype)
        return tf.reduce_sum(tf.nn.softmax(x*beta) * x_range, axis=-1)


    def create_segmentation_model():
        def enc_block(l, units):
            l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'random_normal')(l)
            l = tf.keras.layers.Activation("leaky_relu")(l)
            l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'he_normal')(l)
            l = tf.keras.layers.Activation("leaky_relu")(l)            
            l = tf.keras.layers.MaxPooling2D()(l)              
            l = tf.keras.layers.Dropout(1/units)(l)
            return l
            
        def dec_block(l1, l2, units):
            l1 = tf.keras.layers.Conv2DTranspose(units, kernel_size=3, strides=2,padding="same", kernel_initializer = 'he_normal')(l1)
            l2 = tf.keras.layers.UpSampling2D()(l2)
            l = tf.keras.layers.Concatenate()([l1, l2])
            l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'he_normal')(l)
            l = tf.keras.layers.Activation("leaky_relu")(l)      
            l = tf.keras.layers.Conv2D(units, kernel_size=3, padding="same", kernel_initializer = 'he_normal')(l)
            l = tf.keras.layers.Activation("leaky_relu")(l)
            return l
            
    
        xi = x = tf.keras.layers.Input(IMG_SIZE+(1,))                
        
        x = x1 = enc_block(x, 16)
        x = x2 = enc_block(x, 32)
        x = x3 = enc_block(x, 64)               
        x = x4 = enc_block(x, 128)               
        x = x5 = enc_block(x, 256)               
        #x = x6 = enc_block(x, 512)               
        
        # , kernel_regularizer=tf.keras.regularizers.L1L2(l1=1e-5, l2=1e-4)
        
        x = tf.keras.layers.Reshape((64,256))(x)
        #x = tf.keras.layers.Conv2D(512, (3,3), padding="same", activation="leaky_relu")(x)        
        x = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(256, return_sequences=True, dropout=0.1))(x)
        
        x = tf.keras.layers.Reshape((8,8,512))(x)
        
        #x = tf.keras.layers.Conv2D(512, kernel_size=3, padding="same", activation="tanh", kernel_initializer = 'he_normal')(x)
        #x = tf.keras.layers.Conv2D(512, kernel_size=3, padding="same", activation="tanh", kernel_initializer = 'he_normal')(x)
        #x = tf.keras.layers.Dropout(0.05)(x)
        
        #x = dec_block(x, x6, 512)
        x = dec_block(x, x5, 256)
        x = dec_block(x, x4, 128)
        x = dec_block(x, x3, 64)
        x = dec_block(x, x2, 32)
        x = dec_block(x, x1, 16)
        
        x = tf.keras.layers.Conv2D(3, kernel_size=3, padding="same", activation="softmax")(x)
        return tf.keras.Model(xi,x)
    #"""
    model = create_segmentation_model()
    model.summary()   

    model.compile(optimizer= tf.keras.optimizers.Adam(learning_rate = 1e-3), loss=tf.keras.losses.CategoricalCrossentropy(), metrics="accuracy")


    def callback_plot(epoch, logs):
        if epoch % 1 == 0:
            n = 4
            fig, ax = plt.subplots(3, n)
                   
            k=0
            for x, y0 in list(itertools.islice(dst,0, n-1)) + list(itertools.islice(test_dst, 0, 1)):
                x = x.numpy()                                        
                x = x.reshape(1, *IMG_SIZE, 1)            
                
                y = model.predict(x)[0]
                x = x[0]
                                                
                
                x = np.concatenate([x,x,x], axis=-1)
                x = 0.4*x + 0.6*y               
                #x = np.clip(x+y, 0, 1)            
                ax[0,k].imshow(x)
                ax[0,k].axis('off')
    
                ax[1,k].imshow(y)
                ax[1,k].axis('off')
                
                ax[2,k].imshow(y0)
                ax[2,k].axis('off')
                
                k+=1            
                
            plt.savefig(f"D:\\Public\\CNNLSTMLineSeg\\figures\\ctc_{epoch}_{logs['loss']}.png")
            plt.close()       
            
        
        if epoch % 2 == 0:                
            #save_opt(train_model, "D:\\Public\\CNNLSTMFullPage\\train_model_0")
            model.save("D:\\Public\\CNNLSTMLineSeg\\train_model_0")



    history = model.fit(ds, epochs=2000, steps_per_epoch=20,
        validation_data = test_ds,
        validation_steps = 4,
        callbacks=[tf.keras.callbacks.LambdaCallback(on_epoch_end=callback_plot), tf.keras.callbacks.ReduceLROnPlateau(monitor='loss', factor=0.8, patience=2, cooldown=10, min_lr=1e-6, mode="min")
])

main()


def disp():    
    imgs = []
    gen = dataset_generator(enumerate_datasets(dataset_paths, IMG_SIZE, limit=10), IMG_SIZE, POLY_LEN, PTS_LEN, augment2)         
    
    #gen = transform_generator(gen, lambda d:augment2(*d))    
    
    for i,o in gen:
        i = np.concatenate([i,i,i], axis=-1)
        #plt.imshow(i)
        #plt.imshow(i*0.5+o*0.5)
        
        plt.imshow(np.concatenate([i,o], axis=1))
        plt.show()
       

#disp()