using System;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;
using System.Threading;
using System.Text.RegularExpressions;
using System.Linq;

public class UdpFileClient
{
    // Детали файла
    [Serializable]
    public class FileDetails
    {
        public string FILETYPE = "";
        public long FILESIZE = 0;
    }

    private static FileDetails fileDet;

    // Поля, связанные с UdpClient
    private static int localPort = 5002;
    private static UdpClient receivingUdpClient = new UdpClient(localPort);
    private static IPEndPoint RemoteIpEndPoint = null;

    private static FileStream fs;
    private static Byte[] receiveBytes = new Byte[0];
    private static string returnData="";

    private static IPAddress remoteIPAddress = IPAddress.Parse("192.168.0.12");
    private const int remotePort = 5001;

    static System.Net.IPAddress ip;
    [STAThread]
    static void Main(string[] args)
    {
        String host = System.Net.Dns.GetHostName();
        ip = System.Net.Dns.GetHostByName(host).AddressList[0];
        SendMes("**ip "+ ip+ " online");
        Console.WriteLine("Клиент:");
        Chat();
    }

    private static void Chat()
    {
        try
        {

            // Создаем поток для прослушивания
            Thread tRec = new Thread(new ThreadStart(ReceiverText));
            tRec.Start();

            while (true)
            {
                SendMes(Console.ReadLine());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
        }

    }

    private static void SendMes(string datagram)
    {
        // Создаем UdpClient
        UdpClient sender = new UdpClient();

        // Создаем endPoint по информации об удаленном хосте
        IPEndPoint endPoint = new IPEndPoint(remoteIPAddress, remotePort);

        try
        {
            // Преобразуем данные в массив байтов
            byte[] bytes = Encoding.UTF8.GetBytes(datagram);

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

    private static void GetFileDetails()
    {
        try
        {
            Console.WriteLine("-----------*******Ожидание информации о файле*******-----------");

            // Получаем информацию о файле
            receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);
            Console.WriteLine("----Информация о файле получена!");

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream1 = new MemoryStream();

            // Считываем информацию о файле
            stream1.Write(receiveBytes, 0, receiveBytes.Length);
            stream1.Position = 0;

            // Вызываем метод Deserialize
            fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
            Console.WriteLine("Получен файл типа ." + fileDet.FILETYPE +
                " имеющий размер " + fileDet.FILESIZE.ToString() + " байт");
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }

    private static void GetFileDetails2(byte[] receiveBytes)
    {
        try
        {

            Console.WriteLine("----Информация о файле получена!");

            XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
            MemoryStream stream1 = new MemoryStream();

            // Считываем информацию о файле
            stream1.Write(receiveBytes, 0, receiveBytes.Length);
            stream1.Position = 0;

            // Вызываем метод Deserialize
             fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
             Console.WriteLine("Получен файл типа ." + fileDet.FILETYPE +
                 " имеющий размер " + fileDet.FILESIZE.ToString() + " байт");
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
    }

    public static void ReceiveFile()
    {
        try
        {
            Console.WriteLine("-----------*******Ожидайте получение файла*******-----------");

            // Получаем файл
            receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

            // Преобразуем и отображаем данные
            Console.WriteLine("----Файл получен...Сохраняем...");

            // Создаем временный файл с полученным расширением
            fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Write(receiveBytes, 0, receiveBytes.Length);

            Console.WriteLine("----Файл сохранен...");

            Console.WriteLine("-------Открытие файла------");

            // Открываем файл связанной с ним программой
            Process.Start(fs.Name);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            fs.Close();
            receivingUdpClient.Close();
            Console.Read();
        }
    }

    public static void ReceiveBigFile(Byte[] receiveBytes)
    {
        try
        {
            Console.WriteLine("-----------*******Ожидайте получение файла*******-----------");

            // Получаем файл
           // receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

            // Преобразуем и отображаем данные
            Console.WriteLine("----Файл получен...Сохраняем...");

            // Создаем временный файл с полученным расширением
            fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            fs.Write(receiveBytes, 0, receiveBytes.Length);

            Console.WriteLine("----Файл сохранен...");

            Console.WriteLine("-------Открытие файла------");

            // Открываем файл связанной с ним программой
            Process.Start(fs.Name);
        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
          //  fs.Close();
            receivingUdpClient.Close();
            Console.Read();
        }
    }
    static Byte[] bytesm;
    static bool File=false,Big = false;
    static int nak=0;

    public static void ReceiverText()
    {
        // Создаем UdpClient для чтения входящих данных
        // UdpClient receivingUdpClient2 = new UdpClient(localPort);
        // IPEndPoint RemoteIpEndPoint2 = null;

        try
        {
            Console.WriteLine(
               "\n-----------*******Общий чат*******-----------");

            while (true)
            {
                // Ожидание дейтаграммы
                byte[] receiveBytes = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                returnData = Encoding.UTF8.GetString(receiveBytes);
                if (returnData.ToString() == "File:" || File == true)
                {
                    #region Если это файл...
                    File = true;
                    if (returnData.ToString() == "Small")
                    {
                        GetFileDetails();
                        ReceiveFile();
                        returnData = "";
                        File = false;
                    }
                    else if (returnData.ToString().Length >= 4 && returnData.ToString().Substring(0, 4) == "Info")
                    { GetFileDetails(); }
                    else if (returnData.ToString().Length >= 3 && returnData.ToString().Substring(0, 3) == "Big")
                    {
                        if (Big == false)
                        {
                            Console.WriteLine("-----------*******Ожидайте получение файла*******-----------");
                            String[] elements = Regex.Split(returnData.ToString(), ",");
                            Console.WriteLine(" --> " + elements[0] + " " + elements[1]);
                            nak = Convert.ToInt32(elements[1]);
                            Big = true;
                            returnData = "";
                            bytesm = new Byte[0];
                        }
                    }
                    else if (Big == true)
                    {
                        
                        if (returnData.ToString() == "End")
                        {
                            Console.WriteLine(" ---End--- ");
                            Console.WriteLine("L=" + bytesm.Length);
                            Big = false;
                            fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                            fs.Write(bytesm, 0, bytesm.Length);
                            Process.Start(fs.Name);
                            File = false;
                            SendMes("Ip: "+ip+" Got File");
                        }
                        else
                        {
                            bytesm = bytesm.Concat(receiveBytes).ToArray();
                            Console.WriteLine("----Получено " + receiveBytes.Length + " Байт");
                            nak = nak - 1;
                            Console.WriteLine(" --> " + nak);
                        }
                        /*
                        if (nak <= 0)
                        {
                         //   Console.WriteLine(" ---End--- ");
                         //   Console.WriteLine("L=" + bytesm.Length);
                         //   Big = false;
                            fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                            fs.Write(bytesm, 0, bytesm.Length);
                            Process.Start(fs.Name);
                        //    File = false;
                        }
                         * */
                         
                    }
                    # endregion
                }
                else if (File == false)
                {
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