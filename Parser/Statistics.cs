using Alba.CsConsoleFormat;
using System;
using static System.ConsoleColor;


namespace Parser
{
    internal class Statistics
    {
        private static System.DateTime dateStart;
        public int TaskGetOrders { get; set; }
        public int TaskSendOrders { get; set; }
        public Statistics()
        {
            TaskGetOrders = 0;
            TaskSendOrders = 0;
            dateStart = DateTime.Now;
        }

        public void Show()
        {
            System.TimeSpan date = DateTime.Now - dateStart;
            int proxyCount;
            lock (Program.ProxyList) proxyCount = Program.ProxyList.Count;
            int numberTasks;
            lock (Program.ImeiList) numberTasks = Program.ImeiList.Count;
            int processedOrders;
            lock (Program.OrdersList) processedOrders = Program.OrdersList.Count;
            int wrongCount;
            lock (Program.WrongList) wrongCount = Program.WrongList.Count;
            int threadCount = MainThread.GetRunThread();

            var headerThickness = new LineThickness(LineWidth.Double, LineWidth.Single);
            var doc = new Document(
                new Grid
                {
                    Color = Gray,
                    Columns = { GridLength.Auto, GridLength.Auto },
                    Children =
                    {
                            new Cell("Name") { Stroke = headerThickness },
                            new Cell("Result") { Stroke = headerThickness },
                            new Cell("Number of Tasks"), new Cell(numberTasks.ToString()) { Color = Yellow } ,
                            new Cell("Number of Socks"), new Cell(proxyCount.ToString()) { Color = Yellow },
                            new Cell("Start time"), new Cell(dateStart.ToString()) { Color = Yellow } ,
                            new Cell("Time has Passed"), new Cell(date.ToString(@"hh\:mm\:ss")) { Color = Green },
                            new Cell("Number of processed Orders "), new Cell(processedOrders.ToString()) { Color = Green } ,
                            new Cell("Number of processed Wrong "), new Cell(wrongCount.ToString()) { Color = Green } ,
                            new Cell("Number of active Threads"), new Cell(threadCount.ToString()) { Color = Green},
                            new Cell("Error Count"), new Cell(Loger.GetErorCount().ToString()) { Color = Red} ,
                            new Cell("Total Received"), new Cell(TaskGetOrders.ToString()) {  Color = Cyan},
                            new Cell("Total Sent"), new Cell(TaskSendOrders.ToString()) { Color = Cyan}

                    }
                });

            Console.WriteLine();
            ConsoleRenderer.RenderDocument(doc);
        }
    }
}


