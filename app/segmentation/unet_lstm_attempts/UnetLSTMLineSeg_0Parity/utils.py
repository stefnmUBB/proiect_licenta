import cv2
import numpy as np


def text_to_labels(text:str, characters, SEQ_LEN):
    """
    Converts a string to an array of labels.
    Each character is mapped to an integer label that marks its
    position in the `characters` string.
    The resulted array is padded up to `SEQ_LEN` with a constant
    value (has the same value with the so-called "CTC blank label").
    """
    char_ids = []
    for c in text:
        cid = characters.find(c)
        if cid<0:
            print(f"Warning: undefined character `{c}`")
        else:
            char_ids.append(cid)
    while(len(char_ids)<SEQ_LEN):
        char_ids.append(len(characters))
    return np.array(char_ids)

def enhance(img):
    """
    Increases constrast and makes lines thicker in an image
    """
    pxmin = np.min(img)
    pxmax = np.max(img)
    imgContrast = (img - pxmin) / (pxmax - pxmin) * 255
    kernel = np.ones((3, 3), np.uint8)
    return cv2.erode(imgContrast, kernel, iterations=1) # increase linewidth