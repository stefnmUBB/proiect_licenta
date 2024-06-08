import numpy as np
import tensorflow as tf

def generator(img_floats_iter, poly_len, pts_len, augment=lambda x,y:(x,y)):    
    for x, y, l in img_floats_iter:
        #if len(y)>=seq_len:
        #    print(f"Floats sequence too long ({l})")
        #    continue

        y = [(y[2*i], y[2*i+1]) for i in range(len(y)//2)]
        
        polys = []
        
        h, w, _ = x.shape
        
        poly = []
        for px, py in y:
            if px<0:
                if len(poly)>0:
                    polys.append(np.array(poly, np.float32))
                    poly=[]
            else:
                poly.append([px, py])
        
        x, polys = augment(x, polys)
        
        do_yield=True
        
        if len(polys)>poly_len:
            print(f"polygons length too long ({len(polys)})")
            do_yield=False
            continue
        
        y = []
        for poly in polys:
            p = poly
            if len(p)>pts_len:
                print(f"points count too long ({len(p)})")
                do_yield=False
                break                 
            y.append(np.pad(p, ((0, pts_len-len(p)), (0,0)), 'constant', constant_values=-1))                             
                       
        y = np.array(y)        
        y = np.pad(y, ((0, poly_len-len(y)),(0,0),(0,0)), 'constant', constant_values=-1)             
        
        if do_yield:            
            yield (x, y)        

def dataset_generator(img_floats_iter, img_size, poly_len, pts_len, augment=lambda x,y:(x,y)):
    return tf.data.Dataset.from_generator(lambda: generator(img_floats_iter, poly_len, pts_len, augment),
                                          output_types=(tf.float32, tf.float32),
                                          output_shapes=(img_size+(1,), (poly_len, pts_len, 2)))
                                          
                                          
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