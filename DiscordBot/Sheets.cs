using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace DiscordBot
{
    class Sheets
    {

        private static string sheetID = Config.GoogleData.SpreadsheetID;
        private static string range = Config.GoogleData.Range;

        private static SheetsService service;

        public static void PullInitialSheet()
        {
            service = new SheetsService(new BaseClientService.Initializer()
            {
                ApiKey = Config.GoogleData.APIKey,
                ApplicationName = "Form2Role Bot"
            });

            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(sheetID, range);

            ValueRange responses = request.Execute();
            IList<IList<Object>> values = responses.Values;

            

            if (values != null && values.Count > 0)
            {
                
            }
        }

        public static void CheckSheets()
        {
            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(sheetID, range);

            ValueRange responses = request.Execute();
            IList<IList<Object>> values = responses.Values;

            if (values != null && values.Count > 0)
            {

            }
        }

    }
}
