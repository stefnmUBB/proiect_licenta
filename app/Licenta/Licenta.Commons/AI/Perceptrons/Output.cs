﻿namespace Licenta.Commons.AI.Perceptrons
{
    public class Output : Perceptron
    {
        public override double Activate(double value) => value;

        public override double Derivative(double value) => 1;
    }
}
