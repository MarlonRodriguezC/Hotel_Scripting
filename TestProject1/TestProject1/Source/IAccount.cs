using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Source
{
    internal interface IAccount
    {
        float Balance { get; }
        void Deposit(float amount);

        void WithDraw(float amount);
    }
}
