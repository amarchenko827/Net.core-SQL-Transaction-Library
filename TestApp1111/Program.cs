using OrderSqlTestLibrary;
using System;
using System.Threading;

namespace TestTask1
{
    class Program
    {
        static void Mythread(object customerId)
        {
            int id = (int)customerId;
            Random rnd = new Random();
            int quantity = rnd.Next(1, 4); //случайное число единиц товаров на заказ
            Sets shop = new Sets();
            shop.DbSets();
            for (int i = 0; i < 100; i++)
            {
                shop.MakeOrder(id, quantity);
                Thread.Sleep(5000);
            }
        }

        static void Main(string[] args)
        {
            Sets shop = new Sets();
            shop.DbSets();
            shop.AddProduct();
            shop.CheckProduct();

            Thread thread1 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread2 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread3 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread4 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread5 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread6 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread7 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread8 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread9 = new Thread(new ParameterizedThreadStart(Mythread));
            Thread thread10 = new Thread(new ParameterizedThreadStart(Mythread));

            thread1.Start(1);
            thread2.Start(2);
            thread3.Start(3);
            thread4.Start(4);
            thread5.Start(5);
            thread6.Start(6);
            thread7.Start(7);
            thread8.Start(8);
            thread9.Start(9);
            thread10.Start(10);
        }
    }
}