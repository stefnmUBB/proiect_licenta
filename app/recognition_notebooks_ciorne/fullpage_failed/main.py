import matplotlib
matplotlib.use('Agg')

import itertools
from math import floor
import tensorflow as tf

import cv2
import numpy as np

from dataset import enumerate_datasets
from matplotlib import pyplot as plt

from generator import generator, dataset_generator
from metrics import CERMetric, WERMetric
from model import create_recognition_model, create_train_model, load_opt, save_opt

characters = " !\"#%&'()*+,-./0123456789:;>?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzĂÂÎȘȚăâîșț\n="
IMG_SIZE = (512,512)
SEQ_LEN = 1024
np.random.seed(2024)

dataset_paths = [
    ("D:\\Users\\Stefan\\Datasets\\hw_flex\\IAM_full", None),
    ("D:\\Users\\Stefan\\Datasets\\hw_flex\\Compuneri", "utf8"),
    ("D:\\Users\\Stefan\\Datasets\\hw_flex\\JL", "utf8")
]

def augment(img):
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
        return rotated_mat
    img = cv2.resize(img, (
        floor(img.shape[1]*(1+np.random.random()/4-0.5)),
        floor(img.shape[0]*((1+np.random.random()/4-0.5)))
    ))

    height, width = img.shape

    def move(): return floor(np.random.random()*min(width, height))*0.2

    p_input = np.float32([[0, 0], [width - 1, 0], [width - 1, height - 1], [0, height - 1]])
    p_output = np.float32([[move(), move()], [width - 1 - move(), move()],
                           [width - 1 - move(), height - 1 - move()], [move(), height - 1 - move()]])

    persp = cv2.getPerspectiveTransform(p_input, p_output)
    img = cv2.warpPerspective(img, persp, (width, height), cv2.INTER_LINEAR, borderMode=cv2.BORDER_CONSTANT,
                                    borderValue=(0, 0, 0))
    x0,y0,x1,y1 = np.min(p_output[:,0]), np.min(p_output[:,1]), np.max(p_output[:,0]), np.max(p_output[:,1])
    x0,y0,x1,y1 = map(floor, [x0,y0,x1,y1])

    img = img[y0:y1, x0:x1]

    img = rotate_image(img, np.random.random()*10-5)

    return img

ds = enumerate_datasets(dataset_paths, (512, 512), characters, SEQ_LEN, augment=augment)
ds = dataset_generator(ds, IMG_SIZE, SEQ_LEN, len(characters))
ds = ds.shuffle(150).batch(2)

recognition_model = create_recognition_model(IMG_SIZE, len(characters)+1)
recognition_model.summary(110)

train_model = create_train_model(recognition_model, SEQ_LEN)

class PredLoss(tf.keras.losses.Loss):
    def __init__(self, reduction = "auto", name="pred_loss"):
        super().__init__(reduction=reduction, name=name)
    def call(self, y_true, y_pred):
        return y_pred
        

train_model.compile(optimizer="rmsprop",
                    loss={'ctc': PredLoss(name='ctc')},
                    metrics={'ctc':"accuracy", 'pred':[CERMetric(),WERMetric()]})
                    
history = train_model.fit(ds, shuffle=False, epochs=1, steps_per_epoch=1)
#load_opt(train_model, "D:\\Public\\CNNLSTMFullPage\\train_model_3")
                    

#train_model.summary()
#train_model.get_layer('model').summary()

def callback_plot(epoch, logs):
    if epoch % 1 == 0:
        d = enumerate_datasets(dataset_paths, (512, 512), characters, SEQ_LEN, augment=augment, limit=5)
        d = list(d)
        in_img = np.array([x[0] for x in d])
        in_true = np.array([x[1] for x in d])

        out_pred = recognition_model.predict(in_img)
        seq_length = recognition_model.output.shape[1]
        y, _ = tf.keras.backend.ctc_decode(out_pred, np.array([seq_length] * out_pred.shape[0]), greedy=True)
        y = y[0].numpy()


        for i in range(1):
            img = in_img[i]
            true = in_true[i]
            true = "".join([characters[c] for c in true if c < len(characters)])[:50]+"..."
            pred = out_pred[i]
            ptxt = "".join([characters[c] for c in y[i] if 0 <= c < len(characters)])[:50]+"..."

        fig, axs = plt.subplots(2)
        fig.suptitle(true+"\n"+ptxt)
        axs[0].axis("off")
        axs[0].imshow(img)

        def label2char(l):
            c = characters[l] if l < len(characters) else "ε"
            if c == " ": c = "\\s"
            return c

        for i in range(len(characters) + 1):
            yc = pred[:, i]
            im = np.argmax(yc)
            axs[1].plot(yc)
            plt.text(im, yc[im], label2char(i))                  
        
        plt.savefig(f"D:\\Public\\CNNLSTMFullPage\\figures\\ctc_{epoch}_{logs['loss']}_{logs['pred_CER_metric']}.png")
        plt.close()

        with open(f"D:\\Public\\CNNLSTMFullPage\\figures\\prd_{epoch}.txt","w",encoding="utf8") as f:
            for i in range(len(d)):
                true = "".join([characters[c] for c in in_true[i] if c < len(characters)])
                ptxt = "".join([characters[c] for c in y[i] if 0 <= c < len(characters)])
                f.write(f"Real: {true}\r\nPredicted: {ptxt}\r\n\r\n")

    if epoch % 2 == 0:        
        #save_opt(train_model, "D:\\Public\\CNNLSTMFullPage\\train_model_0")
        train_model.save("D:\\Public\\CNNLSTMFullPage\\train_model_0")


train_model = tf.keras.models.load_model("D:\\Public\\CNNLSTMFullPage\\train_model_0", {"CERMetric":CERMetric(), "WERMetric":WERMetric(), "PredLoss":PredLoss(name='ctc')})

history = train_model.fit(ds,
                          shuffle=False, epochs=1000,
                          steps_per_epoch=200,
                          callbacks=[tf.keras.callbacks.LambdaCallback(on_epoch_end=callback_plot)])