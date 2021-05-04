using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace InterviewTask
{
    class Program
    {     
        public const string FilePath = @"../../../../qbank-export-1-11-2020.txt";
       // public const string FilePath = @"../../qbank-export-1-11-2020.txt";
        public class FinancialDataItem
        {
            public FinancialDataItem()
            {}
            public FinancialDataItem(DateTime date, Double? debit,Double? credit, string paidto, string paidby, string description)
            {
                Date = date;
                Debit = debit;
                Credit = credit;
                Paidto = paidto;
                Paidby = paidby;
                Description = description;
            }
            public DateTime Date { get; set; }
            public Double? Debit { get; set; }
            public Double? Credit { get; set; }
            public string Paidto { get; set; }
            public string Paidby { get; set; }
            public string Description { get; set; }
        }
        public class GroupedPayment
        {
            GroupedPayment()
            { }
            public GroupedPayment(Double? totalpayment, string paidto, int? numberOfPayments)
            {              
               TotalPayment = totalpayment;
               Paidto = paidto;
               NumberOfPayments = numberOfPayments;               
            }          
            public Double? TotalPayment { get; set; }
            public string Paidto { get; set; }
            public int? NumberOfPayments { get; set; }
        }      
        static void Main(string[] args)
        {
     
                List<FinancialDataItem> data = ImportFinancialData(FilePath);
                var creditAmt = data.Sum(x => x.Credit);
                var debitAmt = data.Sum(x => x.Debit);
                Console.WriteLine("CREDITS - TOTAL : $" + creditAmt);
                GenerateReport(data.Where(x => x.Paidby != null).Select(x => x).ToList());
                Console.WriteLine("DEBITS - TOTAL : $" + debitAmt);
                GenerateReport(data.Where(x => x.Paidto != null).Select(x => x).ToList());
                Console.WriteLine("--------------------------------");


                List<FinancialDataItem> possibleDuplicates = GetPossibleDuplicates(data);
                Console.WriteLine("Possible Duplicates");
                GenerateReport(possibleDuplicates);
                Console.WriteLine("--------------------------------");

                List<GroupedPayment> groupedPayments = GetGroupedExpenditures(data);
                Console.WriteLine("Grouped Payments");
                GenerateReportForGroupedPayments(groupedPayments);
                Console.WriteLine("--------------------------------");
                Console.ReadKey();

        }
        public static void GenerateReport(List<FinancialDataItem> data)
        {

            if (data.Any(x => x.Paidby != null))
            {
                data.ForEach(x => Console.WriteLine(x.Date + ": RECIEVED $" + String.Format("{0:n}", x.Credit) + " from " + x.Paidby + " - " + x.Description));
            }
            else
            {
                data.ForEach(x => Console.WriteLine(x.Date + ": PAID $" + String.Format("{0:n}", x.Debit) + " to " + x.Paidto));
            }   
        }
        public static void GenerateReportForGroupedPayments(List<GroupedPayment> data)
        {
            int i = 0;
            data.ForEach(x => Console.WriteLine(++i+":  $"+String.Format("{0:n}", x.TotalPayment)+" in " +x.NumberOfPayments+" payment to " + x.Paidto));
          
        }
            //Task 1 - 
            public static List<FinancialDataItem> ImportFinancialData(string inputPath)
        {
            var financialData = new List<FinancialDataItem>();
            Console.WriteLine("TASK 1:");
            double? valu = null;
           

                var data1 = File.ReadLines(inputPath).Skip(1).Select(x =>
                {
                    var values = x.Split('\t');
                    return new FinancialDataItem
                    {
                        Date = DateTime.Parse(values[0]),
                        Debit = string.IsNullOrWhiteSpace(values[1]) ? valu : Convert.ToDouble(Regex.Replace(values[1], @"[^0-9a-zA-Z.]+", "")),
                        Credit = string.IsNullOrWhiteSpace(values[2]) ? valu : Convert.ToDouble(Regex.Replace(values[2], @"[^0-9a-zA-Z.]+", "")),
                        Paidto = string.IsNullOrWhiteSpace(values[3]) ? null : values[3],
                        Paidby = string.IsNullOrWhiteSpace(values[4]) ? null : values[4],
                        Description = string.IsNullOrWhiteSpace(values[5]) ? null : values[5]
                    };
                }).ToList();
                financialData.AddRange(data1);
                return financialData;

               
        }
        //Task 2 - 
        public static List<DateTime> GetDateTime(List<DateTime> ddata)
        {
            List<DateTime> dt = new List<DateTime>();
            TimeSpan ts = new TimeSpan(0, 0, 0, 10, 0);
            for (int i=0;i<ddata.Count-1;i++)
            {
                TimeSpan val = ddata[i + 1] - ddata[i];
                if (val<ts)
                {
                    dt.Add(ddata[i]);
                    dt.Add(ddata[i+1]);
               }
            }
            return dt;
        }        
        public static List<FinancialDataItem> GetPossibleDuplicates(List<FinancialDataItem> data)
        {
            List<FinancialDataItem> possibleDuplicates = new List<FinancialDataItem>();
            List<DateTime> dt = new List<DateTime>();
            Console.WriteLine("TASK 2:");
            Dictionary<string,List<DateTime>> data2 = data.Where(x => x.Paidto != null).GroupBy(x => new { paid = x.Paidto})
            .Where(grp => grp.Count() > 1).ToDictionary(g => g.Key.paid, g => g.Select(r=>r.Date).ToList());
            data2.Values.ToList().ForEach(x =>
            {
                var listdata = GetDateTime(x);
                dt.AddRange(listdata);
            });
            dt.ForEach(y => possibleDuplicates.Add(data.Where(x => x.Date ==y ).Select(x => x).FirstOrDefault()));
            return possibleDuplicates;
        }
        //Task 3 - 
        public static List<GroupedPayment> GetGroupedExpenditures(List<FinancialDataItem> data)
        {
            List<GroupedPayment> groupedPayments = new List<GroupedPayment>();
            Console.WriteLine("TASK 3:");
            var data1 = data.Where(x => x.Paidto != null).GroupBy(x => x.Paidto) .ToDictionary(y=> y.Key, y=>y.ToList());
            data1.ToList().ForEach(x =>
            {
                groupedPayments.Add(new GroupedPayment(x.Value.Sum(y => y.Debit), x.Key, x.Value.Count));
            });
            return groupedPayments;
        }

    }
}
