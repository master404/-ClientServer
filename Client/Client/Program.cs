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
    [STAThread]
    static void Main(string[] args)
    {
            Thread tRec = new Thread(new ThreadStart(ReceiverText));
            tRec.Start();
//            String[] elements = Regex.Split(returnData.ToString(), ",");
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

    public static void ReceiveFile2(byte[] receiveBytes)
    {
        try
        {
            Console.WriteLine("-----------*******Ожидайте получение файла*******-----------");

            Console.WriteLine("----Получено " + receiveBytes.Length + " Байт...Сохраняем...");

            Console.WriteLine("----Сохранено...");

        }
        catch (Exception eR)
        {
            Console.WriteLine(eR.ToString());
        }
        finally
        {
            //fs.Close();
            //receivingUdpClient.Close();
            //Console.Read();
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
    static Byte[] bytesm= new Byte[0];
    static bool Big = false;
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
                    if (returnData.ToString() == "Small")
                    {
                        GetFileDetails();
                        ReceiveFile();
                        returnData = "";
                    }
                    else if (returnData.ToString().Substring(0, 4) == "Info")
                    { GetFileDetails(); }
                    else if (returnData.ToString().Substring(0, 3) == "Big" )
                    {
                        if (Big == false) {
                            String[] elements = Regex.Split(returnData.ToString(), ",");
                            Console.WriteLine(" --> " + elements[0] + " " + elements[1]);
                            nak = Convert.ToInt32(elements[1]);
                            Big = true;
                            returnData = "";
                        }
                    }
                    else if(Big == true)
                    {
                        bytesm = bytesm.Concat(receiveBytes).ToArray();
                        ReceiveFile2(receiveBytes);
                        nak = nak - 1;
                        Console.WriteLine(" --> " + nak);
                        if (nak <= 0)
                        {
                            Console.WriteLine(" ---End--- " );
                            Big = false;
                            fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                            fs.Write(bytesm, 0, bytesm.Length);
                            Process.Start(fs.Name);
                        }
                        returnData = "";
                       // Console.WriteLine(" --> " + returnData.ToString());
                    }
                }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Возникло исключение: " + ex.ToString() + "\n  " + ex.Message);
        }
    }

}