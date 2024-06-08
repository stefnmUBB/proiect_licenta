from typing import Callable
import tensorflow as tf


class MDLSTM2DCell(tf.keras.layers.Layer):
    def __init__(self, units, **kwargs):
        self.units = units
        #self.dense_f0 = tf.keras.layers.Dense(self.units, activation="sigmoid", use_bias=True)
        #self.dense_f1 = tf.keras.layers.Dense(self.units, activation="sigmoid", use_bias=True)

        super().__init__(**kwargs)

    def build(self, input_shape):
        input_shape, current_shape, hidden_shape = input_shape
        input_unit_size = input_shape[-1]
        hidden_units_size = self.units

        def gen_weights(suffix):
            w = self.add_weight(shape=(input_unit_size, hidden_units_size),
                                 initializer="random_normal", trainable=True, name=f"w{suffix}")
            u = self.add_weight(shape=(hidden_units_size, hidden_units_size),
                                      initializer="random_normal", trainable=True, name=f"u{suffix}")
            b = self.add_weight(shape=(hidden_units_size,), initializer="zeros", trainable=True, name=f"b{suffix}")
            return w, u, b

        self.wf0, self.uf0, self.bf0 = gen_weights('f0')
        self.wf1, self.uf1, self.bf1 = gen_weights('f1')
        self.wi, self.ui, self.bi = gen_weights('i')
        self.wo, self.uo, self.bo = gen_weights('o')
        self.wc, self.uc, self.bc = gen_weights('c')

        super(MDLSTM2DCell, self).build(input_shape)

    @tf.function#(experimental_compile=True)
    def call(self, inputs, **kwargs):
        inputs, c0, c1, h0, h1 = inputs

        f0 = tf.keras.activations.sigmoid(tf.matmul(inputs, self.wf0) + tf.matmul(h0, self.uf0) + self.bf0)
        f1 = tf.keras.activations.sigmoid(tf.matmul(inputs, self.wf1) + tf.matmul(h1, self.uf1) + self.bf1)

        it = tf.keras.activations.sigmoid(tf.matmul(inputs, self.wi) + tf.matmul(h0, self.ui) + tf.matmul(h1, self.ui) + self.bi)
        ot = tf.keras.activations.sigmoid(tf.matmul(inputs, self.wo) + tf.matmul(h0, self.uo) + tf.matmul(h1, self.uo) + self.bo)
        ctpred = tf.keras.activations.tanh(tf.matmul(inputs, self.wc) + tf.matmul(h0, self.uc) + tf.matmul(h1, self.uc) + self.bc) # c~
        ct = tf.multiply(f0, c0) + tf.multiply(f1, c1) + tf.multiply(it, ctpred)
        ht = tf.multiply(ot, tf.keras.activations.tanh(ct))
        return ct, ht

class MDLSTM2D(tf.keras.layers.Layer):
    @staticmethod
    def __dir2crd(d, inc, dec):
        if d == inc: return 1
        if d == dec: return -1
        raise Exception(f"Invalid direction: '{d}'. Must be '{inc}' or '{dec}'")

    def __init__(self, units=32, dir_x="right", dir_y="down", **kwargs):
        self.units = units
        self.delta0 = self.__dir2crd(dir_x, "right", "left")
        self.delta1 = self.__dir2crd(dir_y, "down", "up")
        super().__init__(**kwargs)

    def build(self, input_shape):
        batch_size = input_shape[0]
        self.D0 = tf.constant(input_shape[1], shape=())
        self.D1 = tf.constant(input_shape[2], shape=())

        state_shape = (batch_size, self.units)
        self.cell = MDLSTM2DCell(self.units)
        self.cell.build([(batch_size, input_shape[-1]), state_shape, state_shape])

        super(MDLSTM2D, self).build(input_shape)

    @tf.function
    def call(self, inputs, **kwargs):
        batch_size = tf.shape(inputs)[0]
        state_shape = (batch_size, self.units)

        if self.delta0 < 0: inputs = tf.reverse(inputs, axis=[1])
        if self.delta1 < 0: inputs = tf.reverse(inputs, axis=[2])

        max_iter = self.D0 * self.D1

        inputs = tf.transpose(inputs, [1, 2, 0, 3])
        result_h = tf.TensorArray(tf.float32, size=max_iter+1, infer_shape=True)
        cache_c = tf.TensorArray(tf.float32, size=self.D1+1, infer_shape=True)

        result_h = result_h.write(max_iter, tf.zeros(state_shape))
        cache_c = cache_c.write(self.D1, tf.zeros(state_shape))

        for d0 in range(self.D0):
            for d1 in tf.range(self.D1):
                d = d0*self.D1+d1
                h0 = result_h.read(tf.cond(0 < d0, lambda: d - self.D1, lambda: max_iter))
                h1 = result_h.read(tf.cond(0 < d1, lambda: d - 1, lambda: max_iter))

                c0 = cache_c.read(tf.cond(0 < d0, lambda: d1, lambda: self.D1))
                c1 = cache_c.read(tf.cond(0 < d1, lambda: d1 - 1, lambda: self.D1))

                c, h = self.cell([inputs[d0, d1], c0, c1, h0, h1], training=kwargs['training'])
                result_h = result_h.write(d, h)
                cache_c = cache_c.write(d1, c)

        o = tf.transpose(tf.slice(result_h.stack(), [0, 0, 0], [max_iter, batch_size, self.units]), [1, 0, 2])
        return tf.reshape(o, (batch_size, self.D0, self.D1, tf.shape(o)[-1]))

class QuDirectionalMDLSTM(tf.keras.layers.Layer):
    def __init__(self, units=32, concat=True, **kwargs):
        self.mdrnns = [MDLSTM2D(units) for _ in range(4)]
        self.concat=concat
        super().__init__(**kwargs)

    def build(self, input_shape):
        for mdrnn in self.mdrnns: mdrnn.build(input_shape)
        super(QuDirectionalMDLSTM, self).build(input_shape)

    @tf.function
    def call(self, inputs, **kwargs):
        i0 = self.mdrnns[0](inputs, training=kwargs['training'])
        i1 = self.mdrnns[1](tf.reverse(inputs, axis=[1]), training=kwargs['training'])
        i2 = self.mdrnns[2](tf.reverse(inputs, axis=[2]), training=kwargs['training'])
        i3 = self.mdrnns[3](tf.reverse(inputs, axis=[1,2]), training=kwargs['training'])
        if self.concat:
            return tf.concat([i0,i1,i2,i3], axis=-1)
        return tf.stack([i0,i1,i2,i3], axis=3)
