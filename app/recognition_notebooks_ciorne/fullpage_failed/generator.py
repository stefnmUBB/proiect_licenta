import numpy as np
import tensorflow as tf


def generator(img_txt_iter, logit_len, chars_count):
    logit_len = np.array(logit_len)
    for x, y_true, y_len in img_txt_iter:
        if y_len >= logit_len:
            print(f"Text too long ({y_len})")
        y_true_categorical = tf.keras.utils.to_categorical([y_true], num_classes=chars_count+1)[0]
        yield (x, y_true, np.array([y_len]), np.array([logit_len])), (np.zeros((1,)), y_true_categorical)


def dataset_generator(img_txt_iter, img_size, logit_len, chars_count):
    return tf.data.Dataset.from_generator(lambda: generator(img_txt_iter, logit_len, chars_count),
                                          output_types=((tf.float32, tf.int32, tf.int32, tf.int32),(tf.float32, tf.float32)),
                                          output_shapes=((img_size+(1,), (logit_len,), (1,), (1,)), ((1,), (logit_len, chars_count+1))))