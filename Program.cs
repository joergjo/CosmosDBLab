using System;
using System.Threading.Tasks;

namespace CosmosDBLab
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Lab4.Excercices.QueryAndUpdateAndShowETag();
            Console.WriteLine(">>>>>\tDONE\t<<<<<");
        }
    }
}
