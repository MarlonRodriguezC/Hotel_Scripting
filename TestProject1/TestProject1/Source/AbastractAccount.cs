using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Source
{
    internal abstract class AbastractAccount : IAccount
    {
        private float balance;

        protected AbastractAccount(float balance) => this.balance = balance > 0 ? balance : 0;


        public float Balance => balance;

        public virtual void Deposit(float amount)
        {
            ChangeBalance(amount);
        }

        public void WithDraw(float amount)
        {
            ChangeBalance(amount);
        }

        private void ChangeBalance(float amount) => balance += amount;
    }
}
