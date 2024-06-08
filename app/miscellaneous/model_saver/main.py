# C:\Users\Stefan\miniconda3\envs\kaggle\python.exe D:\Public\model_saver\main.py D:\Public\model_saver\train_model_0 D:\Public\model_saver\model.txt

import tensorflow as tf, os, cv2, numpy as np, struct, sys

def float_to_hex(f): return hex(struct.unpack('<I', struct.pack('<f',f))[0])[2:]

def unpack_model(model):
    result=""    
    for layer in model.layers:        
        result+="[[Layer]]\n"
        layer_type = str(type(layer).__name__)
        result+=f"[type]\n{layer_type}\n"
        
        layer_name = layer.name
        result+=f"[name]\n{layer_name}\n"              
        
        config = layer.get_config()        
        for key in list(config.keys()):
            if "_initializer" in key or key=='trainable':
                config.pop(key)
            elif config[key] is None:
                config.pop(key)       
        
        result+=f"[config]\n{config}\n"               
        
        input_layers = layer._inbound_nodes[0].inbound_layers
        if not isinstance(input_layers, list):
            input_layers = [input_layers]
        input_layers = [_.name for _ in input_layers]    

        if len(input_layers)>0:
            result+=f"[inputs]\n{';'.join(input_layers)}\n"
                
        weights = [(_.shape, _.flatten().tolist()) for _ in layer.get_weights()]
        
        weight_shapes = [" ".join(map(str,_[0])) for _ in weights]
        result+=f"[weight_shapes]\n{';'.join(weight_shapes)}\n" if len(weight_shapes)>0 else "[weight_shapes]\n0\n"
        weights = [_[1] for _ in weights]
        
        if len(weights)>0:
            result+=f"[weights]\n"        
            result+=f"{' '.join(map(lambda w:' '.join(map(float_to_hex, w)), weights))}\n"
            
    o = model.output
    if not isinstance(o, list):
        o = [o]
    result+=f"[[Outputs]]\n{';'.join(map(lambda l:l.name.split('/')[0], o))}" # conv2d_20/Softmax:0 ??
    return result
      

if len(sys.argv)!=3:
    print("Wrong arguments count.")
    exit(-1)
 
in_path = sys.argv[1]
out_path = sys.argv[2]
    

print("Loading")
model = tf.keras.models.load_model(in_path, compile=False)

print("Unpacking")
o = unpack_model(model)

print("Writing")

with open(out_path, 'w') as f:
    f.write(o)    

print("Done.")