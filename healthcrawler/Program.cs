using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace healthcrawler
{
    class Program
    {
        static IWebDriver driver;
        static String rootUrl;
        static void Main(string[] args)
        {
            int gameIndex = int.Parse(args[0]);
            List<String> seatsPrefix = new List<string>();
            for (int i=1;i<args.Length;i++)
            {
                seatsPrefix.Add(args[i]);
            }
            String url = @"https://tixcraft.com/activity/game/19_KarenMok";
            rootUrl = @"https://tixcraft.com";
            int x = 0;
            driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            while (true)
            {
                driver.Navigate().GoToUrl(url);
                if (x==0)
                    Thread.Sleep(1000);
                try {
                    IList<IWebElement> games = driver.FindElements(By.XPath("//input[@class='btn btn-next']"));
                    String gameUrl = games[gameIndex].GetAttribute("data-href");
                    if (gameUrl != "")
                    {
                        GetTickets(gameUrl, seatsPrefix);
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
                Console.WriteLine("{0}\t{1}",x,DateTime.Now);
                x++;
            }
            
        }
        static void GetTickets(string gameUrl, List<String> seatsPrefix)
        {
            
            gameUrl = rootUrl + gameUrl;
            Console.WriteLine(gameUrl);
            driver.Navigate().GoToUrl(gameUrl);
            IList<IWebElement> seats = driver.FindElements(By.XPath("//li[@class='select_form_b']"));
            bool buyedTickets = false;
            foreach(string searPrefix in seatsPrefix)
            {
                foreach (var seat in seats)
                {
                    string seadStr = seat.Text;
                    if (seat.Text.Contains(searPrefix))
                    {
                        try
                        {
                            if (seadStr.Contains("熱賣中") == false)
                            {
                                string retainedTicketStr = seat.Text.Substring(seadStr.IndexOf("剩餘") + 3);
                                int retainedTicket = int.Parse(retainedTicketStr);
                                if (retainedTicket < 4)
                                    continue;
                            }
                            IWebElement aElement = seat.FindElement(By.XPath("a"));
                            Console.WriteLine(aElement.GetAttribute("id"));
                            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                            executor.ExecuteScript("arguments[0].click();", aElement);

                            SelectElement element = new SelectElement(driver.FindElement(By.XPath("//select[@class='mobile-select']")));
                            element.SelectByValue("4");

                            driver.FindElement(By.XPath("//input[@id='TicketForm_agree']")).Click();
                            buyedTickets = true;
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Ticket Soldout : {0}", seadStr);
                        }
                        
                        
                    }
                    if (buyedTickets)
                        break;
                    Console.WriteLine("Ticket Soldout : {0}",seadStr);
                }
                if (buyedTickets)
                    break;
            }
            Console.WriteLine("Success");

        }
        
    }

}
