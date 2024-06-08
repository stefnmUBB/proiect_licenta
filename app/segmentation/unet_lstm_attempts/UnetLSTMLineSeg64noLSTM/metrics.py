import tensorflow as tf

class CERMetric(tf.keras.metrics.Metric):
    """
    A custom Keras metric to compute the Character Error Rate
    """

    def __init__(self, name='CER_metric', **kwargs):
        super(CERMetric, self).__init__(name=name, **kwargs)
        self.cer_accumulator = self.add_weight(name="total_cer", initializer="zeros")
        self.counter = self.add_weight(name="cer_count", initializer="zeros")

    def update_state(self, y_true, y_pred, sample_weight=None):
        pred_input_shape = tf.keras.backend.shape(y_pred)
        pred_input_length = tf.ones(shape=pred_input_shape[0]) * tf.keras.backend.cast(pred_input_shape[1], 'float32')
        true_input_shape = tf.keras.backend.shape(y_true)
        true_input_length = tf.ones(shape=true_input_shape[0]) * tf.keras.backend.cast(true_input_shape[1], 'float32')

        pred, _ = tf.keras.backend.ctc_decode(y_pred, pred_input_length, greedy=True)
        true, _ = tf.keras.backend.ctc_decode(y_true, true_input_length, greedy=True)

        pred, true = pred[0], true[0]

        pred_length = tf.keras.backend.cast(tf.math.argmin(pred, axis=1), 'int32')
        true_length = tf.keras.backend.cast(tf.math.argmin(true, axis=1), 'int32')

        pred = tf.keras.backend.ctc_label_dense_to_sparse(pred, pred_length)
        true = tf.keras.backend.ctc_label_dense_to_sparse(true, true_length)

        distance = tf.edit_distance(pred, true, normalize=True)

        self.cer_accumulator.assign_add(tf.reduce_sum(distance))
        self.counter.assign_add(tf.cast(pred_input_shape[0], 'float32'))

    def result(self):
        return tf.math.divide_no_nan(self.cer_accumulator, self.counter)

    def reset_state(self):
        self.cer_accumulator.assign(0.0)
        self.counter.assign(0.0)


class WERMetric(tf.keras.metrics.Metric):
    """
    A custom Keras metric to compute the Word Error Rate
    """

    def __init__(self, name='WER_metric', **kwargs):
        super(WERMetric, self).__init__(name=name, **kwargs)
        self.wer_accumulator = self.add_weight(name="total_wer", initializer="zeros")
        self.counter = self.add_weight(name="wer_count", initializer="zeros")

    def update_state(self, y_true, y_pred, sample_weight=None):
        pred_input_shape = tf.keras.backend.shape(y_pred)
        pred_input_length = tf.ones(shape=pred_input_shape[0]) * tf.keras.backend.cast(pred_input_shape[1], 'float32')
        true_input_shape = tf.keras.backend.shape(y_true)
        true_input_length = tf.ones(shape=true_input_shape[0]) * tf.keras.backend.cast(true_input_shape[1], 'float32')

        pred, _ = tf.keras.backend.ctc_decode(y_pred, pred_input_length, greedy=True)
        true, _ = tf.keras.backend.ctc_decode(y_true, true_input_length, greedy=True)

        pred, true = pred[0], true[0]

        pred_length = tf.keras.backend.cast(tf.math.argmin(pred, axis=1), 'int32')
        true_length = tf.keras.backend.cast(tf.math.argmin(true, axis=1), 'int32')

        pred = tf.keras.backend.ctc_label_dense_to_sparse(pred, pred_length)
        true = tf.keras.backend.ctc_label_dense_to_sparse(true, true_length)

        distance = tf.edit_distance(pred, true, normalize=True)

        red_y_true = tf.argmax(y_true, axis=2)

        spaces_mask = tf.equal(red_y_true, tf.zeros((), dtype=tf.int64))
        spaces_count = tf.reduce_sum(tf.cast(spaces_mask, tf.int32), axis=1)

        words_count = tf.add(spaces_count, tf.ones_like(spaces_count))
        words_count = tf.reduce_sum(words_count)

        self.wer_accumulator.assign_add(tf.reduce_sum(distance))
        self.counter.assign_add(tf.cast(words_count, 'float32'))

    def result(self):
        return tf.math.divide_no_nan(self.wer_accumulator, self.counter)

    def reset_state(self):
        self.wer_accumulator.assign(0.0)
        self.counter.assign(0.0)