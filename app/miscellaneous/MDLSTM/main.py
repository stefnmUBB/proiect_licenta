import tensorflow as tf
import tensorboard

from mdlstm import MDLSTM2D, QuDirectionalMDLSTM

mnist = tf.keras.datasets.mnist
(x_train, y_train), (x_test, y_test) = mnist.load_data()
x_train, x_test = x_train / 255.0, x_test / 255.0
sample, sample_label = x_train[0], y_train[0]
print(x_train.shape)
print(y_train.shape)

model = tf.keras.models.Sequential([
    tf.keras.layers.Input(x_train.shape[1:]),
    tf.keras.layers.Reshape((28,28,1)),
    tf.keras.layers.Conv2D(8, kernel_size=3, strides=2),
    QuDirectional(units = 32),
    tf.keras.layers.Flatten(),
    tf.keras.layers.Dense(10, activation="softmax")
])

model.summary()
model.compile(optimizer="adam", loss="mse", metrics="accuracy")
model.fit(x_train, tf.keras.utils.to_categorical(y_train), epochs=5, validation_data=(x_test, tf.keras.utils.to_categorical(y_test)))

