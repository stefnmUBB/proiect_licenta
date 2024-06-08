import tensorflow as tf
import pickle



def create_recognition_model(img_size, labels_len):
    x = x_input = tf.keras.layers.Input(shape=(*img_size, 1), name='x_input')

    x = tf.keras.layers.Conv2D(64, kernel_size=(5, 5), padding='same')(x)
    x = tf.keras.layers.LeakyReLU(0.01)(x)
    x = tf.keras.layers.MaxPooling2D(pool_size=(2, 2))(x)

    x1 = tf.keras.layers.Conv2D(64, kernel_size=(5, 5), padding='same')(x)
    x = tf.keras.layers.Concatenate()([x, x1])
    x = tf.keras.layers.LeakyReLU(0.01)(x)
    x = tf.keras.layers.MaxPooling2D(pool_size=(2, 2))(x)

    x = tf.keras.layers.Conv2D(128, kernel_size=(3, 3), padding='same')(x)
    x = tf.keras.layers.LeakyReLU(0.01)(x)
    x = tf.keras.layers.MaxPooling2D(pool_size=(2, 2))(x)

    x = tf.keras.layers.Conv2D(256, kernel_size=(3, 3), padding='same')(x)
    x = tf.keras.layers.LeakyReLU(0.01)(x)

    x = tf.keras.layers.MaxPooling2D(pool_size=(2, 1))(x)

    x = tf.keras.layers.Conv2D(512, kernel_size=(3, 3), padding='same')(x)
    x = tf.keras.layers.BatchNormalization()(x)
    x = tf.keras.layers.LeakyReLU(0.01)(x)
    x = tf.keras.layers.MaxPooling2D(pool_size=(2, 1))(x)

    x = tf.keras.layers.Conv2D(512, kernel_size=(3, 3), padding='same')(x)    
    x = tf.keras.layers.LeakyReLU(0.01)(x)
    x = tf.keras.layers.Conv2D(512, kernel_size=(2, 1), strides=2)(x)    

    # ----- LSTM -----
    x = tf.keras.layers.Reshape((-1, 256))(x)

    x = tf.keras.layers.Bidirectional(tf.keras.layers.LSTM(128, return_sequences=True))(x)        

    x = tf.keras.layers.Reshape((1024, 1, -1))(x)
    x = tf.keras.layers.Conv2D(labels_len, kernel_size=(1, 1))(x)
    x = tf.keras.layers.Reshape((-1, labels_len))(x)
    x = tf.keras.layers.Softmax()(x)

    return tf.keras.models.Model(x_input, x)

class CTCLayer(tf.keras.layers.Layer):
    def __init__(self, **kwargs):
        super().__init__(**kwargs)

    def call(self, inputs):
        y_true, y_pred, logit_len, label_len = inputs
        return tf.keras.backend.ctc_batch_cost(y_true, y_pred, logit_len, label_len)

def ctc_layer(data):
    y_true, y_pred, logit_len, label_len = data
    return tf.keras.backend.ctc_batch_cost(y_true, y_pred, logit_len, label_len)

def create_train_model(model, SEQ_LEN):
    x_input = tf.keras.layers.Input(shape=model.inputs[0].shape[1:], name='img')
    y_true = tf.keras.layers.Input(shape=(SEQ_LEN,), name='y_true')
    label_len = tf.keras.layers.Input(shape=(1,), name="label_len")
    logits_len = tf.keras.layers.Input(shape=(1,), name="logits_len")

    y_pred = model(x_input)    

    #ctc_out = tf.keras.layers.Lambda(ctc_layer, name="ctc")([y_true, y_pred, logits_len, label_len])
    ctc_out = CTCLayer(name="ctc")([y_true, y_pred, logits_len, label_len])
    pred_out = tf.keras.layers.Identity(trainable=False, name="pred")(y_pred)
    return tf.keras.models.Model([x_input, y_true, label_len, logits_len], [ctc_out, pred_out])

def save_opt(model, name):    
    model.save_weights(f'{name}_weights.h5')
    symbolic_weights = getattr(model.optimizer, 'variables')
    weight_values = tf.keras.backend.batch_get_value(symbolic_weights)
    with open(f'{name}_optimizer.pkl', 'wb') as f:
        pickle.dump(weight_values, f)

def load_opt(model, name):     
    print(model.optimizer)
    model.load_weights(f'{name}_weights.h5')
    model.make_train_function()
    with open(f'{name}_optimizer.pkl', 'rb') as f:
        weight_values = pickle.load(f)      
    
    variables = dict([(str(i), weight_values[i]) for i in range(len(weight_values))])    
    model.optimizer.load_own_variables(variables)
