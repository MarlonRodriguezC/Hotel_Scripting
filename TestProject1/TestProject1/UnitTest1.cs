using TestProject1.Source;

namespace TestProject1
{
    public class Tests
    {

        private IAccount accountA, accountB, accountC;
        [SetUp]
        public void Setup()
        {
            accountA = new SavingAccount(1f);
            accountB = new SavingAccount(0f);
            accountC = new SavingAccount(-1f);
        }

        [Test]
        public void TestCreateAccount()
        {
            IAccount account = new Account();
            Assert.That(account.Balance == 100);

            IAccount account1 = new Account(100000);
            Assert.That(account1.Balance == 100000);

            IAccount account2 = new Account(1);
            Assert.That(account2.Balance == 1);

            IAccount account3 = new Account(0);
            Assert.That(account3.Balance == 0);

            IAccount account4 = new Account(-100);
            /*account4.balance = 100;*/
            Assert.That(account4.Balance == 0);

            IAccount account5 = new AccountB(100);
            Assert.That(account5.Balance == 100);
        }

        [Test]
        public void TestDepositAccount()
        {
            IAccount account = new Account();
            account.Deposit(50);
            Assert.That(account.Balance == 150);

            IAccount account1 = new AccountB(100);
            account1.Deposit(50);
            Assert.That(account1.Balance == 150);
        }

        [Test]
        public void TestWithdrawAccount()
        {
            IAccount account = new Account();
            account.WithDraw(50);
            Assert.That(account.Balance == 50);

            IAccount account1 = new AccountB(100);
            account1.WithDraw(50);
            Assert.That(account1.Balance == 50);
        }

        [TestCase(1F)]
        [TestCase(0F)]
        [TestCase(-1F)]
        public void TestForTestCases(float amount)
        {
            if (amount > 0)
            {
                Assert.DoesNotThrow(() =>
                {
/*                    accountA.Deposit(amount);
                    accountB.Deposit(amount);*/
                    accountC.Deposit(amount);
                });
            }
            else
            {
                Assert.Throws<ArgumentException>(() =>
                {
/*                    accountA.Deposit(amount);
                    accountB.Deposit(amount);*/
                    accountC.Deposit(amount);
                });
            }
        }
    }
}