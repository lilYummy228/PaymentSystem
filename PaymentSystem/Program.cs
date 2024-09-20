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

            PaymentSystem1 paymentSystem1 = new PaymentSystem1
                (new MD5HashGenerator<int>(order.Id), Domain.ru, Currency.RUB);
            Console.WriteLine(paymentSystem1.GetPayingLink(order));

            PaymentSystem2 paymentSystem2 = new PaymentSystem2
                (new MD5HashGenerator<int>(order.Id + order.Amount), Domain.ru);
            Console.WriteLine(paymentSystem2.GetPayingLink(order));

            PaymentSystem3 paymentSystem3 = new PaymentSystem3
                (new SHA1HashGenerator<int>(order.Amount + order.Id + secretKey), Domain.com, Currency.RUB, secretKey);
            Console.WriteLine(paymentSystem3.GetPayingLink(order));
        }
    }

    public interface IPaymentSystem
    {
        string GetPayingLink(Order order);
    }

    public class PaymentSystem1 : IPaymentSystem
    {
        private readonly IHashGenerator _hashGenerator;
        private readonly Domain _domain;
        private readonly Currency _currency;
        private readonly string _name;

        public PaymentSystem1(IHashGenerator hashGenerator, Domain domain, Currency currency, string name = "system1")
        {
            _hashGenerator = hashGenerator;
            _domain = domain;
            _currency = currency;
            _name = name;
        }

        public string GetPayingLink(Order order) =>
            $"pay.{_name}.{_domain}/order?amount={order.Amount}{_currency}&hash={_hashGenerator.GenerateHash()}";
    }

    public class PaymentSystem2 : IPaymentSystem
    {
        private readonly IHashGenerator _hashGenerator;
        private readonly Domain _domain;
        private readonly string _name;

        public PaymentSystem2(IHashGenerator hashGenerator, Domain domain, string name = "system2")
        {
            _hashGenerator = hashGenerator;
            _domain = domain;
            _name = name;
        }

        public string GetPayingLink(Order order) =>
            $"order.{_name}.{_domain}/pay?hash={_hashGenerator.GenerateHash()}";
    }

    public class PaymentSystem3 : IPaymentSystem
    {
        private readonly IHashGenerator _hashGenerator;
        private readonly Domain _domain;
        private readonly Currency _currency;
        private readonly int _secretKey;
        private readonly string _name;

        public PaymentSystem3(IHashGenerator hashGenerator, Domain domain, Currency currency, int secretKey, string name = "system3")
        {
            _hashGenerator = hashGenerator;
            _domain = domain;
            _currency = currency;
            _secretKey = secretKey;
            _name = name;
        }

        public string GetPayingLink(Order order) =>
            $"{_name}.{_domain}/pay?amount={order.Amount}&curency={_currency}&hash={_hashGenerator.GenerateHash()}";
    }

    public class MD5HashGenerator<T> : IHashGenerator
    {
        private readonly string _input;

        public MD5HashGenerator(T input) =>
            _input = input.ToString() ?? throw new ArgumentNullException($"{nameof(input)} is null");

        public string GenerateHash() =>
            Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(_input)));
    }

    public interface IHashGenerator
    {
        string GenerateHash();
    }

    public class SHA1HashGenerator<T> : IHashGenerator
    {
        private readonly string _input;

        public SHA1HashGenerator(T input) =>
            _input = input.ToString() ?? throw new ArgumentNullException($"{nameof(input)} is null");

        public string GenerateHash() =>
            Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(_input)));
    }

    public class Order
    {
        public readonly int Id;
        public readonly int Amount;

        public Order(int id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }

    public enum Domain
    {
        ru = 0,
        com
    }

    public enum Currency
    {
        RUB = 0
    }
}
