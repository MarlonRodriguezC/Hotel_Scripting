using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1.Source
{
    internal class AccountB : IAccount
    {

        private float balance;

        public AccountB(float balance) {
            this.balance = balance > 0 ? balance : 0;
        }
        public float Balance => balance;

        public void Deposit(float amount)
        {
            ChangeBalance(amount);
        }

        public void WithDraw(float amount)
        {
            ChangeBalance(-amount);
        }

        private void ChangeBalance(float amount) => balance += amount;
    }
}
