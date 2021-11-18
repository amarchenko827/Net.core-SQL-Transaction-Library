using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace OrderSqlTestLibrary
{
    public class Sets
    {
        private string conString; //строка подключения
        private static IConfiguration conf; //конфигурация базы данных
        private SqlConnection sql = null;

        public void DbSets()
        //подключение к базе данных. конфигурация хранится со строкой подключения хранится отдельно в json
        {
            var dir = Directory.GetCurrentDirectory().Replace("\\bin\\Debug\\net5.0", "");
            var builder = new ConfigurationBuilder().SetBasePath(dir).AddJsonFile("appsettings.json", false, true);

            conf = builder.Build();
            conString = conf.GetConnectionString("TestDB");
            sql = new SqlConnection(conString);
            sql.Open();
            if (sql.State == ConnectionState.Open)
            {
                Console.WriteLine("Подключение успешно\n");
            }
            else Console.WriteLine("Не удалось подключиться к базе данных\n");
        }
        public void AddProduct()
        //добавление в таблицу тестового продукта
        {
            SqlCommand addProd = new SqlCommand(
                "IF NOT EXISTS(SELECT Id FROM Products WITH(UPDLOCK) WHERE Id = 1)" +
                "INSERT INTO Products (ProductName, UnitPrice, UnitOnStock) VALUES ('TestProduct', '250', 100)", sql);
            addProd.ExecuteNonQuery();
        }

        public void CheckProduct()
        //проверка наличия продукта
        {
            SqlDataReader reader;
            SqlCommand checkProd = new SqlCommand(
                "SELECT * FROM Products WITH (UPDLOCK) WHERE Id = 1", sql);

            reader = checkProd.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(reader["ProductName"] + ", цена: " + reader["UnitPrice"] + " рублей, остаток: " + reader["UnitOnStock"] + " штук\n\n\n\n");
            }
            reader.Close();
        }

        public void MakeOrder(int customerIdPar, int quantityPar)
        /*создание заказа. в параметрах имя пользователя и количество единиц товара, которое он хочет заказать.
        данные о заказе включают в себя также название продукта, полную стоимость заказа и дату заказа,
        на основе которой осуществляется проверка*/
        {
            var productId = 1;   //т.к. работа подразумевает 1 тестовый продукт - упрощено получение его данных (просто создаём обычную переменную)
            var customerId = customerIdPar;
            var quantity = quantityPar;
            DateTime orderDate = DateTime.Now;
            SqlDataReader reader;

            SqlCommand prodInfo = new SqlCommand(
                "SELECT UnitPrice, UnitOnStock FROM Products WITH (UPDLOCK) WHERE Id = @Id", sql); //получаем данные продукта

            prodInfo.Parameters.AddWithValue("@Id", productId);
            reader = prodInfo.ExecuteReader();
            reader.Read();

            var unitPrice = Convert.ToInt32(reader["UnitPrice"]); //цена за единицу
            var fullPrice = unitPrice * quantity; //полная стоимость заказа
            var unitOnStock = Convert.ToInt32(reader["UnitOnStock"]);

            reader.Close();

            SqlCommand customerInfo = new SqlCommand(
                "SELECT Name FROM Customers WITH (UPDLOCK) WHERE Id = @Id", sql); //получаем данные заказчика

            customerInfo.Parameters.AddWithValue("@Id", customerId);
            reader = customerInfo.ExecuteReader();
            reader.Read();

            var customerName = reader["Name"].ToString();

            reader.Close();


            if (unitOnStock == 0)
            {
                Console.WriteLine("Товара нет в наличии\n\n");
            }
            else if (unitOnStock < quantity && unitOnStock != 0) //проверка на случай если пользователь заказывает товара больше, чем есть
            {
                Console.WriteLine("Товара нет в таком количестве, попробуйте снова\n\n");
            }
            else if (unitOnStock >= quantity && unitOnStock != 0) //главное рассматриваемое задание
            {
                unitOnStock -= quantity; //изменение оставшегося количества товаров

                SqlTransaction transaction = sql.BeginTransaction(); //применение транзакции для предотвращения ошибок

                SqlCommand makeOrder = sql.CreateCommand();
                makeOrder.Transaction = transaction;

                try //конструкция try catch для обработки ошибок
                {
                    //makeOrder.CommandText = "UPDATE Products  WITH (UPDLOCK) SET UnitOnStock = @UnitOnStock WHERE Id = 1";
                    //makeOrder.Parameters.AddWithValue("@UnitOnStock", unitOnStock);
                    //makeOrder.ExecuteNonQuery();
                    makeOrder.CommandText = "UPDATE Products  WITH (UPDLOCK) SET UnitOnStock = @UnitOnStock WHERE Id = 1" +
                    "IF NOT EXISTS(SELECT OrderDate, CustomerId FROM Orders WITH (UPDLOCK) WHERE OrderDate = @OrderDate AND CustomerId = @CustomerId)" +
                    "INSERT INTO Orders (ProductId, OrderDate, CustomerId, Quantity, FullPrice) VALUES (@ProductId, @OrderDate, @CustomerId, @Quantity, @FullPrice)";
                    makeOrder.Parameters.AddWithValue("@UnitOnStock", unitOnStock);
                    makeOrder.Parameters.AddWithValue("@ProductId", 1);
                    makeOrder.Parameters.AddWithValue("@OrderDate", orderDate);
                    makeOrder.Parameters.AddWithValue("@CustomerId", customerId);
                    makeOrder.Parameters.AddWithValue("@Quantity", quantity);
                    makeOrder.Parameters.AddWithValue("@FullPrice", fullPrice);
                    makeOrder.ExecuteNonQuery();
                    transaction.Commit();
                    Console.WriteLine("Заказ сделан, На складе осталость " + unitOnStock + " единиц товара\n");
                    Console.WriteLine("Последний заказчик: " + customerName + ", заказал " + quantity + " единиц товара на стоимость " + fullPrice + "\n\n");
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    transaction.Rollback();
                }
            }
        }
    }
}
