using System; 
using System.Collections.Generic;
using System.Drawing; 
using System.Threading;
using System.Windows.Forms;
using System.IO; 
using System.Net; 
using System.Net.Sockets; 
using NAudio.Wave; 
using AForge.Video.DirectShow; 
using AForge.Video;
using System.Drawing.Imaging; 
using System.Text; 

namespace Client_GUI
{
    // прод консюмер для одного
    //дек
    // Основная форма клиента для связи с сервером
    public partial class Client : Form
    {
        // Статус подключения к серверу
        public static bool serverConnect = false;

        // Текущее состояние звонка (звонок активен или нет)
        private bool isCalling = false;

        // IP-адрес сервера, к которому будет происходить подключение
        private string serverIp = "192.168.56.1";

        private static int urgentOutputPort = 13004; // Порт для срочных сообщений

        // Порты, используемые клиентом для связи с сервером:
        private static int messageOutputPort = 13000; // Порт для текстовых сообщений
        private static int videoOutputPort = 13002;   // Порт для видеопотока
        private static int audioOutputPort = 13003;   // Порт для аудиопотока

        // Порт, который слушает клиент (для получения данных от сервера)
        private static int messageInputPort = 13001;

        // Добавьте в секцию подключений
        private TcpClient urgentOutputClient;
        private NetworkStream urgentOutputStream;

        // Сервер, который принимает входящие соединения (например, от сервера)
        private TcpListener messageOutputServer;

        // TCP-клиент для отправки сообщений на сервер
        private TcpClient messageOutputClient;
        private NetworkStream messageOutputStream; // Поток для передачи сообщений

        // TCP-клиент для отправки видео на сервер
        private TcpClient videoOutputClient;
        private NetworkStream videoOutputStream; // Поток для передачи видео

        // TCP-клиент для отправки аудио на сервер
        private TcpClient audioOutputClient;
        private NetworkStream audioOutputStream; // Поток для передачи аудио

        // Источник видео (камера)
        private VideoCaptureDevice videoSource;

        // Список доступных камер
        private FilterInfoCollection videoDevices;

        // Объект для захвата аудио с микрофона
        private WaveIn waveIn;

        // История переписки чата
        public List<string> chat_history = new List<string>();

