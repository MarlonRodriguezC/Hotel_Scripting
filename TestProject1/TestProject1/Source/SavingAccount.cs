using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Source
{
    internal class SavingAccount : AbastractAccount
    {
        public SavingAccount(float balance) : base(balance)
        {
        }

        public override void Deposit(float amount)
        {
            if (amount > 0) { 
                base.Deposit(amount);
            }
            else
            {
                throw new ArgumentException("Gay");
            }
        }
    }
}
