﻿namespace CSA
{
    public interface IAlgorithm
    {
        string Name { get; }
        void Execute();
    }
}