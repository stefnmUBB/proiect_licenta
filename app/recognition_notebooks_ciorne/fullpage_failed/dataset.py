import os
import cv2
import numpy as np
import itertools

from utils import enhance, text_to_labels


def enumerate_dataset(dataset_path, encoding, IMG_SIZE, characters, SEQ_LEN, augment=lambda x:x):
    for root, dirs, files in os.walk(dataset_path, topdown=False):
        for name in files:
            if name.endswith(".png"):
                img = cv2.imread(os.path.join(root, name), cv2.IMREAD_GRAYSCALE)
                img = augment(enhance(img))
                img = cv2.resize(img, IMG_SIZE[::-1])
                img = np.reshape(img, IMG_SIZE + (1,))
                with open(os.path.join(root, name[:-4] + ".txt"), "r", encoding=encoding) as f:
                    txt = f.read()
                tlabels = text_to_labels(txt, characters, SEQ_LEN)
                yield img/255., tlabels, len(txt)

def enumerate_datasets(datasets: list[tuple[str, str]], IMG_SIZE, characters, SEQ_LEN, limit=None, augment=lambda x:x):
    iterators = [itertools.cycle(enumerate_dataset(path, enc, IMG_SIZE, characters, SEQ_LEN, augment))
                 for (path,enc) in datasets]
    count = 0
    while True:
        for it in iterators:
            yield next(it)
            count += 1
            if limit is not None and count >= limit:
                return
