using System.Collections.Generic;
using System.Threading;

namespace Parser
{
    // Основной тред. Создает потоки
    internal class MainThread
    {
        public static List<Thread> ThreadList = new List<Thread>();

        // получение заданий. Adminka должен быть создан
        private static void GetTasks()
        {
            Adminka adminka = new Adminka((string)Program.setting.dynamic.LoginAdmin, (string)Program.setting.dynamic.PassAdmin);

            while (true)
            {
                int count = 0;
                bool b = false;

                for (int i = 0; i < 5; i++)
                {
                    if ((b = adminka.Signin())) break;
                    Thread.Sleep(5000);
                }

                if (!b)
                {
                    Loger.Error("Error Signin. Sleep 1 min");
                    Thread.Sleep(60000);
                    continue;
                }

                for (int i = 0; i < 5; i++)
                {
                    count = adminka.GetInProcess((int)Program.setting.dynamic.CountGetTask);
                    if (count != -1)
                    {
                        lock (Program.stats)
                            Program.stats.TaskGetOrders += count;
                        break;
                    }
                    Thread.Sleep(5000);
                }

                if (count > 0) return;

                for (int i = 0; i < 5; i++)
                {
                    count = adminka.GetNew((int)Program.setting.dynamic.CountGetTask);
                    if (count != -1)
                    {
                        lock (Program.stats)
                            Program.stats.TaskGetOrders += count;
                        break;
                    }
                    Thread.Sleep(5000);
                }

                if (count > 0) return;

                if (count == -1)
                {
                    Loger.Error("Failed to get jobs. Sleep 1 min");
                    Thread.Sleep(60000);
                    continue;
                }

                if (count == 0)
                {
                    Loger.Info("No jobs. Sleep 1 min");
                    Thread.Sleep(60000);
                    continue;
                }
            }

        }

        // Отправка заданий
        private static void SendTask()
        {
            Adminka adminka = new Adminka((string)Program.setting.dynamic.LoginAdmin, (string)Program.setting.dynamic.PassAdmin);

            while (true)
            {
                lock (Program.OrdersList) lock (Program.WrongList) if ((Program.OrdersList.Count == 0) && (Program.WrongList.Count == 0)) return;
                bool b = false;

                for (int i = 0; i < 5; i++)
                {
                    if ((b = adminka.Signin())) break;
                    Thread.Sleep(5000);
                }

                if (!b)
                {
                    Loger.Error("Error Signin. Sleep 1 min");
                    Thread.Sleep(60000);
                    continue;
                }

                lock (Program.OrdersList)
                {
                    if (Program.OrdersList.Count > 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            if ((b = adminka.SendOrders()))
                            {
                                lock (Program.stats)
                                    Program.stats.TaskSendOrders += Program.OrdersList.Count;
                                Program.OrdersList.Clear();
                                break;
                            }
                            Loger.Info("SendOrders error, sleep 5 sec");
                            Thread.Sleep(5000);
                        }

                        if (!b)
                        {
                            Loger.Error("Error SendOrders. Sleep 1 min");
                            Thread.Sleep(60000);
                            continue;
                        }
                    }
                    else
                    {
                        Loger.Info("OrdersList.Count = 0");
                    }

                }

                lock (Program.WrongList)
                {
                    if (Program.WrongList.Count > 0)
                    {

                        for (int i = 0; i < 5; i++)
                        {
                            if ((b = adminka.SendWrong()))
                            {
                                lock (Program.stats)
                                    Program.stats.TaskSendOrders += Program.WrongList.Count;
                                Program.WrongList.Clear();
                                break;
                            }
                            Loger.Info("SendWrong error, sleep 5 sec");
                            Thread.Sleep(5000);
                        }

                        if (!b)
                        {
                            Loger.Error("Error SendWrong. Sleep 1 min");
                            Thread.Sleep(60000);
                            continue;
                        }

                    }
                    else
                    {
                        Loger.Info("WrongList.Count = 0");
                        return;
                    }

                    return;
                }

            }

        }

        public static int GetRunThread()
        {
            int result = 0;

            lock (ThreadList)
            {
                for (int i = 0; i < ThreadList.Count; i++)
                {
                    //Console.WriteLine("Thread: {0} : {1}", i, ThreadList[i].ThreadState);
                    if (ThreadList[i].ThreadState != ThreadState.Stopped)
                    {
                        result++;
                    }

                }
                return result;
            }
        }

        public static void Cycle()
        {
            // цикл
            while (true)
            {
                GetTasks();

                int count;
                lock (Program.ImeiList)
                {
                    if ((int)Program.setting.dynamic.CountThread > Program.ImeiList.Count)
                        count = Program.ImeiList.Count;
                    else
                        count = (int)Program.setting.dynamic.CountThread;
                }


                Loger.Info($"Start {count} threads");

                ThreadList.Clear();

                for (int i = 0; i < count; i++)
                {
                    ThreadList.Add(new Thread(Jobs.Go));
                    ThreadList[i].Name = i.ToString();
                    ThreadList[i].Start();
                    Thread.Sleep(20);
                }

                while (true)
                {
                    if (GetRunThread() == 0) break;
                    Thread.Sleep(1000);
                }

                SendTask();
            }
        }

    }
}

