using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Threading;

public class UdpFileServer
{
    // Информация о файле (требуется для получателя)
    [Serializable]
    public class FileDetails
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

    private static FileDetails fileDet = new FileDetails();

    // Поля, связанные с UdpClient
    private static IPAddress remoteIPAddress;
    private const int remotePort = 5002;
    private static UdpClient sender = new UdpClient();
    private static IPEndPoint endPoint;

    // Filestream object
    private static FileStream fs;
    [STAThread]
    static void Main(string[] args)
    {
        try
        {
            // Получаем удаленный IP-адрес и создаем IPEndPoint
           // Console.WriteLine("Введите удаленный IP-адрес");
           // remoteIPAddress = IPAddress.Parse(Console.ReadLine().ToString());//"127.0.0.1");
            remoteIPAddress = IPAddress.Parse("127.0.0.1");
            endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            // Получаем путь файла и его размер (должен быть меньше 8kb)
            Console.WriteLine("Введите путь к файлу и его имя");
            fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);
            
            
            if (fs.Length > 8192)
            {
                Console.WriteLine("Файл больше 8кБ");
                SendText("Info");
                SendFileInfo();
                Thread.Sleep(2000);
                SendText("Big," + (fs.Length / 8100+1));
                SendBigFile();
                fs.Close();
                return;
            }
            else
            SendText("Small");
            
            // Отправляем информацию о файле
            SendFileInfo();

            // Ждем 2 секунды
            Thread.Sleep(2000);

            // Отправляем сам файл
            SendFile();

            Console.ReadLine();

        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }
    public static void SendFileInfo()
    {

        // Получаем тип и расширение файла
        fileDet.FILETYPE = fs.Name.Substring((int)fs.Name.Length - 3, 3);

        // Получаем длину файла
        fileDet.FILESIZE = fs.Length;

        XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
        MemoryStream stream = new MemoryStream();

        // Сериализуем объект
        fileSerializer.Serialize(stream, fileDet);

        // Считываем поток в байты
        stream.Position = 0;
        Byte[] bytes = new Byte[stream.Length];
        stream.Read(bytes, 0, Convert.ToInt32(stream.Length));

        Console.WriteLine("Отправка деталей файла...");

        // Отправляем информацию о файле
        sender.Send(bytes, bytes.Length, endPoint);
        stream.Close();

    }
    private static void SendBigFile()
    {
        Byte[] bytes = new Byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);
        int numBytesToRead = 8190;
        int n = 0;
        long del = fs.Length / numBytesToRead;
        for (int i = 0; i <= del; i++)
        {
            if (i == del) { numBytesToRead = Convert.ToInt32(fs.Length - numBytesToRead * del); }
            Byte[] bytes_min = new Byte[numBytesToRead];
            n = 0;
            for (int j = i * numBytesToRead; j < i * numBytesToRead + numBytesToRead; j++)
            {
                bytes_min[n] = bytes[j];
                n++;
            }
            SendFile2(bytes_min);
            Thread.Sleep(500);
        }
    }
    private static void SendFile()
    {
        // Создаем файловый поток и переводим его в байты
        Byte[] bytes = new Byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);

        Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
        try
        {
            // Отправляем файл
            sender.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            // Закрываем соединение и очищаем поток
            fs.Close();
            sender.Close();
        }
        Console.WriteLine("Файл успешно отправлен.");
        Console.Read();
    }

    private static void SendFile2(Byte[] bytes)
    {
        Console.WriteLine("Отправка части файла размером " + bytes.Length + " байт");
        try
        {
            // Отправляем файл
            sender.Send(bytes, bytes.Length, endPoint);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        Console.WriteLine("Успешно отправлено.");
    }

    private static void SendText(string datagram)
    {
        // Создаем UdpClient
        UdpClient sender2 = new UdpClient();

        // Создаем endPoint по информации об удаленном хосте
        IPEndPoint endPoint2 = new IPEndPoint(remoteIPAddress, remotePort);

        try
        {
            // Преобразуем данные в массив байтов
            byte[] bytes = Encoding.UTF8.GetBytes(datagram);

            // Отправляем данные
            sender2.Send(bytes, bytes.Length, endPoint2);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
        }
        finally
        {
            // Закрыть соединение
            sender2.Close();
        }
    }
}








/*
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace UdpSample
{
    class Chat
    {
        private static IPAddress remoteIPAddress;
        private static int remotePort;
        private static int localPort;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Получаем данные, необходимые для соединения
                Console.WriteLine("Укажите локальный порт");
                localPort = Convert.ToInt16(Console.ReadLine());
                //localPort = 5001;
                //localPort = 5002;
                Console.WriteLine("Укажите удаленный порт");
                remotePort = Convert.ToInt16(Console.ReadLine());
                //remotePort = 5002;
                //remotePort = 5001;
                //Console.WriteLine("Укажите удаленный IP-адрес");
                //remoteIPAddress = IPAddress.Parse(Console.ReadLine());
                remoteIPAddress=IPAddress.Parse("127.0.0.1");
                //remoteIPAddress = IPAddress.Parse("192.168.0.64");
               
                //////////////////Определяю IP/////////////////
                //String host = System.Net.Dns.GetHostName();
                //System.Net.IPAddress ip = System.Net.Dns.GetHostByName(host).AddressList[0];
                //Console.WriteLine(ip);
                ///////////////////////////////////////////////
                
                // Создаем поток для прослушивания
                Thread tRec = new Thread(new ThreadStart(Receiver));
                tRec.Start();

                while (true)
                {
                    Send(Console.ReadLine());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }

        private static void Send(string datagram)
        {
            // Создаем UdpClient
            UdpClient sender = new UdpClient();

            // Создаем endPoint по информации об удаленном хосте
            IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

            try
            {
                // Преобразуем данные в массив байтов
                byte[] bytes = Encoding.UTF8.GetBytes(datagram);

                if (datagram == "start")
                {
                    bytes = File.ReadAllBytes("123.png");
                    Console.WriteLine("Поехали...");
                }

                // Отправляем данные
                sender.Send(bytes, bytes.Length, endPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
            finally
            {
                // Закрыть соединение
                sender.Close();
            }
        }
        static int flag=0;
        public static void Receiver()
        {
            // Создаем UdpClient для чтения входящих данных
            UdpClient receivingUdpClient = new UdpClient(localPort);

            IPEndPoint RemoteIpEndPoint = null;

            try
            {
                Console.WriteLine(
                   "\n-----------*******Общий чат*******-----------");

                while (true)
                {
                    
                    // Ожидание дейтаграммы
                    byte[] receiveBytes = receivingUdpClient.Receive(
                       ref RemoteIpEndPoint);
                    if (flag == 1)
                    {
                        Console.WriteLine(" --> Получен файл");
                        File.WriteAllBytes(@"D:\savedFile.png", receiveBytes);
                        flag = 0;
                    }
                    else
                    {
                        // Преобразуем и отображаем данные
                        string returnData = Encoding.UTF8.GetString(receiveBytes);

                        if (returnData.ToString() == "start")
                        {
                            flag = 1;
                        }

                        Console.WriteLine(" --> " + returnData.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
            }
        }
    }
}
*/