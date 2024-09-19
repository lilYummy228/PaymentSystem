using System;
using System.Text;
using System.Security.Cryptography;

namespace PaymentSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            //Выведите платёжные ссылки для трёх разных систем платежа: 
            //pay.system1.ru/order?amount=12000RUB&hash={MD5 хеш ID заказа}
            //order.system2.ru/pay?hash={MD5 хеш ID заказа + сумма заказа}
            //system3.com/pay?amount=12000&curency=RUB&hash={SHA-1 хеш сумма заказа + ID заказа + секретный ключ от системы}

            int amount = 12000;
            int minRandomNumber = 1;
            int maxRandomNumber = 101;

            Random random = new Random();
            int id = random.Next(minRandomNumber, maxRandomNumber);
            int secretKey = random.Next(minRandomNumber, maxRandomNumber);

            Order order = new Order(id, amount);

            PaymentSystem1 paymentSystem1 = new PaymentSystem1(Domain.ru, Currency.RUB);
            Console.WriteLine(paymentSystem1.GetPayingLink(order));

            PaymentSystem2 paymentSystem2 = new PaymentSystem2(Domain.ru);
            Console.WriteLine(paymentSystem2.GetPayingLink(order));

            PaymentSystem3 paymentSystem3 = new PaymentSystem3(Currency.RUB, Domain.com, secretKey);
            Console.WriteLine(paymentSystem3.GetPayingLink(order));
        }
    }

    public class PaymentSystem1 : PaymentSystem
    {
        private Currency _currency;

        public PaymentSystem1(Domain domain, Currency currency, string name = "system1")
        {
            _domain = domain;
            _currency = currency;
            _name = name;
        }

        public override string GetPayingLink(Order order) =>
            $"pay.{_name.ToLower()}.{_domain}/{nameof(order)}?amount={order.Amount}{_currency}&{GetHash(order.Id)}";

        public override string GetHash(int input) =>
            $"hash={Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Convert.ToString(input))))}";
    }

    public class PaymentSystem2 : PaymentSystem
    {
        public PaymentSystem2(Domain domain, string name = "system2")
        {
            _domain = domain;
            _name = name;
        }

        public override string GetHash(int input) =>
            $"hash={Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Convert.ToString(input))))}";

        public override string GetPayingLink(Order order) =>
            $"order.{_name.ToLower()}.{_domain}/pay?{GetHash(order.Id + order.Amount)}";
    }

    public class PaymentSystem3 : PaymentSystem
    {
        private Currency _currency;
        private int _secretKey;

        public PaymentSystem3(Currency currency, Domain domain, int secretKey, string name = "system3")
        {
            _currency = currency;
            _domain = domain;
            _secretKey = secretKey;
            _name = name;
        }

        public override string GetHash(int input) =>
            $"hash={Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(Convert.ToString(input))))}";

        public override string GetPayingLink(Order order) =>
            $"{_name.ToLower()}.{_domain}/pay?amount={order.Amount}&currency={_currency}&{GetHash(order.Amount + order.Id + _secretKey)}";
    }

    public abstract class PaymentSystem : IPaymentSystem
    {
        protected Domain _domain;
        protected string _name;

        public abstract string GetHash(int input);

        public abstract string GetPayingLink(Order order);
    }

    public class Order
    {
        public readonly int Id;
        public readonly int Amount;

        public Order(int id, int amount) => (Id, Amount) = (id, amount);
    }

    public interface IHashCoding
    {
        string GetHash(int input);
    }

    public interface IPaymentSystem : IHashCoding
    {
        string GetPayingLink(Order order);
    }

    public enum Domain
    {
        ru = 0,
        com
    }

    public enum Currency
    {
        RUB = 0,
        USD
    }
}
