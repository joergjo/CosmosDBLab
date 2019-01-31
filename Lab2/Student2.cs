namespace CosmosDBLab.Lab2
{
    public class Student2
    {
        public string studentAlias { get; set; }
        public int age { get; set; }
        public int enrollmentYear { get; set; }
        public int projectedGraduationYear { get; set; }
        public FinancialInfo financialData { get; set; }

        public class FinancialInfo
        {
            public double tuitionBalance { get; set; }
        }
    }
    
}