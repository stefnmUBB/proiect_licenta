using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.AI.Models;
using LillyScan.Backend.Math;
using LillyScan.Backend.Types;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LillyScan
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            var a = new Tensor<float>((1, 4, 4, 1), Enumerable.Range(1, 16).Select(_ => (float)_).ToArray());
            var b = new Tensor<float>((1, 4, 4, 1), Enumerable.Range(2, 16).Select(_ => (float)_).ToArray());
            a = Tensors.Concatenate(new[] { a, b });
            var k2 = Tensors.Ones<float>((2, 2, 1, 2));
            var k3 = Tensors.Ones<float>((3, 3, 1, 2));

            a.Print();

            a.Conv2DTranspose(k2, (1, 1)).Squeeze().Print("k2, (1,1)");
            a.Conv2DTranspose(k2, (2, 2)).Squeeze().Print("k2, (2,2)");
            a.Conv2DTranspose(k2, (2, 1)).Squeeze().Print("k2, (2,1)");
            a.Conv2DTranspose(k2, (3, 3)).Squeeze().Print("k2, (3,3)");
            a.Conv2DTranspose(k2, (2, 3)).Squeeze().Print("k2, (2,3)");
            a.Conv2DTranspose(k3, (1, 1)).Squeeze().Print("k3, (1,1)");
            a.Conv2DTranspose(k3, (2, 2)).Squeeze().Print("k3, (2,2)");
            a.Conv2DTranspose(k3, (2, 1)).Squeeze().Print("k3, (2,1)");
            a.Conv2DTranspose(k3, (3, 3)).Squeeze().Print("k3, (3,3)");
            a.Conv2DTranspose(k3, (2, 3)).Squeeze().Print("k3, (2,3)");



            //var up = new UpSampling2D(a.Shape, (3, 2));
            //up.Call(a)[0].Squeeze().Print();

            //ModelLoader.LoadFromStream(File.Open(@"D:\Public\model_saver\model.txt", FileMode.Open));
            /*
            var a = Tensors.Ones<float>((1, 4, 4, 1));

            var l0 = new InputLayer(a.Shape.AsPlaceholder(axis: 0));
            var l1 = new Conv2D(l0.GetOutputShapes(), 1, kernelSize: (5, 1), useBias: true);

            var b = l0.Call(a);
            b = l1.Call(b);

            b[0].Squeeze().Print();    */

            Console.WriteLine("Done");
            Console.ReadLine();
        }


        static void Main2()
        {
            NameSolver.DumpNamedTypes();

            void dprint(object o)
            {
                if(o is Dictionary<string, object> d)
                {
                    foreach(var kv in d)
                    {
                        Console.Write($"{{{kv.Key}: ");
                        dprint(kv.Value);
                        Console.WriteLine($"}}");                        
                    }
                    return;
                }
                if(o is object[] a)
                {
                    Console.Write("[");
                    foreach (var x in a)
                    {
                        dprint(x);
                        Console.Write(",");
                    }
                    Console.Write("]");
                    return;
                }
                Console.Write(o ?? "None");
            }

            void test(string ip)
            {
                Console.WriteLine("_____________________________");
                Console.WriteLine(ip);                
                var d = PythonDictionaryParser.Parse(ip);
                Console.WriteLine("Decoded:");
                dprint(d);
                //d.ForEach(_=>Console.WriteLine(_));
            }

            test("{}");
            test("{'batch_input_shape': (None, 256, 256, 1), 'dtype': 'float32', 'sparse': False, 'ragged': False}");
            //test("{'batch_input_shape': (None, 256, 256, 1), 'dtype': 'float32', 'sparse': False, 'ragged': False}");
            //test("{'dtype': 'float32', 'filters': 16, 'kernel_size': (3, 3), 'strides': (1, 1), 'padding': 'same', 'data_format': 'channels_last', 'dilation_rate': (1, 1), 'groups': 1, 'activation': 'linear', 'use_bias': True}");
            test("{'dtype': 'float32', 'layer': {'module': 'keras.layers', 'class_name': 'LSTM', 'config': {'name': 'lstm', 'trainable': True, 'dtype': 'float32', 'return_sequences': True, 'return_state': False, 'go_backwards': False, 'stateful': False, 'unroll': False, 'time_major': False, 'units': 256, 'activation': 'tanh', 'recurrent_activation': 'sigmoid', 'use_bias': True, 'kernel_initializer': {'module': 'keras.initializers', 'class_name': 'GlorotUniform', 'config': {'seed': None}, 'registered_name': None}, 'recurrent_initializer': {'module': 'keras.initializers', 'class_name': 'Orthogonal', 'config': {'gain': 1.0, 'seed': None}, 'registered_name': None}, 'bias_initializer': {'module': 'keras.initializers', 'class_name': 'Zeros', 'config': {}, 'registered_name': None}, 'unit_forget_bias': True, 'kernel_regularizer': None, 'recurrent_regularizer': None, 'bias_regularizer': None, 'activity_regularizer': None, 'kernel_constraint': None, 'recurrent_constraint': None, 'bias_constraint': None, 'dropout': 0.05, 'recurrent_dropout': 0.0, 'implementation': 2}, 'registered_name': None}, 'merge_mode': 'concat'}");

            Console.ReadLine();

            return;
            var t = new Tensor<float>((2, 3, 2), new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            //t = t.Transpose(new[] { 1,0,2 });
            var t1 = new Tensor<float>((1, 5, 5, 1), new float[] { 1, 2, 3, 4, 5, 2, 3, 4, 5, 6, 3, 4, 5, 6, 7, 4, 5, 6, 7, 8, 5, 6, 7, 8, 9 });
            var t2 = new Tensor<float>((3, 3, 1, 2), new float[]
            {
                1,2, 1,2, 1,2,
                1,2, 1,2, 1,2,
                1,2, 1,2, 1,2,
            });

            //Console.WriteLine(t1.ReduceAxis(0, Operations.Sum<float>()).Print());
            t1.Reshape((5, 5)).Print();

            t = t1.Conv2D(t2).Reshape((5, 5, 2));

            t[null, null, new IndexAccessor(0)].Print();
            t[null, null, new IndexAccessor(1)].Print();

            //t1.Conv2D(t2).Reshape((5, 5, 2)).Print();
                      



            //var t1 = new Tensor<float>((2, 2), new float[] { 1, 2, 3, 4 });
            //var t2 = Tensors.Ones<float>((3, 2, 1));



            //Console.WriteLine(t.Shape.IterateSubDimsIndices(1).ToArray().JoinToString("\n"));

            Console.ReadLine();
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());*/
        }
    }
}
