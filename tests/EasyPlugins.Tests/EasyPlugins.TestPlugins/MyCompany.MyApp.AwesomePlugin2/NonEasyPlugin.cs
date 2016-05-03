using Dapper;
using ServiceStack.Text;

namespace MyCompany.MyApp.AwesomePlugin2
{
    public class NonEasyPlugin
    {
        public NonEasyPlugin()
        {
            // use ServiceStack.Text
            SomeProperty1 = (new {Test = "ABC"}).ToJson();

            // use Newtonsoft.Json
            SomeProperty2 = Newtonsoft.Json.JsonConvert.SerializeObject((new {Test = "ABC"}));

            // Dapper sql
            SomeProperty3 = Dapper.CommandFlags.NoCache;
        }

        public CommandFlags SomeProperty3 { get; set; }

        public string SomeProperty2 { get; set; }

        public string SomeProperty1 { get; set; }
    }
}