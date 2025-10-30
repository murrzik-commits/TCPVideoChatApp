using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Linq;


namespace Server_GUI
{
    public partial class Server : Form
    {
        private Dictionary<Guid, string> connectedClients = new Dictionary<Guid, string>();
        private readonly object clientsLock = new object();
        // IP клиента
        private string clientIP = "192.168.43.224";

        // Флаг состояния подключения
        private bool ConnectButton_clicked = false;

        // История чата для логгирования
        public List<string> chat_history = new List<string>();

        // Порт для приёма текстовых сообщений
        private static int messageInputPort = 13000;
        // Порт для приёма видеопотока
        private static int videoInputPort = 13002;
        // Порт для приёма аудиопотока
        private static int audioInputPort = 13003;
        // Порт для управления важными сигналами и получения IP
        private static int urgentInputPort = 13004;

        // Порт для отправки сообщений
        private static int messageOutputPort = 13001;


        public Server()
        {
            InitializeComponent(); // Инициализация компонентов формы
            listBoxClients.HorizontalScrollbar = true;
            Thread urgentThread = new Thread(StartUrgentServer)
            {
                IsBackground = true
            };
            urgentThread.Start();
            // Запуск сервера для текстовых сообщений в отдельном потоке
            Thread messageThread = new Thread(startMessageServer)
            {
                IsBackground = true, // Фоновый поток
            };
            messageThread.Start();

            // Запуск сервера для видео
            Thread videoThread = new Thread(StartVideoServer)
            {
                IsBackground = true
            };
            videoThread.Start();

            // Запуск сервера для аудио
            Thread audioThread = new Thread(StartAudioServer)
            {
                IsBackground = true
            };
            audioThread.Start();
        }
        private void StartUrgentServer()
        {
            var listener = new TcpListener(IPAddress.Any, urgentInputPort);
            listener.Start();

            while (true)
            {
                var client = listener.AcceptTcpClient();
                _ = HandleUrgentClient(client);
            }
        }
        private async Task HandleUrgentClient(TcpClient client)
        {
            using (client)
            {
                var stream = client.GetStream();
                byte[] buffer = new byte[256];

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessUrgentMessage(message, client); // Передаем client в метод
                }
            }
        }
        private void ProcessUrgentMessage(string message, TcpClient client)
        {
            if (message.StartsWith("IP:"))
            {
                string ip = message.Substring(3);
                Guid clientId = Guid.NewGuid();

                lock (clientsLock)
                {
                    connectedClients[clientId] = ip;
                }

                UpdateClientList();

                // Запускаем таймер для проверки получения IP
                var timer = new System.Timers.Timer(5000) { AutoReset = false };
                timer.Elapsed += (s, e) => CheckIPReceived(clientId, client);
                timer.Start();
            }
            else switch (message)
                {
                    case "APP_CLOSED":
                        RemoveClientByIp(GetClientIp(client)); // Реализуем метод получения IP
                        break;
                    case "CALL_ENDED":
                        ResetCallUI();
                        break;
                }
        }
        private string GetClientIp(TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
        }
        private void UpdateClientList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateClientList));
                return;
            }

            listBoxClients.Items.Clear();
            lock (clientsLock)
            {
                int counter = 1;
                foreach (var client in connectedClients)
                {
                    listBoxClients.Items.Add($"{counter}) {client.Value}");
                    counter++;
                }
            }
        }
        private void RemoveClientByIp(string ip)
        {
            lock (clientsLock)
            {
                var key = connectedClients.FirstOrDefault(x => x.Value == ip).Key;
                if (key != Guid.Empty)
                {
                    connectedClients.Remove(key);
                    UpdateClientList();
                }
            }
        }
        private void CheckIPReceived(Guid clientId, TcpClient client)
        {
            lock (clientsLock)
            {
                if (!connectedClients.ContainsKey(clientId))
                {
                    client.Close();
                    Console.WriteLine($"Client {clientId} disconnected - IP not received");
                }
            }
        }

        /// <summary>
        /// Обновление индикатора уровня аудио
        /// </summary>
        /// <param name="level">Уровень громкости от 0 до 1</param>
        private void UpdateAudioLevel(float level)
        {
            // Проверка необходимости синхронизации с UI-потоком
            if (audioLevelPanel.InvokeRequired)
            {
                // Рекурсивный вызов через делегат
                audioLevelPanel.Invoke(new Action<float>(UpdateAudioLevel), level);
            }
            else
            {
                // Рассчёт высоты индикатора
                int maxHeight = audioLevelPanel.Height;
                int currentHeight = (int)(maxHeight * level);

                // Отрисовка индикатора
                using (Graphics g = audioLevelPanel.CreateGraphics())
                {
                    g.Clear(SystemColors.Control); // Очистка предыдущего состояния
                    using (Brush b = new SolidBrush(level > 0.8f ? Color.Red : Color.Green))
                    {
                        // Рисуем прямоугольник-индикатор
                        Rectangle rect = new Rectangle(
                            0, 
                            maxHeight - currentHeight, // Позиция по Y
                            audioLevelPanel.Width, 
                            currentHeight
                        );
                        g.FillRectangle(b, rect);
                    }
                }
            }
        }
        /// <summary>
        /// Расчёт уровня громкости из аудиобуфера
        /// </summary>
        /// <param name="buffer">Буфер с аудиоданными</param>
        /// <param name="bytesRead">Количество считанных байт</param>
        /// <returns>Уровень громкости от 0 до 1</returns>
        private float CalculateVolumeLevel(byte[] buffer, int bytesRead)
        {
            double sum = 0;
            // Обработка 16-битных сэмплов (по 2 байта)
            for (int i = 0; i < bytesRead; i += 2)
            {
                // Чтение 16-битного сэмпла
                short sample = BitConverter.ToInt16(buffer, i);
                sum += sample * sample; // Сумма квадратов
            }

            // Расчёт RMS (Root Mean Square)
            double rms = Math.Sqrt(sum / (bytesRead / 2));
            return (float)(rms / short.MaxValue); // Нормализация
        }
        /// <summary>
        /// Запуск сервера для видеопотока
        /// </summary>
        private void StartVideoServer()
        {
            // Создание TCP-слушателя для всех интерфейсов
            var listener = new TcpListener(IPAddress.Any, videoInputPort);
            listener.Start(); // Старт прослушивания

            // Цикл обработки подключений
            while (true)
            {
                // Принятие нового клиента
                var client = listener.AcceptTcpClient();
                // Асинхронная обработка клиента
                _ = HandleVideo(client);
            }
        }

        /// <summary>
        /// Запуск сервера аудиопотока
        /// </summary>
        private void StartAudioServer()
        {
            // Привязываем порт для прослушивания
            var listener = new TcpListener(IPAddress.Any, audioInputPort);
            listener.Start();
            while (true)
            {
                // Принимает запрос на подключение
                var client = listener.AcceptTcpClient();
                // Асинхронная обработка аудиопотока
                _ = HandleUrgentAudio(client);
            }
        }
        /// <summary>
        /// Обработка видеопотока от клиента
        /// </summary>
        /// <param name="client">TCP-клиент</param>
        private async Task HandleVideo(TcpClient client)
        {
            var stream = client.GetStream();// Получение сетевого потока
            byte[] buffer = new byte[4];// Буфер для размера кадра

            while (true)
            {
                // Чтение размера кадра (4 байта)
                int read = await stream.ReadAsync(buffer, 0, 4);
                if (read != 4) break;// Проверка целостности данных

                // Конвертация размера кадра из байтов
                int length = BitConverter.ToInt32(buffer, 0);
                byte[] frameData = new byte[length];// Буфер для кадра
                int totalRead = 0;

                // Чтение полного кадра
                while (totalRead < length)
                {
                    read = await stream.ReadAsync(frameData, totalRead, length - totalRead);
                    if (read == 0) break; // если не прочитан
                    totalRead += read;
                }

                // Обработка кадра
                try
                {
                    using (var ms = new MemoryStream(frameData))
                    {
                        Bitmap bitmap = new Bitmap(ms); // Создание изображения
                        pictureBoxRemoteVideo.Invoke((MethodInvoker)delegate {
                            pictureBoxRemoteVideo.Image?.Dispose(); // Освобождение предыдущего изображения
                            pictureBoxRemoteVideo.Image = new Bitmap(bitmap);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка отображения кадра: " + ex.Message);
                }
            }

            client.Close();
        }
        /// <summary>
        /// Обработка срочных аудиоданных
        /// </summary>
        /// <param name="client"></param>
        /// <returns>TCP соединение с клиентом</returns>
        private async Task HandleUrgentAudio(TcpClient client)
        {
            // Настройка сокета для inline-обработки срочных данных (OOB)
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.OutOfBandInline, true);

            // Буфер для аудиоданных (1024 байта)
            byte[] buffer = new byte[1024];

            // Цикл приема данных
            while (true)
            {
                try
                {
                    // Асинхронное чтение срочных данных из сокета
                    int received = await client.Client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                    // Логирование информации о полученных данных
                    Console.WriteLine("Получено срочное аудио (" + received + " байт)");

                    // Расчет уровня громкости
                    float level = CalculateVolumeLevel(buffer, received);

                    // Обновление UI индикатора громкости
                    UpdateAudioLevel(level);
                }
                catch (Exception ex)
                {
                    // Обработка ошибок приема данных
                    Console.WriteLine("Ошибка приёма срочного аудио: " + ex.Message);
                    break;// Выход из цикла при ошибке
                }
            }
            // Закрытие соединения с клиентом
            client.Close();
        }
        /// <summary>
        /// Запуск сервера для текстовых сообщений
        /// </summary>
        public void startMessageServer()
        {
            TcpListener server = null;
            try
            {
                // Создание TCP-сервера для всех интерфейсов
                server = new TcpListener(IPAddress.Any, messageInputPort);

                // Старт прослушивания порта
                server.Start();

                // Буфер для данных (256 байт)
                Byte[] bytes = new Byte[256];
                String data = null;

                // Основной цикл работы сервера
                while (true)
                {
                    // Принятие нового подключения
                    TcpClient client = server.AcceptTcpClient();

                    // Обновление UI кнопки подключения
                    DisconnectButton.BeginInvoke(new Action(() =>
                    {
                        DisconnectButton.BackColor = Color.Green;
                        DisconnectButton.Text = "Connected";
                        ConnectButton_clicked = true;
                    }));
                    data = null;// Сброс переменной данных

                    // Получение сетевого потока
                    NetworkStream stream = client.GetStream();
                    int i;// Счетчик прочитанных байт

                    // Чтение данных из потока
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Конвертация байтов в строку UTF-8
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);

                        // Добавление сообщения в историю
                        chat_history.Add("[Client ]: " + data);
                        
                        // Отображение сообщения в UI
                        LeftLabel(data);
                    }

                    // Закрытие клиентского соединения
                    client.Close();

                    // Сброс состояния интерфейса
                    ResetCallUI();
                }

            }
            catch (SocketException c)
            {
                // Обработка ошибок сокета
                Console.WriteLine("SocketException: {0}", c);
            }
            finally
            {
                // Гарантированная остановка сервера
                server.Stop();
            }
        }

        /// <summary>
        /// Label для отображения сообщения со стороны клиента
        /// </summary>
        /// <param name="data">Сообщение</param>
        private void LeftLabel(string data)
        {
            string input = data;
            Label ServerLabel = new Label(); // Создание новой метки

            // Потокобезопасное добавление в UI
            Invoke(new Action(() =>
            {
                this.Controls.Add(ServerLabel); // Добавление на форму
                flowLayoutPanel1.Controls.Add(ServerLabel); // Добавление в контейнер
            }));

            // Настройка свойств метки
            ServerLabel.BeginInvoke(new Action(() =>
            {
                ServerLabel.Text = input; // Текст сообщения
                ServerLabel.BackColor = Color.Red; // Цвет фона
                ServerLabel.ForeColor = Color.White; // Цвет текста
                ServerLabel.BorderStyle = BorderStyle.FixedSingle; // Граница
                ServerLabel.Margin = new Padding(0, 0, 0, 4); // Отступы
                ServerLabel.AutoSize = true; // Авторазмер
            }));

            // Вывод в консоль для отладки
            Console.WriteLine(data);
        }
        /// <summary>
        /// Получает локальный IPv4 адрес текущего компьютера
        /// </summary>
        /// <returns>IPv4 адрес в виде строки</returns>
        /// <exception cref="Exception">Если IPv4 адреса не найдены</exception>
        public static string GetLocalIPAddress()
        {
            // Получаем информацию о сетевых интерфейсах хоста
            var host = Dns.GetHostEntry(Dns.GetHostName());

            // Перебираем все IP-адреса в списке
            foreach (var ip in host.AddressList)
            {
                // Ищем адреса IPv4 семейства
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); // Возвращаем первый найденный IPv4
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        /// <summary>
        /// Устанавливает TCP-соединение и отправляет сообщение
        /// </summary>
        /// <param name="server">IP-адрес сервера</param>
        /// <param name="message">Текст сообщения для отправки</param>
        static void Connect(String server, String message)
        {
            try
            {
                // Создаем TCP-клиент и подключаемся к серверу
                TcpClient client = new TcpClient(server, messageOutputPort);

                // Выводим сообщение в консоль для отладки
                Console.WriteLine("(You): {0}", message);

                // Конвертируем текст в байты UTF-8
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                // Получаем сетевой поток для записи
                NetworkStream stream = client.GetStream();

                // Отправляем данные через сеть
                stream.Write(data, 0, data.Length);

                // Закрываем поток и соединение
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
        /// <summary>
        /// Обработчик нажатия кнопки отправки сообщения
        /// </summary>
        private void SendButton_Click(object sender, EventArgs e)
        {
            // Проверяем флаг подключения
            if (ConnectButton_clicked)
            {
                // Получаем текст из текстового поля
                string input = ServerTextBox.Text;

                // Проверяем на пустую строку
                if (input != "")
                {
                    // Добавляем сообщение в историю
                    chat_history.Add("[Server]: " + input);

                    // Отображаем сообщение справа
                    RightLabel(input);

                    // Очищаем поле ввода
                    ServerTextBox.Clear();

                    // Отправляем сообщение через TCP
                    Connect(clientIP, input);
                }
            }
            else
            {
                // Показываем ошибку если нет подключения
                MessageBox.Show("No connection, make sure your client is connected to this server.");
            }
        }

        /// <summary>
        /// Создает метку для отображения исходящих сообщений
        /// </summary>
        /// <param name="x">Текст сообщения</param>
        private void RightLabel(string x)
        {
            string input = x;
            Label MyLabel = new Label();  // Создаем новую метку

            // Добавляем элементы в UI потокобезопасно
            this.Controls.Add(MyLabel);          // На форму
            flowLayoutPanel1.Controls.Add(MyLabel); // В контейнер

            // Настраиваем свойства метки
            MyLabel.Text = input;  // Текст сообщения
            MyLabel.BackColor = Color.Green;  // Зеленый фон
            MyLabel.ForeColor = Color.White;     // Белый текст
            MyLabel.BorderStyle = BorderStyle.FixedSingle; // Граница
            MyLabel.Margin = new Padding(  // Правый отступ
                flowLayoutPanel1.Width - MyLabel.Right, 0, 0, 4);
            MyLabel.AutoSize = true;  // Авторазмер
        }
        /// <summary>
        /// Сбрасывает состояние элементов управления
        /// </summary>
        private void ResetCallUI()
        {
            // Проверка необходимости синхронизации с UI потоком
            if (InvokeRequired)
            {
                Invoke(new Action(ResetCallUI));
                return;
            }

            // Обновляем кнопку подключения
            DisconnectButton.Text = "Disconnect";  // Текст
            DisconnectButton.BackColor = Color.Red; // Красный цвет
            ConnectButton_clicked = false; // Сбрасываем флаг

            // Очищаем изображение с видео
            
            pictureBoxRemoteVideo.Image?.Dispose(); // Освобождаем ресурсы
            pictureBoxRemoteVideo.Image = null; // Удаляем ссылку
        }
        /// <summary>
        /// Экспортирует историю чата в текстовый файл
        /// </summary>
        private void ExportButton_Click(object sender, EventArgs e)
        {
            Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            // Настраиваем диалог сохранения
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt"; // Фильтр форматов
            saveFileDialog1.FilterIndex = 2;  // Выбранный фильтр
            saveFileDialog1.RestoreDirectory = true; // Восстановление директории

            // Показываем диалог
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Открываем поток для записи
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Создаем writer и записываем историю
                    TextWriter txt = new StreamWriter(myStream);
                    foreach (string s in chat_history)
                    {
                        txt.Write(s + "\n"); // Добавляем перенос строки
                    }
                    txt.Close(); // Закрываем writer
                    myStream.Close(); // Закрываем поток
                }
            }
        }
    }
}
