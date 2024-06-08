import tensorflow as tf

IMG_SIZE = (64,64)

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
    
    x = x1 = enc_block(x, 8)
    x = x2 = enc_block(x, 16)
    x = x3 = enc_block(x, 32)               
    x = x4 = enc_block(x, 64)               
    x = x5 = enc_block(x, 128)               
    #x = x6 = enc_block(x, 256)               
        
    
    x = tf.keras.layers.Reshape((1,512))(x)    
    x = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(256, return_sequences=True, dropout=0.1))(x)
    
    x = tf.keras.layers.Reshape((2,2,128))(x)      
    
    #x = dec_block(x, x6, 256)
    x = dec_block(x, x5, 128)
    x = dec_block(x, x4, 64)
    x = dec_block(x, x3, 32)
    x = dec_block(x, x2, 16)
    x = dec_block(x, x1, 8)
    
    x = tf.keras.layers.Conv2D(3, kernel_size=3, padding="same", activation="softmax")(x)
    return tf.keras.Model(xi,x)
#"""
model = create_segmentation_model()
model.summary()   
model.save_weights("x.h5")