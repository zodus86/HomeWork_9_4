using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace HomeWork_9_4
{
    class Program
    {
        static TelegramBotClient bot;
        static string rootPath = "D:\\temp\\";

        static void Main(string[] args)
        {

            string token = File.ReadAllText("D:\\token");

            bot = new TelegramBotClient(token);
            bot.OnMessage += MessageListener;
            bot.StartReceiving();
            Console.ReadKey();
        }

        private static void MessageListener(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            string text = $"{DateTime.Now.ToLongTimeString()}: {e.Message.Chat.FirstName} {e.Message.Chat.Id} {e.Message.Text}";

            Console.WriteLine($"{text} TypeMessage: {e.Message.Type.ToString()}");
            string messageText = "";

            if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document)
            {
                Console.WriteLine(e.Message.Document.FileId);
                Console.WriteLine(e.Message.Document.FileName);
                Console.WriteLine(e.Message.Document.FileSize);

                DownLoad(e.Message.Document.FileId, e.Message.Document.FileName);
            }
           
            if (e.Message.Text == null) return;

            if (e.Message.Text == "/start")
            {
                messageText = "Вас приветсвует секретный бот облако" +
                    "\nотправляйте мне свои документы и файлы я буду их хранить" +
                    "\nСписок доступных комманд:" +
                    "\n/seeFiles" +
                    "\nнапишите Имя файла для скачки";
            }else if(e.Message.Text == "/seeFiles")
            {
                messageText = GetDir();
            }else if(File.Exists(rootPath + e.Message.Text))
            {
                UpLoad(e.Message.Text , e.Message.Chat.Id);
            }else if (String.IsNullOrEmpty(messageText))
            { 
                messageText = e.Message.Text + " - файл отсутсвует в базе данных";
            }

            bot.SendTextMessageAsync(e.Message.Chat.Id,
                $"{messageText}"
                );
        }
        
        /// <summary>
        /// выгрузить файл
        /// </summary>
        /// <param name="file"></param>
        static async void UpLoad(string file, long userID)
        {
            using (FileStream stream = File.Open(rootPath + file , FileMode.Open))
            {
                InputOnlineFile iof = new InputOnlineFile(stream);
                iof.FileName = file;
                var send = await bot.SendDocumentAsync(userID, iof, "Вот то что вы просили!");
            }
        }

        
        /// <summary>
        /// скачать файл
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="path"></param>
        static async void DownLoad(string fileId, string path)
        {
            try
            {
                var file = await bot.GetFileAsync(fileId);
                FileStream fs = new FileStream(rootPath + path, FileMode.Create);
                await bot.DownloadFileAsync(file.FilePath, fs);
                fs.Close();
                fs.Dispose();
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex);
            }

        }
    

        /// <summary>
        /// Получение всех файлов 
        /// </summary>
        /// <param name="path">Путь к каталогу</param>
        /// <param name="trim">Количество отступов</param>
        static string GetDir()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(rootPath); 
            StringBuilder text = new StringBuilder();

            foreach (var item in directoryInfo.GetFiles())         
            {
                text.Append($"\n{item.Name}  размер файла {item.Length}.");            
            }

            return text.ToString();
        }

    }
}
