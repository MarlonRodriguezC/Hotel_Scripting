using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Source
{
    internal class Account : IAccount
    {
        public float balance;

        public Account() { 
            balance = 100;
        }

        public Account(float balance)
        {
            if (balance < 0)
            {
                this.balance = 0;
            }
            else
            {
                this.balance = balance;
            }
        }

        public float Balance => balance;

        public void Deposit(float amount) {
            if  (amount > 0)
            {
                balance += amount;
            }
        }

        public void WithDraw(float amount)
        {
            balance -= amount;
        }
    }
}
