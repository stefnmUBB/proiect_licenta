import numpy as np
import tensorflow as tf

def generator(img_floats_iter, poly_len, pts_len, augment=lambda x,y:(x,y)):   
    for i,o in img_floats_iter:        
        i = tf.constant(i)
        o = tf.constant(o)        
        yield (i,o)

def dataset_generator(img_floats_iter, img_size, poly_len, pts_len, augment=lambda x,y:(x,y)):
    return tf.data.Dataset.from_generator(lambda: generator(img_floats_iter, poly_len, pts_len, augment),
                                          output_types=(tf.float32, tf.float32),
                                          output_shapes=(img_size+(1,), img_size+(3,)))
                                          
                                          
def transform_generator(generator, processor):
    iterator = iter(generator)
    first = next(iterator)
    first_processed = processor(first)     
    
    def new_generator():
        nonlocal first_processed, iterator, processor        
        yield first_processed
        for it in iterator:
            yield processor(it)          
    
    output_shapes = tuple([tuple(map(int, _.shape)) for _ in first_processed])    
    output_types = tuple([_.dtype for _ in first_processed])
    
    print(output_shapes)
    
    return tf.data.Dataset.from_generator(new_generator,
                                          output_types=output_types,
                                          output_shapes=output_shapes)
        
        
def get_points(data, ALL_PTS_LEN): # data = img, poly_float_norm ==> img, points[uniq|sort], indexed_poly
    x,y = [_.numpy() for _ in data]                             

    h, w, _ = x.shape
    y0 = y = (y*[w,h]).astype(np.int32)
            
    polys = []
    for i in range(len(y)):
        poly = y[i]
        l = min(np.argmax(poly<0, axis=0))        
        if l>2:
            polys.append(poly[:l])                

    points = np.concatenate(polys)
    points = np.unique(points, axis=0)          

    def indexof(x):        
        for i in range(len(points)):                
            if points[i][0]==x[0] and points[i][1]==x[1]:                    
                return i            
        return -1                    

    indexed_poly = np.apply_along_axis(indexof, axis=2, arr=y0)                                        
    points = np.pad(points, ((0,ALL_PTS_LEN-len(points)), (0, 0)), 'constant', constant_values=-1)                
    return x, points, indexed_poly