        // Конструктор формы клиента
        public Client()
        {
            InitializeComponent(); // Инициализация всех элементов интерфейса

            // Пытаемся подключиться к серверу при запуске клиента
            serverConnect = ConnectToServer();

            // Запускаем прослушивание входящих сообщений в отдельном потоке
            Thread thread = new Thread(StartListen) { IsBackground = true };
            thread.Start();
        }
        /// <summary>
        /// Метод StartListen() — ожидает входящие сообщения от сервера.
        /// </summary>
        public void StartListen()
        {
            try
            {
                // Буфер для чтения данных из сети
                Byte[] bytes = new Byte[256];

                // Переменная для хранения полученного сообщения
                String data = null;

                while (true)
                {
                    // Ожидаем входящее TCP-соединение
                    TcpClient client = messageOutputServer.AcceptTcpClient();
                    data = null;

                    // Получаем поток для чтения данных
                    NetworkStream stream = client.GetStream();
                    // Переменная для количества прочитанных байт
                    int i;

                    // Читаем данные до тех пор, пока не закончатся
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Преобразуем байты в строку
                        data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);
                        // Отображаем сообщение в UI
                        LeftLabel(data);
                        // Добавляем в историю чата
                        chat_history.Add("[Server]: " + data);
                    }
                    // Закрываем текущее соединение после завершения обмена
                    client.Close();
                }
            }
            catch (SocketException c)
            {
                // Логируем исключение, если произошла ошибка сети
                Console.WriteLine("SocketException: {0}", c);
            }
            finally
            {
                // Останавливаем сервер (messageOutputServer), если он был запущен
                messageOutputServer.Stop();
            }
        }
        /// <summary>
        /// Соединение с сервером
        /// </summary>
        /// <returns>True если удачно</returns>
        private bool ConnectToServer()
        {
            try
            {
                ConnectMessageInputPort();  // Клиент слушает порт для получения сообщений
                ConnectMessageOutputPort(); // Клиент отправляет сообщения через этот порт
                ConnectOutputAudioPort();   // Аудио
                ConnectOutputVideoPort();   // Видео
                ConnectOutputUrgentPort();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
                return false;
            }
        }
        
        private void ConnectOutputUrgentPort()
        {
            urgentOutputClient = new TcpClient(serverIp, urgentOutputPort);
            urgentOutputStream = urgentOutputClient.GetStream();

            // Отправка IP клиента при подключении
            string ipMessage = "IP:" + GetLocalIPAddress();
            byte[] data = Encoding.UTF8.GetBytes(ipMessage);
            urgentOutputStream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// Соединяемся с портом для получения сообщений
        /// </summary>
        private void ConnectMessageInputPort()
        {
            // Указываем, что сервер будет принимать подключения на всех сетевых интерфейсах (0.0.0.0)
            IPAddress localAddr = IPAddress.Any;

            // Получаем локальный IP-адрес клиента и слушаем messageInputPort (13001)
            messageOutputServer = new TcpListener(IPAddress.Parse(GetLocalIPAddress()), messageInputPort);

            // Запускаем TCP-сервер на этом порту
            messageOutputServer.Start();
        }
        /// <summary>
        /// Соединяемся с портом для передачи сообщений
        /// </summary>
        private void ConnectMessageOutputPort()
        {
            // Создаём клиентское TCP-соединение с сервером по указанному IP и порту (13000)
            messageOutputClient = new TcpClient(serverIp, messageOutputPort);

            // Получаем поток для обмена данными
            messageOutputStream = messageOutputClient.GetStream();
        }
        /// <summary>
        /// Соединяемся с портом для передачи аудиопотока
        /// </summary>
        private void ConnectOutputAudioPort()
        {
            // Подключаемся к серверу по TCP для отправки аудио
            audioOutputClient = new TcpClient(serverIp, audioOutputPort);

            // Получаем поток для передачи аудиоданных
            audioOutputStream = audioOutputClient.GetStream();
        }
        /// <summary>
        /// Соединяемся с портом для передачи видеопотока
        /// </summary>
        private void ConnectOutputVideoPort()
        {
            // Подключаемся к серверу по TCP для передачи видеопотока
            videoOutputClient = new TcpClient(serverIp, videoOutputPort);

            // Получаем поток для отправки видео
            videoOutputStream = videoOutputClient.GetStream();
        }
        /// <summary>
        /// получаем локальный IP
        /// </summary>
        /// <returns>Строку IP</returns>
        /// <exception cref="Exception"></exception>
        public static string GetLocalIPAddress()
        {
            // Получаем хост и список его IP-адресов
            var host = Dns.GetHostEntry(Dns.GetHostName());

            // Перебираем все IP-адреса у этой машины
            foreach (var ip in host.AddressList)
            {
                // Возвращаем первый попавшийся IPv4-адрес
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); // Например: "192.168.1.5"
                }
            }

            // Если нет IPv4-адреса — выбрасываем исключение
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        /// <summary>
        /// Отправка сообщения
        /// </summary>
        /// <param name="clientStream">Поток</param>
        /// <param name="message">Сообщение</param>
        void SendMessage(NetworkStream clientStream, String message)
        {
            // Преобразуем строку в байты с использованием UTF-8
            Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

            // Отправляем данные по TCP-потоку
            clientStream.Write(data, 0, data.Length);
        }
        /// <summary>
        /// Обработчик отправки сообщения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendButton_Click(object sender, EventArgs e)
        {
            // Проверяем, установлено ли соединение
            if (serverConnect)
            {
                // Берём текст из поля ввода
                string input = ClientTextBox.Text;

                // Проверяем, что поле не пустое
                if (input != "")
                {
                    // Добавляем сообщение в историю чата
                    chat_history.Add("[Client ]: " + input);

                    // Отображаем сообщение как исходящее (зелёный цвет)
                    RightLabel(input);

                    // Очищаем поле ввода
                    ClientTextBox.Clear();

                    // Отправляем сообщение на сервер
                    SendMessage(messageOutputStream, input);
                }
            }
            else
            {
                // Если нет подключения — показываем предупреждение
                MessageBox.Show("No connection, make sure to press connect.");
            }
        }

        private void RightLabel(string x)
        {
            // Текстовое содержимое сообщения
            string input = x;

            // Создаём новую метку (Label), которая будет отображать сообщение
            Label MyLabel = new Label();

            // Добавляем её в коллекцию контролов формы (чтобы она существовала)
            this.Controls.Add(MyLabel);

            // Добавляем метку в контейнер flowLayoutPanel1
            flowLayoutPanel1.Controls.Add(MyLabel);

            // Задаём текст метки
            MyLabel.Text = input;

            // Цвет фона — зелёный (для исходящих сообщений)
            MyLabel.BackColor = Color.Green;

            // Цвет текста — белый
            MyLabel.ForeColor = Color.White;

            // Рамка вокруг метки
            MyLabel.BorderStyle = BorderStyle.FixedSingle;

            // Автоматически изменяем размер метки по содержимому
            MyLabel.AutoSize = true;

            // Задаём отступы от других элементов (пока не очень корректно)
            MyLabel.Margin = new Padding(flowLayoutPanel1.Width - MyLabel.Right, 0, 0, 4);
        }

        private void LeftLabel(string data)
        {
            // Входящее сообщение
            string input = data;

            // Создаём новую метку для отображения
            Label ServerLabel = new Label();

            // Так как мы работаем с UI, используем Invoke, чтобы обновить форму из другого потока
            Invoke(new Action(() =>
            {
                // Добавляем метку в коллекцию контролов формы
                this.Controls.Add(ServerLabel);

                // Добавляем метку в контейнер чата
                flowLayoutPanel1.Controls.Add(ServerLabel);
            }));

            // Выполняем настройку метки в UI-потоке
            ServerLabel.BeginInvoke(new Action(() =>
            {
                // Устанавливаем текст
                ServerLabel.Text = input;

                // Фон красный (входящие сообщения)
                ServerLabel.BackColor = Color.Red;

                // Текст белый
                ServerLabel.ForeColor = Color.White;

                // Граница вокруг метки
                ServerLabel.BorderStyle = BorderStyle.FixedSingle;

                // Отступы
                ServerLabel.Margin = new Padding(0, 0, 0, 4);

                // Размер метки зависит от содержимого
                ServerLabel.AutoSize = true;
            }));
        }
        /// <summary>
        /// Обработчик события кнопки видеозвонка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoCallButton_Click(object sender, EventArgs e)
        {
            if (!isCalling)
            {
                try
                {
                    // Пересоздаем подключения
                    ConnectOutputAudioPort();
                    ConnectOutputVideoPort();

                    // Включаем возможность получения OOB-данных
                    if (audioOutputClient != null && audioOutputClient.Connected)
                    {
                        // Устанавливаем флаг, чтобы получать срочные данные inline
                        audioOutputClient.Client.SetSocketOption(
                            SocketOptionLevel.Socket,          // Уровень сокета
                            SocketOptionName.OutOfBandInline,  // Получать OOB как обычные данные
                            true                             // Включить
                                );
                    }

                    // Запускаем захват видео и аудио
                    StartVideo(); // Захват видео
                    StartAudio(); // Захват аудио

                    // Обновляем кнопку UI, чтобы показать, что мы в режиме звонка
                    VideoCallButton.Invoke((MethodInvoker)delegate {
                        VideoCallButton.Text = "Завершить";
                        VideoCallButton.BackColor = Color.Red;
                    });

                    isCalling = true;// Устанавливаем флаг активного звонка
                }
                catch (Exception ex)
                {
                    // Ловим любые ошибки при запуске звонка
                    MessageBox.Show("Ошибка подключения: " + ex.Message);
                }
            }
            else
            {
                SendUrgentMessage("CALL_ENDED");
                // Отправляем сигнал о завершении
                if (messageOutputStream != null && messageOutputClient?.Connected == true)
                {
                    byte[] exitMessage = Encoding.UTF8.GetBytes("ТРАНСЛЯЦИЯ ОКОНЧЕНА");
                    messageOutputStream.Write(exitMessage, 0, exitMessage.Length);
                }
                // Останавливаем передачу видео и аудио
                StopVideo();
                StopAudio();

                // Обновляем UI
                VideoCallButton.Invoke((MethodInvoker)delegate {
                    VideoCallButton.Text = "Трансляция";
                    VideoCallButton.BackColor = SystemColors.Control;
                });

                isCalling = false;// Сбрасываем флаг звонка
            }
        }
        /// <summary>
        /// метод для отправки срочных сообщений
        /// </summary>
        /// <param name="message">Срочное сообщение</param>
        private void SendUrgentMessage(string message)
        {
            try
            {
                if (urgentOutputStream != null && urgentOutputClient?.Connected == true)
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    urgentOutputClient.Client.Send(data, 0, data.Length, SocketFlags.OutOfBand);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Urgent message error: {ex.Message}");
            }
        }
        /// <summary>
        /// Остановка видеопотока
        /// </summary>
        private void StopVideo()
        {
            try
            {
                // Проверяем, есть ли активный источник видео
                if (videoSource != null && videoSource.IsRunning)
                {
                    // Корректно останавливаем камеру
                    videoSource.SignalToStop();   // Отправляем сигнал остановки
                    videoSource.WaitForStop();    // Ждём завершения работы
                    videoSource = null;          // Очищаем ссылку
                }

                // Закрываем TCP-соединение для видео
                videoOutputClient?.Client?.Shutdown(SocketShutdown.Both); // Завершаем обмен данными
                videoOutputClient?.Close();     // Закрываем клиентское соединение
                videoOutputClient = null;       // Очищаем ссылку на клиента
                videoOutputStream = null;      // Очищаем поток
                                               // Очищаем изображение
                if (PictureBoxLocalVideo.InvokeRequired)
                {
                    PictureBoxLocalVideo.BeginInvoke((MethodInvoker)delegate {
                        PictureBoxLocalVideo.Image?.Dispose();
                        PictureBoxLocalVideo.Image = null;
                    });
                }
                else
                {
                    PictureBoxLocalVideo.Image?.Dispose();
                    PictureBoxLocalVideo.Image = null;
                }
            }
            catch (Exception ex)
            {
                // Ловим ошибки при остановке камеры
                MessageBox.Show("Ошибка камеры при завершении звонка: " + ex.Message);
            }
        }
        /// <summary>
        /// Остановка аудиопотока
        /// </summary>
        private void StopAudio()
        {
            try
            {
                // Проверяем, есть ли активный микрофон
                if (waveIn != null)
                {
                    waveIn.StopRecording(); // Останавливаем запись
                    waveIn.Dispose();        // Освобождаем ресурсы
                    waveIn = null;           // Очищаем ссылку
                }

                // Закрываем TCP-соединение для аудио
                audioOutputClient?.Client?.Shutdown(SocketShutdown.Both); // Прекращаем обмен
                audioOutputClient?.Close();   // Закрываем клиент
                audioOutputClient = null;     // Очищаем ссылку
                audioOutputStream = null;     // Очищаем поток
            }
            catch (Exception ex)
            {
                // Ловим ошибки при остановке микрофона
                MessageBox.Show("Ошибка микрофона при завершении звонка: " + ex.Message);
            }
        }
        /// <summary>
        /// Запуск видеопотока
        /// </summary>
        private void StartVideo()
        {
            // === Инициализация видео ===
            // Получаем список доступных видеокамер
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            // Если камер нет — выводим сообщение
            if (videoDevices.Count == 0)
            {
                MessageBox.Show("Нет доступных видеокамер.");
                return;
            }

            // Создаем устройство захвата с первой камеры
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            // Подписываемся на событие нового кадра
            videoSource.NewFrame += VideoSource_NewFrame;

            // Начинаем захват видео
            videoSource.Start();
        }
        /// <summary>
        /// Запуск аудиопотока
        /// </summary>
        private void StartAudio()
        {
            // === Инициализация аудио ===
            waveIn = new WaveIn();                 // Создаём объект записи
            waveIn.DeviceNumber = 0;               // Используем первый микрофон
            waveIn.WaveFormat = new WaveFormat(8000, 16, 1); // Формат: 8kHz, 16-bit, моно
            waveIn.DataAvailable += WaveIn_DataAvailable; // Подписываемся на событие данных
            waveIn.StartRecording();               // Начинаем запись
        }
        /// <summary>
        /// Jбработка каждого нового кадра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (videoOutputStream == null || !videoOutputClient.Connected)
                return;

            // Создаем два отдельных клона для разных целей
            Bitmap uiFrame = null;
            Bitmap sendingFrame = null;

            try
            {
                using (var sourceFrame = (Bitmap)eventArgs.Frame.Clone())
                {
                    // Клон для UI
                    uiFrame = (Bitmap)sourceFrame.Clone();

                    // Клон для отправки
                    sendingFrame = (Bitmap)sourceFrame.Clone();
                }

                // Обновление UI
                UpdatePictureBox(uiFrame);

                // Отправка кадра
                SendVideoFrame(sendingFrame);
            }
            finally
            {
                sendingFrame?.Dispose();
            }
        }

        private void UpdatePictureBox(Bitmap frame)
        {
            if (PictureBoxLocalVideo.InvokeRequired)
            {
                PictureBoxLocalVideo.BeginInvoke((MethodInvoker)delegate
                {
                    SafeUpdatePictureBox(frame);
                });
            }
            else
            {
                SafeUpdatePictureBox(frame);
            }
        }

        private void SafeUpdatePictureBox(Bitmap newFrame)
        {
            var oldImage = PictureBoxLocalVideo.Image;

            try
            {
                PictureBoxLocalVideo.Image = (Bitmap)newFrame.Clone();
                PictureBoxLocalVideo.SizeMode = PictureBoxSizeMode.Zoom;
            }
            finally
            {
                oldImage?.Dispose();
            }
        }

        private void SendVideoFrame(Bitmap frame)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    frame.Save(ms, ImageFormat.Jpeg);
                    byte[] frameBytes = ms.ToArray();
                    byte[] length = BitConverter.GetBytes(frameBytes.Length);

                    videoOutputStream.Write(length, 0, 4);
                    videoOutputStream.Write(frameBytes, 0, frameBytes.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Send error: {ex.Message}");
            }
        }
        /// <summary>
        /// Обработчик волны аудио
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (audioOutputClient == null || !audioOutputClient.Connected) return;

            // Отправляем как срочные данные
            audioOutputClient.Client.Send(e.Buffer, 0, e.BytesRecorded, SocketFlags.OutOfBand);
        }
        /// <summary>
        /// Обработчик кнопки экспорта в файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportButton_Click(object sender, EventArgs e)
        {
            Stream myStream; // Поток для сохранения файла
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            // Устанавливаем фильтр: только .txt файлы
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt";

            // По умолчанию выбираем txt
            saveFileDialog1.FilterIndex = 2;

            // Восстанавливаем последнюю директорию
            saveFileDialog1.RestoreDirectory = true;

            // Открываем диалог выбора файла
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Открываем поток для записи
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    // Создаем писатель текста
                    TextWriter txt = new StreamWriter(myStream);

                    // Записываем каждую строку из истории чата
                    foreach (string s in chat_history)
                    {
                        txt.Write(s + "\n"); // Добавляем перевод строки
                    }

                    // Закрываем писатель
                    txt.Close();

                    // Закрываем поток
                    myStream.Close();
                }
            }
        }
        /// <summary>
        /// Обработчик закрытия формы
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                SendUrgentMessage("APP_CLOSED");

                // Закрываем все сетевые потоки и клиенты
                messageOutputStream?.Close();
                messageOutputClient?.Close();

                videoOutputStream?.Close();
                videoOutputClient?.Close();

                audioOutputStream?.Close();
                audioOutputClient?.Close();
            }
            catch
            {
                // Игнорируем ошибки при завершении
            }
            finally
            {
                urgentOutputStream?.Close();
                urgentOutputClient?.Close();
                // Вызываем базовый метод OnFormClosed
                base.OnFormClosed(e);
            }
        }
    }
}
